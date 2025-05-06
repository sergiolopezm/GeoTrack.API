using GeoTrack.API.Attributes;
using GeoTrack.API.Domain.Contracts.CiudadRepository;
using GeoTrack.API.Domain.Contracts;
using GeoTrack.API.Shared.GeneralDTO;
using GeoTrack.API.Shared.InDTO.CiudadInDto;
using Microsoft.AspNetCore.Mvc;

namespace GeoTrack.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [JwtAuthorization]
    [ProducesResponseType(typeof(RespuestaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RespuestaDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(RespuestaDto), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(RespuestaDto), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(RespuestaDto), StatusCodes.Status500InternalServerError)]
    public class CiudadController : ControllerBase
    {
        private readonly ICiudadRepository _ciudadRepository;
        private readonly ILogRepository _logRepository;

        public CiudadController(ICiudadRepository ciudadRepository, ILogRepository logRepository)
        {
            _ciudadRepository = ciudadRepository;
            _logRepository = logRepository;
        }

        /// <summary>
        /// Obtiene todas las ciudades
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ObtenerTodos()
        {
            try
            {
                var ciudades = await _ciudadRepository.ObtenerTodosAsync();
                return Ok(RespuestaDto.Exitoso(
                    "Ciudades obtenidas",
                    $"Se han obtenido {ciudades.Count} ciudades",
                    ciudades));
            }
            catch (Exception ex)
            {
                await _logRepository.ErrorAsync(
                    GetUsuarioId(),
                    HttpContext.Connection.RemoteIpAddress?.ToString(),
                    "ObtenerTodasCiudades",
                    ex.Message);

                return StatusCode(500, RespuestaDto.ErrorInterno());
            }
        }

        /// <summary>
        /// Obtiene ciudades por departamento
        /// </summary>
        [HttpGet("por-departamento/{departamentoId}")]
        public async Task<IActionResult> ObtenerPorDepartamento(int departamentoId)
        {
            try
            {
                var ciudades = await _ciudadRepository.ObtenerPorDepartamentoIdAsync(departamentoId);
                return Ok(RespuestaDto.Exitoso(
                    "Ciudades obtenidas",
                    $"Se han obtenido {ciudades.Count} ciudades para el departamento ID: {departamentoId}",
                    ciudades));
            }
            catch (Exception ex)
            {
                await _logRepository.ErrorAsync(
                    GetUsuarioId(),
                    HttpContext.Connection.RemoteIpAddress?.ToString(),
                    $"ObtenerCiudadesPorDepartamento: {departamentoId}",
                    ex.Message);

                return StatusCode(500, RespuestaDto.ErrorInterno());
            }
        }

        /// <summary>
        /// Obtiene una ciudad por su ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> ObtenerPorId(int id)
        {
            try
            {
                var ciudad = await _ciudadRepository.ObtenerPorIdAsync(id);

                if (ciudad == null)
                {
                    return NotFound(RespuestaDto.NoEncontrado("Ciudad"));
                }

                return Ok(RespuestaDto.Exitoso(
                    "Ciudad obtenida",
                    $"Se ha obtenido la ciudad '{ciudad.Nombre}'",
                    ciudad));
            }
            catch (Exception ex)
            {
                await _logRepository.ErrorAsync(
                    GetUsuarioId(),
                    HttpContext.Connection.RemoteIpAddress?.ToString(),
                    $"ObtenerCiudadPorId: {id}",
                    ex.Message);

                return StatusCode(500, RespuestaDto.ErrorInterno());
            }
        }

        /// <summary>
        /// Obtiene una lista paginada de ciudades
        /// </summary>
        [HttpGet("paginado")]
        public async Task<IActionResult> ObtenerPaginado(
            [FromQuery] int pagina = 1,
            [FromQuery] int elementosPorPagina = 10,
            [FromQuery] int? departamentoId = null,
            [FromQuery] string? busqueda = null)
        {
            try
            {
                var ciudades = await _ciudadRepository.ObtenerPaginadoAsync(
                    pagina, elementosPorPagina, departamentoId, busqueda);

                return Ok(RespuestaDto.Exitoso(
                    "Ciudades obtenidas",
                    $"Se han obtenido {ciudades.Lista?.Count ?? 0} ciudades de un total de {ciudades.TotalRegistros}",
                    ciudades));
            }
            catch (Exception ex)
            {
                await _logRepository.ErrorAsync(
                    GetUsuarioId(),
                    HttpContext.Connection.RemoteIpAddress?.ToString(),
                    "ObtenerCiudadesPaginado",
                    ex.Message);

                return StatusCode(500, RespuestaDto.ErrorInterno());
            }
        }

        /// <summary>
        /// Crea una nueva ciudad
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] CiudadDto ciudadDto)
        {
            try
            {
                var resultado = await _ciudadRepository.CrearAsync(ciudadDto, GetUsuarioId());

                if (resultado.Exito)
                {
                    await _logRepository.AccionAsync(
                        GetUsuarioId(),
                        HttpContext.Connection.RemoteIpAddress?.ToString(),
                        "CrearCiudad",
                        $"Se ha creado la ciudad '{ciudadDto.Nombre}'");

                    return CreatedAtAction(nameof(ObtenerPorId), new { id = ((CiudadDto)resultado.Resultado!).Id }, resultado);
                }
                else
                {
                    return BadRequest(resultado);
                }
            }
            catch (Exception ex)
            {
                await _logRepository.ErrorAsync(
                    GetUsuarioId(),
                    HttpContext.Connection.RemoteIpAddress?.ToString(),
                    "CrearCiudad",
                    ex.Message);

                return StatusCode(500, RespuestaDto.ErrorInterno());
            }
        }

        /// <summary>
        /// Actualiza una ciudad existente
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Actualizar(int id, [FromBody] CiudadDto ciudadDto)
        {
            try
            {
                // Verificar que el ID coincida
                if (ciudadDto.Id != 0 && ciudadDto.Id != id)
                {
                    return BadRequest(RespuestaDto.ParametrosIncorrectos(
                        "Actualización fallida",
                        "El ID de la ciudad no coincide con el ID de la URL"));
                }

                // Verificar que la ciudad exista
                var ciudadExistente = await _ciudadRepository.ExisteAsync(id);
                if (!ciudadExistente)
                {
                    return NotFound(RespuestaDto.NoEncontrado("Ciudad"));
                }

                var resultado = await _ciudadRepository.ActualizarAsync(id, ciudadDto, GetUsuarioId());

                if (resultado.Exito)
                {
                    await _logRepository.AccionAsync(
                        GetUsuarioId(),
                        HttpContext.Connection.RemoteIpAddress?.ToString(),
                        "ActualizarCiudad",
                        $"Se ha actualizado la ciudad '{ciudadDto.Nombre}'");

                    return Ok(resultado);
                }
                else
                {
                    return BadRequest(resultado);
                }
            }
            catch (Exception ex)
            {
                await _logRepository.ErrorAsync(
                    GetUsuarioId(),
                    HttpContext.Connection.RemoteIpAddress?.ToString(),
                    $"ActualizarCiudad: {id}",
                    ex.Message);

                return StatusCode(500, RespuestaDto.ErrorInterno());
            }
        }

        /// <summary>
        /// Elimina una ciudad
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            try
            {
                // Verificar que la ciudad exista
                var ciudadExistente = await _ciudadRepository.ExisteAsync(id);
                if (!ciudadExistente)
                {
                    return NotFound(RespuestaDto.NoEncontrado("Ciudad"));
                }

                var resultado = await _ciudadRepository.EliminarAsync(id);

                if (resultado.Exito)
                {
                    await _logRepository.AccionAsync(
                        GetUsuarioId(),
                        HttpContext.Connection.RemoteIpAddress?.ToString(),
                        "EliminarCiudad",
                        $"Se ha eliminado la ciudad con ID '{id}'");

                    return Ok(resultado);
                }
                else
                {
                    return BadRequest(resultado);
                }
            }
            catch (Exception ex)
            {
                await _logRepository.ErrorAsync(
                    GetUsuarioId(),
                    HttpContext.Connection.RemoteIpAddress?.ToString(),
                    $"EliminarCiudad: {id}",
                    ex.Message);

                return StatusCode(500, RespuestaDto.ErrorInterno());
            }
        }

        /// <summary>
        /// Obtiene el ID del usuario actual desde el token JWT
        /// </summary>
        private Guid GetUsuarioId()
        {
            var claim = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier);
            return claim != null ? Guid.Parse(claim.Value) : Guid.Empty;
        }
    }
}
