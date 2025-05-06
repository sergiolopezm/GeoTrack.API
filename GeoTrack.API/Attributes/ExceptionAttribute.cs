using GeoTrack.API.Domain.Contracts;
using GeoTrack.API.Shared.GeneralDTO;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

namespace GeoTrack.API.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class ExceptionAttribute : Attribute, IExceptionFilter
    {
        private readonly ILogRepository _logRepository;
        private readonly ILogger<ExceptionAttribute> _logger;

        public ExceptionAttribute(ILogRepository logRepository, ILogger<ExceptionAttribute> logger)
        {
            _logRepository = logRepository;
            _logger = logger;
        }

        public void OnException(ExceptionContext context)
        {
            _logger.LogError(context.Exception, "Error no controlado: {Message}", context.Exception.Message);

            // Registrar el error en el log de la aplicación
            try
            {
                string? idUsuario = null;
                var userClaim = context.HttpContext.User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userClaim != null && Guid.TryParse(userClaim.Value, out var userId))
                {
                    idUsuario = userId.ToString();
                }

                _logRepository.ErrorAsync(
                    idUsuario != null ? Guid.Parse(idUsuario) : null,
                    context.HttpContext.Connection.RemoteIpAddress?.ToString(),
                    $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}",
                    context.Exception.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar excepción en log de base de datos");
            }

            context.Result = new ObjectResult(RespuestaDto.ErrorInterno())
            {
                StatusCode = 500,
            };

            context.ExceptionHandled = true;
        }
    }
}
