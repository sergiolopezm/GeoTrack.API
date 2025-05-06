using GeoTrack.API.Shared.GeneralDTO;
using GeoTrack.API.Shared.InDTO.PaisInDto;

namespace GeoTrack.API.Domain.Contracts.PaisRepository
{
    /// <summary>
    /// Interfaz para la gestión de países en el sistema
    /// </summary>
    public interface IPaisRepository
    {
        Task<List<PaisDto>> ObtenerTodosAsync();
        Task<PaisDto?> ObtenerPorIdAsync(int id);
        Task<RespuestaDto> CrearAsync(PaisDto paisDto, Guid usuarioId);
        Task<RespuestaDto> ActualizarAsync(int id, PaisDto paisDto, Guid usuarioId);
        Task<RespuestaDto> EliminarAsync(int id);
        Task<bool> ExisteAsync(int id);
        Task<bool> ExistePorNombreAsync(string nombre);
        Task<bool> ExistePorCodigoAsync(string codigo);
        Task<PaginacionDto<PaisDto>> ObtenerPaginadoAsync(int pagina, int elementosPorPagina, string? busqueda = null);
    }
}
