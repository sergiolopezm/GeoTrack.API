using GeoTrack.API.Domain.Contracts;
using GeoTrack.API.Shared.GeneralDTO;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

namespace GeoTrack.API.Attributes
{
    public class AccesoAttribute : ActionFilterAttribute
    {
        private readonly IAccesoRepository _accesoRepository;

        public AccesoAttribute(IAccesoRepository accesoRepository)
        {
            _accesoRepository = accesoRepository;
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            string sitio = context.HttpContext.Request.Headers["Sitio"].FirstOrDefault() ?? string.Empty;
            string clave = context.HttpContext.Request.Headers["Clave"].FirstOrDefault() ?? string.Empty;

            if (string.IsNullOrEmpty(sitio) || string.IsNullOrEmpty(clave))
            {
                context.Result = new ObjectResult(RespuestaDto.ParametrosIncorrectos(
                    "Acceso inválido",
                    "No se han enviado credenciales de acceso"))
                {
                    StatusCode = 401
                };
                return;
            }

            if (!await _accesoRepository.ValidarAccesoAsync(sitio, clave))
            {
                context.Result = new ObjectResult(RespuestaDto.ParametrosIncorrectos(
                    "Acceso inválido",
                    "Las credenciales de acceso son inválidas"))
                {
                    StatusCode = 401
                };
                return;
            }

            await next();
        }
    }
}

