using GeoTrack.API.Shared.GeneralDTO;
using GeoTrack.API.Shared.InDTO.CiudadInDto;

namespace GeoTrack.API.Domain.Contracts.CiudadRepository
{
    /// <summary>
    /// Interfaz para la gestión de ciudades en el sistema
    /// </summary>
    public interface ICiudadRepository
    {
        Task<List<CiudadDto>> ObtenerTodosAsync();
        Task<List<CiudadDto>> ObtenerPorDepartamentoIdAsync(int departamentoId);
        Task<CiudadDto?> ObtenerPorIdAsync(int id);
        Task<RespuestaDto> CrearAsync(CiudadDto ciudadDto, Guid usuarioId);
        Task<RespuestaDto> ActualizarAsync(int id, CiudadDto ciudadDto, Guid usuarioId);
        Task<RespuestaDto> EliminarAsync(int id);
        Task<bool> ExisteAsync(int id);
        Task<bool> ExistePorNombreYDepartamentoAsync(string nombre, int departamentoId);
        Task<PaginacionDto<CiudadDto>> ObtenerPaginadoAsync(int pagina, int elementosPorPagina, int? departamentoId = null, string? busqueda = null);
    }
}
