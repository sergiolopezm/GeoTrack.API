using GeoTrack.API.Domain.Contracts.PaisRepository;
using GeoTrack.API.Domain.Contracts;
using GeoTrack.API.Shared.GeneralDTO;
using GeoTrack.API.Shared.InDTO.PaisInDto;
using Microsoft.AspNetCore.Mvc;
using GeoTrack.API.Attributes;

namespace GeoTrack.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [JwtAuthorization]
    [ServiceFilter(typeof(LogAttribute))]
    [ServiceFilter(typeof(ExceptionAttribute))]
    [ProducesResponseType(typeof(RespuestaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RespuestaDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(RespuestaDto), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(RespuestaDto), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(RespuestaDto), StatusCodes.Status500InternalServerError)]
    public class PaisController : ControllerBase
    {
        private readonly IPaisRepository _paisRepository;
        private readonly ILogRepository _logRepository;

        public PaisController(IPaisRepository paisRepository, ILogRepository logRepository)
        {
            _paisRepository = paisRepository;
            _logRepository = logRepository;
        }

        /// <summary>
        /// Obtiene todos los países
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ObtenerTodos()
        {
            try
            {
                var paises = await _paisRepository.ObtenerTodosAsync();
                return Ok(RespuestaDto.Exitoso(
                    "Países obtenidos",
                    $"Se han obtenido {paises.Count} países",
                    paises));
            }
            catch (Exception ex)
            {
                await _logRepository.ErrorAsync(
                    GetUsuarioId(),
                    HttpContext.Connection.RemoteIpAddress?.ToString(),
                    "ObtenerTodosPaises",
                    ex.Message);

                return StatusCode(500, RespuestaDto.ErrorInterno());
            }
        }

        /// <summary>
        /// Obtiene un país por su ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> ObtenerPorId(int id)
        {
            try
            {
                var pais = await _paisRepository.ObtenerPorIdAsync(id);

                if (pais == null)
                {
                    return NotFound(RespuestaDto.NoEncontrado("País"));
                }

                return Ok(RespuestaDto.Exitoso(
                    "País obtenido",
                    $"Se ha obtenido el país '{pais.Nombre}'",
                    pais));
            }
            catch (Exception ex)
            {
                await _logRepository.ErrorAsync(
                    GetUsuarioId(),
                    HttpContext.Connection.RemoteIpAddress?.ToString(),
                    $"ObtenerPaisPorId: {id}",
                    ex.Message);

                return StatusCode(500, RespuestaDto.ErrorInterno());
            }
        }

        /// <summary>
        /// Obtiene una lista paginada de países
        /// </summary>
        [HttpGet("paginado")]
        public async Task<IActionResult> ObtenerPaginado([FromQuery] int pagina = 1, [FromQuery] int elementosPorPagina = 10, [FromQuery] string? busqueda = null)
        {
            try
            {
                var paises = await _paisRepository.ObtenerPaginadoAsync(pagina, elementosPorPagina, busqueda);
                return Ok(RespuestaDto.Exitoso(
                    "Países obtenidos",
                    $"Se han obtenido {paises.Lista?.Count ?? 0} países de un total de {paises.TotalRegistros}",
                    paises));
            }
            catch (Exception ex)
            {
                await _logRepository.ErrorAsync(
                    GetUsuarioId(),
                    HttpContext.Connection.RemoteIpAddress?.ToString(),
                    "ObtenerPaisesPaginado",
                    ex.Message);

                return StatusCode(500, RespuestaDto.ErrorInterno());
            }
        }

        /// <summary>
        /// Crea un nuevo país
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] PaisDto paisDto)
        {
            try
            {
                var resultado = await _paisRepository.CrearAsync(paisDto, GetUsuarioId());

                if (resultado.Exito)
                {
                    await _logRepository.AccionAsync(
                        GetUsuarioId(),
                        HttpContext.Connection.RemoteIpAddress?.ToString(),
                        "CrearPais",
                        $"Se ha creado el país '{paisDto.Nombre}'");

                    return CreatedAtAction(nameof(ObtenerPorId), new { id = ((PaisDto)resultado.Resultado!).Id }, resultado);
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
                    "CrearPais",
                    ex.Message);

                return StatusCode(500, RespuestaDto.ErrorInterno());
            }
        }

        /// <summary>
        /// Actualiza un país existente
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Actualizar(int id, [FromBody] PaisDto paisDto)
        {
            try
            {
                // Verificar que el ID coincida
                if (paisDto.Id != 0 && paisDto.Id != id)
                {
                    return BadRequest(RespuestaDto.ParametrosIncorrectos(
                        "Actualización fallida",
                        "El ID del país no coincide con el ID de la URL"));
                }

                // Verificar que el país exista
                var paisExistente = await _paisRepository.ExisteAsync(id);
                if (!paisExistente)
                {
                    return NotFound(RespuestaDto.NoEncontrado("País"));
                }

                var resultado = await _paisRepository.ActualizarAsync(id, paisDto, GetUsuarioId());

                if (resultado.Exito)
                {
                    await _logRepository.AccionAsync(
                        GetUsuarioId(),
                        HttpContext.Connection.RemoteIpAddress?.ToString(),
                        "ActualizarPais",
                        $"Se ha actualizado el país '{paisDto.Nombre}'");

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
                    $"ActualizarPais: {id}",
                    ex.Message);

                return StatusCode(500, RespuestaDto.ErrorInterno());
            }
        }

        /// <summary>
        /// Elimina un país
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            try
            {
                // Verificar que el país exista
                var paisExistente = await _paisRepository.ExisteAsync(id);
                if (!paisExistente)
                {
                    return NotFound(RespuestaDto.NoEncontrado("País"));
                }

                var resultado = await _paisRepository.EliminarAsync(id);

                if (resultado.Exito)
                {
                    await _logRepository.AccionAsync(
                        GetUsuarioId(),
                        HttpContext.Connection.RemoteIpAddress?.ToString(),
                        "EliminarPais",
                        $"Se ha eliminado el país con ID '{id}'");

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
                    $"EliminarPais: {id}",
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
