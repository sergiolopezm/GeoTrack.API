using GeoTrack.API.Domain.Contracts;
using GeoTrack.API.Shared.GeneralDTO;
using System.Text.Json;

namespace GeoTrack.API.Attributes
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error no manejado");
                await HandleExceptionAsync(context, ex);

                // Registrar en log de base de datos si está disponible
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

                        await logRepository.ErrorAsync(
                            idUsuario != null ? Guid.Parse(idUsuario) : null,
                            context.Connection.RemoteIpAddress?.ToString(),
                            $"{context.Request.Method} {context.Request.Path}",
                            ex.Message);
                    }
                }
                catch (Exception logEx)
                {
                    _logger.LogError(logEx, "Error al registrar en log de base de datos");
                }
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;

            var respuesta = RespuestaDto.ErrorInterno();
            var json = JsonSerializer.Serialize(respuesta);
            await context.Response.WriteAsync(json);
        }
    }
}
