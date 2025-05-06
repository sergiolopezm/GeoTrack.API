using GeoTrack.API.Domain.Contracts;
using Microsoft.AspNetCore.Mvc.Filters;

namespace GeoTrack.API.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class LogAttribute : ActionFilterAttribute
    {
        private readonly ILogRepository _logRepository;

        public LogAttribute(ILogRepository logRepository)
        {
            _logRepository = logRepository;
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // Ejecutar la acción
            var resultContext = await next();

            // Registrar la acción después de ejecutarla
            string idUsuario = context.HttpContext.Request.Headers["IdUsuario"].FirstOrDefault()?.Split(" ").Last();
            string ip = context.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";
            string accion = context.HttpContext.Request.Path.Value ?? string.Empty;
            string tipo = resultContext.HttpContext.Response.StatusCode.ToString();
            string detalle = string.Empty;

            if (resultContext.Result is Microsoft.AspNetCore.Mvc.ObjectResult objectResult &&
                objectResult.Value is GeoTrack.API.Shared.GeneralDTO.RespuestaDto respuesta)
            {
                detalle = respuesta.Detalle ?? respuesta.Mensaje ?? string.Empty;
            }

            await _logRepository.LogAsync(
                idUsuario != null ? Guid.Parse(idUsuario) : null,
                ip,
                accion,
                detalle,
                tipo);
        }
    }
}
