using GeoTrack.API.Domain.Contracts;

namespace GeoTrack.API.Attributes
{
    public class LoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<LoggingMiddleware> _logger;

        public LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            // Registrar inicio de solicitud
            _logger.LogInformation("Iniciando solicitud HTTP {Method} {Path}", context.Request.Method, context.Request.Path);

            // Capturar la respuesta original
            var originalBodyStream = context.Response.Body;
            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            // Continuar con la solicitud
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                await _next(context);
                stopwatch.Stop();

                // Registrar solicitud exitosa
                _logger.LogInformation("Solicitud completada HTTP {Method} {Path} - Estado: {StatusCode} - Tiempo: {ElapsedMilliseconds}ms",
                    context.Request.Method, context.Request.Path, context.Response.StatusCode, stopwatch.ElapsedMilliseconds);

                // Registrar en log de base de datos si es necesario
                await RegistrarLogAsync(context, stopwatch.ElapsedMilliseconds);
            }
            catch
            {
                stopwatch.Stop();
                // La excepción se manejará en el middleware de errores
                throw;
            }
            finally
            {
                // Copiar la respuesta modificada al cuerpo original
                responseBody.Seek(0, SeekOrigin.Begin);
                await responseBody.CopyToAsync(originalBodyStream);
            }
        }

        private async Task RegistrarLogAsync(HttpContext context, long tiempoTranscurrido)
        {
            // Solo registrar en BD solicitudes importantes (no archivos estáticos, etc.)
            if (context.Request.Path.StartsWithSegments("/api") && context.Response.StatusCode != 401)
            {
                try
                {
                    if (context.RequestServices.GetService<ILogRepository>() is ILogRepository logRepository)
                    {
                        string? idUsuario = null;
                        var userClaim = context.User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier);
                        if (userClaim != null && Guid.TryParse(userClaim.Value, out var userId))
                        {
                            idUsuario = userId.ToString();
                        }

                        string tipo = context.Response.StatusCode.ToString();
                        string accion = $"{context.Request.Method} {context.Request.Path}";
                        string detalle = $"Tiempo: {tiempoTranscurrido}ms - Estado: {context.Response.StatusCode}";

                        await logRepository.LogAsync(
                            idUsuario != null ? Guid.Parse(idUsuario) : null,
                            context.Connection.RemoteIpAddress?.ToString(),
                            accion,
                            detalle,
                            tipo);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al registrar log de solicitud");
                }
            }
        }
    }
}
