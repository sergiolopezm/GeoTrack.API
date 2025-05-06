using GeoTrack.API.Domain.Contracts.DepartamentoRepository;
using GeoTrack.API.Infrastructure;
using GeoTrack.API.Shared.GeneralDTO;
using GeoTrack.API.Shared.InDTO.DepartamentoInDto;
using GeoTrack.API.Util;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GeoTrack.API.Domain.Services.DepartamentoService
{
    /// <summary>
    /// Implementación del repositorio de departamentos
    /// </summary>
    public class DepartamentoRepository : IDepartamentoRepository
    {
        private readonly DBContext _context;
        private readonly ILogger<DepartamentoRepository> _logger;

        public DepartamentoRepository(DBContext context, ILogger<DepartamentoRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene todos los departamentos activos
        /// </summary>
        public async Task<List<DepartamentoDto>> ObtenerTodosAsync()
        {
            _logger.LogInformation("Obteniendo todos los departamentos");

            return await _context.Departamentos
                .Where(d => d.Activo)
                .Include(d => d.Pais)
                .Select(d => new DepartamentoDto
                {
                    Id = d.Id,
                    Nombre = d.Nombre,
                    PaisId = d.PaisId,
                    Pais = d.Pais.Nombre,
                    Activo = d.Activo,
                    FechaCreacion = d.FechaCreacion,
                    FechaModificacion = d.FechaModificacion,
                    CreadoPorId = d.CreadoPorId,
                    ModificadoPorId = d.ModificadoPorId,
                    CiudadesCount = d.Ciudades.Count(c => c.Activo)
                })
                .OrderBy(d => d.Nombre)
                .ToListAsync();
        }

        /// <summary>
        /// Obtiene todos los departamentos de un país específico
        /// </summary>
        public async Task<List<DepartamentoDto>> ObtenerPorPaisIdAsync(int paisId)
        {
            _logger.LogInformation("Obteniendo departamentos del país con ID: {PaisId}", paisId);

            return await _context.Departamentos
                .Where(d => d.PaisId == paisId && d.Activo)
                .Include(d => d.Pais)
                .Select(d => new DepartamentoDto
                {
                    Id = d.Id,
                    Nombre = d.Nombre,
                    PaisId = d.PaisId,
                    Pais = d.Pais.Nombre,
                    Activo = d.Activo,
                    FechaCreacion = d.FechaCreacion,
                    FechaModificacion = d.FechaModificacion,
                    CreadoPorId = d.CreadoPorId,
                    ModificadoPorId = d.ModificadoPorId,
                    CiudadesCount = d.Ciudades.Count(c => c.Activo)
                })
                .OrderBy(d => d.Nombre)
                .ToListAsync();
        }

        /// <summary>
        /// Obtiene un departamento por su ID
        /// </summary>
        public async Task<DepartamentoDto?> ObtenerPorIdAsync(int id)
        {
            _logger.LogInformation("Obteniendo departamento con ID: {Id}", id);

            return await _context.Departamentos
                .Where(d => d.Id == id)
                .Include(d => d.Pais)
                .Select(d => new DepartamentoDto
                {
                    Id = d.Id,
                    Nombre = d.Nombre,
                    PaisId = d.PaisId,
                    Pais = d.Pais.Nombre,
                    Activo = d.Activo,
                    FechaCreacion = d.FechaCreacion,
                    FechaModificacion = d.FechaModificacion,
                    CreadoPorId = d.CreadoPorId,
                    ModificadoPorId = d.ModificadoPorId,
                    CreadoPor = d.CreadoPor != null ? $"{d.CreadoPor.Nombre} {d.CreadoPor.Apellido}" : null,
                    ModificadoPor = d.ModificadoPor != null ? $"{d.ModificadoPor.Nombre} {d.ModificadoPor.Apellido}" : null,
                    CiudadesCount = d.Ciudades.Count(c => c.Activo)
                })
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Crea un nuevo departamento
        /// </summary>
        public async Task<RespuestaDto> CrearAsync(DepartamentoDto departamentoDto, Guid usuarioId)
        {
            try
            {
                _logger.LogInformation("Creando departamento: {Nombre}", departamentoDto.Nombre);

                // Validar que el país exista
                var paisExiste = await _context.Paises.AnyAsync(p => p.Id == departamentoDto.PaisId && p.Activo);
                if (!paisExiste)
                {
                    return RespuestaDto.ParametrosIncorrectos(
                        "Creación fallida",
                        "El país especificado no existe o no está activo");
                }

                // Validar que no exista un departamento con el mismo nombre en el mismo país
                if (await ExistePorNombreYPaisAsync(departamentoDto.Nombre, departamentoDto.PaisId))
                {
                    return RespuestaDto.ParametrosIncorrectos(
                        "Creación fallida",
                        $"Ya existe un departamento con el nombre '{departamentoDto.Nombre}' en el país seleccionado");
                }

                var departamento = new Departamento
                {
                    Nombre = departamentoDto.Nombre,
                    PaisId = departamentoDto.PaisId,
                    Activo = true,
                    FechaCreacion = DateTime.Now,
                    CreadoPorId = usuarioId
                };

                await _context.Departamentos.AddAsync(departamento);
                await _context.SaveChangesAsync();

                // Obtener el nombre del país para la respuesta
                var nombrePais = await _context.Paises
                    .Where(p => p.Id == departamentoDto.PaisId)
                    .Select(p => p.Nombre)
                    .FirstOrDefaultAsync();

                return RespuestaDto.Exitoso(
                    "Departamento creado",
                    $"El departamento '{departamento.Nombre}' ha sido creado correctamente",
                    new DepartamentoDto
                    {
                        Id = departamento.Id,
                        Nombre = departamento.Nombre,
                        PaisId = departamento.PaisId,
                        Pais = nombrePais,
                        Activo = departamento.Activo,
                        FechaCreacion = departamento.FechaCreacion,
                        CreadoPorId = departamento.CreadoPorId
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear departamento {Nombre}", departamentoDto.Nombre);
                return RespuestaDto.ErrorInterno(ex.Message);
            }
        }

        /// <summary>
        /// Actualiza un departamento existente
        /// </summary>
        public async Task<RespuestaDto> ActualizarAsync(int id, DepartamentoDto departamentoDto, Guid usuarioId)
        {
            try
            {
                _logger.LogInformation("Actualizando departamento con ID: {Id}", id);

                var departamento = await _context.Departamentos.FindAsync(id);
                if (departamento == null)
                {
                    return RespuestaDto.NoEncontrado("Departamento");
                }

                // Validar que el país exista
                var paisExiste = await _context.Paises.AnyAsync(p => p.Id == departamentoDto.PaisId && p.Activo);
                if (!paisExiste)
                {
                    return RespuestaDto.ParametrosIncorrectos(
                        "Actualización fallida",
                        "El país especificado no existe o no está activo");
                }

                // Validar que no exista otro departamento con el mismo nombre en el mismo país
                if (departamento.Nombre != departamentoDto.Nombre &&
                    await _context.Departamentos.AnyAsync(d => d.Nombre == departamentoDto.Nombre &&
                                                              d.PaisId == departamentoDto.PaisId &&
                                                              d.Id != id))
                {
                    return RespuestaDto.ParametrosIncorrectos(
                        "Actualización fallida",
                        $"Ya existe un departamento con el nombre '{departamentoDto.Nombre}' en el país seleccionado");
                }

                departamento.Nombre = departamentoDto.Nombre;
                departamento.PaisId = departamentoDto.PaisId;
                departamento.Activo = departamentoDto.Activo;
                departamento.FechaModificacion = DateTime.Now;
                departamento.ModificadoPorId = usuarioId;

                await _context.SaveChangesAsync();

                // Obtener el nombre del país para la respuesta
                var nombrePais = await _context.Paises
                    .Where(p => p.Id == departamentoDto.PaisId)
                    .Select(p => p.Nombre)
                    .FirstOrDefaultAsync();

                return RespuestaDto.Exitoso(
                    "Departamento actualizado",
                    $"El departamento '{departamento.Nombre}' ha sido actualizado correctamente",
                    new DepartamentoDto
                    {
                        Id = departamento.Id,
                        Nombre = departamento.Nombre,
                        PaisId = departamento.PaisId,
                        Pais = nombrePais,
                        Activo = departamento.Activo,
                        FechaCreacion = departamento.FechaCreacion,
                        FechaModificacion = departamento.FechaModificacion,
                        CreadoPorId = departamento.CreadoPorId,
                        ModificadoPorId = departamento.ModificadoPorId
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar departamento {Id}", id);
                return RespuestaDto.ErrorInterno(ex.Message);
            }
        }

        /// <summary>
        /// Elimina un departamento (desactivación lógica)
        /// </summary>
        public async Task<RespuestaDto> EliminarAsync(int id)
        {
            try
            {
                _logger.LogInformation("Eliminando departamento con ID: {Id}", id);

                var departamento = await _context.Departamentos.FindAsync(id);
                if (departamento == null)
                {
                    return RespuestaDto.NoEncontrado("Departamento");
                }

                // Verificar si tiene ciudades asociadas
                var tieneCiudades = await _context.Ciudades.AnyAsync(c => c.DepartamentoId == id && c.Activo);
                if (tieneCiudades)
                {
                    return RespuestaDto.ParametrosIncorrectos(
                        "Eliminación fallida",
                        $"No se puede eliminar el departamento '{departamento.Nombre}' porque tiene ciudades asociadas");
                }

                departamento.Activo = false;
                departamento.FechaModificacion = DateTime.Now;

                await _context.SaveChangesAsync();

                return RespuestaDto.Exitoso(
                    "Departamento eliminado",
                    $"El departamento '{departamento.Nombre}' ha sido eliminado correctamente",
                    null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar departamento {Id}", id);
                return RespuestaDto.ErrorInterno(ex.Message);
            }
        }

        /// <summary>
        /// Verifica si existe un departamento con el ID especificado
        /// </summary>
        public async Task<bool> ExisteAsync(int id)
        {
            return await _context.Departamentos.AnyAsync(d => d.Id == id);
        }

        /// <summary>
        /// Verifica si existe un departamento con el nombre especificado en un país específico
        /// </summary>
        public async Task<bool> ExistePorNombreYPaisAsync(string nombre, int paisId)
        {
            return await _context.Departamentos.AnyAsync(d => d.Nombre == nombre && d.PaisId == paisId);
        }

        /// <summary>
        /// Obtiene una lista paginada de departamentos
        /// </summary>
        public async Task<PaginacionDto<DepartamentoDto>> ObtenerPaginadoAsync(
             int pagina,
             int elementosPorPagina,
             int? paisId = null,
             string? busqueda = null)
        {
            _logger.LogInformation(
                "Obteniendo departamentos paginados. Página: {Pagina}, Elementos: {Elementos}, País: {PaisId}, Búsqueda: {Busqueda}",
                pagina, elementosPorPagina, paisId, busqueda);

            // 1. Query base con navegación necesaria
            IQueryable<Departamento> query = _context.Departamentos
                .Include(d => d.Pais)
                .Include(d => d.CreadoPor)
                .Include(d => d.ModificadoPor)
                .Include(d => d.Ciudades);

            // 2. Filtro por país
            if (paisId.HasValue)
                query = query.Where(d => d.PaisId == paisId);

            // 3. Filtro de búsqueda
            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                busqueda = busqueda.ToLower();
                query = query.Where(d =>
                    d.Nombre.ToLower().Contains(busqueda) ||
                    d.Pais.Nombre.ToLower().Contains(busqueda));
            }

            // 4. Totales para la paginación
            int totalRegistros = await query.CountAsync();
            int totalPaginas = (int)Math.Ceiling((double)totalRegistros / elementosPorPagina);

            // 5. Recuperar entidades paginadas
            List<Departamento> departamentos = await query
                .OrderBy(d => d.Nombre)
                .Skip((pagina - 1) * elementosPorPagina)
                .Take(elementosPorPagina)
                .AsNoTracking()
                .ToListAsync();

            // 6. Mapear a DTO
            List<DepartamentoDto> departamentosDto =
                Mapping.ConvertirLista<Departamento, DepartamentoDto>(departamentos);

            // 7. Completar campos calculados / específicos
            for (int i = 0; i < departamentos.Count; i++)
            {
                var entidad = departamentos[i];
                var dto = departamentosDto[i];

                dto.Pais = entidad.Pais?.Nombre;

                dto.CreadoPor = entidad.CreadoPor != null
                    ? $"{entidad.CreadoPor.Nombre} {entidad.CreadoPor.Apellido}"
                    : null;

                dto.ModificadoPor = entidad.ModificadoPor != null
                    ? $"{entidad.ModificadoPor.Nombre} {entidad.ModificadoPor.Apellido}"
                    : null;

                dto.CiudadesCount = entidad.Ciudades?.Count(c => c.Activo) ?? 0;
            }

            // 8. Construir resultado paginado
            return new PaginacionDto<DepartamentoDto>
            {
                Pagina = pagina,
                ElementosPorPagina = elementosPorPagina,
                TotalPaginas = totalPaginas,
                TotalRegistros = totalRegistros,
                Lista = departamentosDto
            };
        }
    }
}
