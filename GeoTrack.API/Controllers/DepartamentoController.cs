using GeoTrack.API.Attributes;
using GeoTrack.API.Domain.Contracts.DepartamentoRepository;
using GeoTrack.API.Domain.Contracts;
using GeoTrack.API.Shared.GeneralDTO;
using GeoTrack.API.Shared.InDTO.DepartamentoInDto;
using Microsoft.AspNetCore.Mvc;

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
    public class DepartamentoController : ControllerBase
    {
        private readonly IDepartamentoRepository _departamentoRepository;
        private readonly ILogRepository _logRepository;

        public DepartamentoController(IDepartamentoRepository departamentoRepository, ILogRepository logRepository)
        {
            _departamentoRepository = departamentoRepository;
            _logRepository = logRepository;
        }

        /// <summary>
        /// Obtiene todos los departamentos
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ObtenerTodos()
        {
            try
            {
                var departamentos = await _departamentoRepository.ObtenerTodosAsync();
                return Ok(RespuestaDto.Exitoso(
                    "Departamentos obtenidos",
                    $"Se han obtenido {departamentos.Count} departamentos",
                    departamentos));
            }
            catch (Exception ex)
            {
                await _logRepository.ErrorAsync(
                    GetUsuarioId(),
                    HttpContext.Connection.RemoteIpAddress?.ToString(),
                    "ObtenerTodosDepartamentos",
                    ex.Message);

                return StatusCode(500, RespuestaDto.ErrorInterno());
            }
        }

        /// <summary>
        /// Obtiene departamentos por país
        /// </summary>
        [HttpGet("por-pais/{paisId}")]
        public async Task<IActionResult> ObtenerPorPais(int paisId)
        {
            try
            {
                var departamentos = await _departamentoRepository.ObtenerPorPaisIdAsync(paisId);
                return Ok(RespuestaDto.Exitoso(
                    "Departamentos obtenidos",
                    $"Se han obtenido {departamentos.Count} departamentos para el país ID: {paisId}",
                    departamentos));
            }
            catch (Exception ex)
            {
                await _logRepository.ErrorAsync(
                    GetUsuarioId(),
                    HttpContext.Connection.RemoteIpAddress?.ToString(),
                    $"ObtenerDepartamentosPorPais: {paisId}",
                    ex.Message);

                return StatusCode(500, RespuestaDto.ErrorInterno());
            }
        }

        /// <summary>
        /// Obtiene un departamento por su ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> ObtenerPorId(int id)
        {
            try
            {
                var departamento = await _departamentoRepository.ObtenerPorIdAsync(id);

                if (departamento == null)
                {
                    return NotFound(RespuestaDto.NoEncontrado("Departamento"));
                }

                return Ok(RespuestaDto.Exitoso(
                    "Departamento obtenido",
                    $"Se ha obtenido el departamento '{departamento.Nombre}'",
                    departamento));
            }
            catch (Exception ex)
            {
                await _logRepository.ErrorAsync(
                    GetUsuarioId(),
                    HttpContext.Connection.RemoteIpAddress?.ToString(),
                    $"ObtenerDepartamentoPorId: {id}",
                    ex.Message);

                return StatusCode(500, RespuestaDto.ErrorInterno());
            }
        }

        /// <summary>
        /// Obtiene una lista paginada de departamentos
        /// </summary>
        [HttpGet("paginado")]
        public async Task<IActionResult> ObtenerPaginado(
            [FromQuery] int pagina = 1,
            [FromQuery] int elementosPorPagina = 10,
            [FromQuery] int? paisId = null,
            [FromQuery] string? busqueda = null)
        {
            try
            {
                var departamentos = await _departamentoRepository.ObtenerPaginadoAsync(
                    pagina, elementosPorPagina, paisId, busqueda);

                return Ok(RespuestaDto.Exitoso(
                    "Departamentos obtenidos",
                    $"Se han obtenido {departamentos.Lista?.Count ?? 0} departamentos de un total de {departamentos.TotalRegistros}",
                    departamentos));
            }
            catch (Exception ex)
            {
                await _logRepository.ErrorAsync(
                    GetUsuarioId(),
                    HttpContext.Connection.RemoteIpAddress?.ToString(),
                    "ObtenerDepartamentosPaginado",
                    ex.Message);

                return StatusCode(500, RespuestaDto.ErrorInterno());
            }
        }

        /// <summary>
        /// Crea un nuevo departamento
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] DepartamentoDto departamentoDto)
        {
            try
            {
                var resultado = await _departamentoRepository.CrearAsync(departamentoDto, GetUsuarioId());

                if (resultado.Exito)
                {
                    await _logRepository.AccionAsync(
                        GetUsuarioId(),
                        HttpContext.Connection.RemoteIpAddress?.ToString(),
                        "CrearDepartamento",
                        $"Se ha creado el departamento '{departamentoDto.Nombre}'");

                    return CreatedAtAction(nameof(ObtenerPorId), new { id = ((DepartamentoDto)resultado.Resultado!).Id }, resultado);
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
                    "CrearDepartamento",
                    ex.Message);

                return StatusCode(500, RespuestaDto.ErrorInterno());
            }
        }

        /// <summary>
        /// Actualiza un departamento existente
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Actualizar(int id, [FromBody] DepartamentoDto departamentoDto)
        {
            try
            {
                // Verificar que el ID coincida
                if (departamentoDto.Id != 0 && departamentoDto.Id != id)
                {
                    return BadRequest(RespuestaDto.ParametrosIncorrectos(
                        "Actualización fallida",
                        "El ID del departamento no coincide con el ID de la URL"));
                }

                // Verificar que el departamento exista
                var departamentoExistente = await _departamentoRepository.ExisteAsync(id);
                if (!departamentoExistente)
                {
                    return NotFound(RespuestaDto.NoEncontrado("Departamento"));
                }

                var resultado = await _departamentoRepository.ActualizarAsync(id, departamentoDto, GetUsuarioId());

                if (resultado.Exito)
                {
                    await _logRepository.AccionAsync(
                        GetUsuarioId(),
                        HttpContext.Connection.RemoteIpAddress?.ToString(),
                        "ActualizarDepartamento",
                        $"Se ha actualizado el departamento '{departamentoDto.Nombre}'");

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
                    $"ActualizarDepartamento: {id}",
                    ex.Message);

                return StatusCode(500, RespuestaDto.ErrorInterno());
            }
        }

        /// <summary>
        /// Elimina un departamento
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            try
            {
                // Verificar que el departamento exista
                var departamentoExistente = await _departamentoRepository.ExisteAsync(id);
                if (!departamentoExistente)
                {
                    return NotFound(RespuestaDto.NoEncontrado("Departamento"));
                }

                var resultado = await _departamentoRepository.EliminarAsync(id);

                if (resultado.Exito)
                {
                    await _logRepository.AccionAsync(
                        GetUsuarioId(),
                        HttpContext.Connection.RemoteIpAddress?.ToString(),
                        "EliminarDepartamento",
                        $"Se ha eliminado el departamento con ID '{id}'");

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
                    $"EliminarDepartamento: {id}",
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
