using GeoTrack.API.Shared.GeneralDTO;
using GeoTrack.API.Shared.InDTO.DepartamentoInDto;

namespace GeoTrack.API.Domain.Contracts.DepartamentoRepository
{
    /// <summary>
    /// Interfaz para la gestión de departamentos en el sistema
    /// </summary>
    public interface IDepartamentoRepository
    {
        Task<List<DepartamentoDto>> ObtenerTodosAsync();
        Task<List<DepartamentoDto>> ObtenerPorPaisIdAsync(int paisId);
        Task<DepartamentoDto?> ObtenerPorIdAsync(int id);
        Task<RespuestaDto> CrearAsync(DepartamentoDto departamentoDto, Guid usuarioId);
        Task<RespuestaDto> ActualizarAsync(int id, DepartamentoDto departamentoDto, Guid usuarioId);
        Task<RespuestaDto> EliminarAsync(int id);
        Task<bool> ExisteAsync(int id);
        Task<bool> ExistePorNombreYPaisAsync(string nombre, int paisId);
        Task<PaginacionDto<DepartamentoDto>> ObtenerPaginadoAsync(int pagina, int elementosPorPagina, int? paisId = null, string? busqueda = null);
    }
}
