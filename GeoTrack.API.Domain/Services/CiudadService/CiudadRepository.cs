using GeoTrack.API.Domain.Contracts.CiudadRepository;
using GeoTrack.API.Infrastructure;
using GeoTrack.API.Shared.GeneralDTO;
using GeoTrack.API.Shared.InDTO.CiudadInDto;
using GeoTrack.API.Util;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GeoTrack.API.Domain.Services.CiudadService
{
    /// <summary>
    /// Implementación del repositorio de ciudades
    /// </summary>
    public class CiudadRepository : ICiudadRepository
    {
        private readonly DBContext _context;
        private readonly ILogger<CiudadRepository> _logger;

        public CiudadRepository(DBContext context, ILogger<CiudadRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene todas las ciudades activas
        /// </summary>
        public async Task<List<CiudadDto>> ObtenerTodosAsync()
        {
            _logger.LogInformation("Obteniendo todas las ciudades");

            return await _context.Ciudades
                .Where(c => c.Activo)
                .Include(c => c.Departamento)
                .ThenInclude(d => d.Pais)
                .Select(c => new CiudadDto
                {
                    Id = c.Id,
                    Nombre = c.Nombre,
                    DepartamentoId = c.DepartamentoId,
                    Departamento = c.Departamento.Nombre,
                    PaisId = c.Departamento.PaisId,
                    Pais = c.Departamento.Pais.Nombre,
                    CodigoPostal = c.CodigoPostal,
                    Activo = c.Activo,
                    FechaCreacion = c.FechaCreacion,
                    FechaModificacion = c.FechaModificacion,
                    CreadoPorId = c.CreadoPorId,
                    ModificadoPorId = c.ModificadoPorId
                })
                .OrderBy(c => c.Nombre)
                .ToListAsync();
        }

        /// <summary>
        /// Obtiene todas las ciudades de un departamento específico
        /// </summary>
        public async Task<List<CiudadDto>> ObtenerPorDepartamentoIdAsync(int departamentoId)
        {
            _logger.LogInformation("Obteniendo ciudades del departamento con ID: {DepartamentoId}", departamentoId);

            return await _context.Ciudades
                .Where(c => c.DepartamentoId == departamentoId && c.Activo)
                .Include(c => c.Departamento)
                .ThenInclude(d => d.Pais)
                .Select(c => new CiudadDto
                {
                    Id = c.Id,
                    Nombre = c.Nombre,
                    DepartamentoId = c.DepartamentoId,
                    Departamento = c.Departamento.Nombre,
                    PaisId = c.Departamento.PaisId,
                    Pais = c.Departamento.Pais.Nombre,
                    CodigoPostal = c.CodigoPostal,
                    Activo = c.Activo,
                    FechaCreacion = c.FechaCreacion,
                    FechaModificacion = c.FechaModificacion,
                    CreadoPorId = c.CreadoPorId,
                    ModificadoPorId = c.ModificadoPorId
                })
                .OrderBy(c => c.Nombre)
                .ToListAsync();
        }

        /// <summary>
        /// Obtiene una ciudad por su ID
        /// </summary>
        public async Task<CiudadDto?> ObtenerPorIdAsync(int id)
        {
            _logger.LogInformation("Obteniendo ciudad con ID: {Id}", id);

            return await _context.Ciudades
                .Where(c => c.Id == id)
                .Include(c => c.Departamento)
                .ThenInclude(d => d.Pais)
                .Select(c => new CiudadDto
                {
                    Id = c.Id,
                    Nombre = c.Nombre,
                    DepartamentoId = c.DepartamentoId,
                    Departamento = c.Departamento.Nombre,
                    PaisId = c.Departamento.PaisId,
                    Pais = c.Departamento.Pais.Nombre,
                    CodigoPostal = c.CodigoPostal,
                    Activo = c.Activo,
                    FechaCreacion = c.FechaCreacion,
                    FechaModificacion = c.FechaModificacion,
                    CreadoPorId = c.CreadoPorId,
                    ModificadoPorId = c.ModificadoPorId,
                    CreadoPor = c.CreadoPor != null ? $"{c.CreadoPor.Nombre} {c.CreadoPor.Apellido}" : null,
                    ModificadoPor = c.ModificadoPor != null ? $"{c.ModificadoPor.Nombre} {c.ModificadoPor.Apellido}" : null
                })
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Crea una nueva ciudad
        /// </summary>
        public async Task<RespuestaDto> CrearAsync(CiudadDto ciudadDto, Guid usuarioId)
        {
            try
            {
                _logger.LogInformation("Creando ciudad: {Nombre}", ciudadDto.Nombre);

                // Validar que el departamento exista
                var departamentoExiste = await _context.Departamentos
                    .AnyAsync(d => d.Id == ciudadDto.DepartamentoId && d.Activo);

                if (!departamentoExiste)
                {
                    return RespuestaDto.ParametrosIncorrectos(
                        "Creación fallida",
                        "El departamento especificado no existe o no está activo");
                }

                // Validar que no exista una ciudad con el mismo nombre en el mismo departamento
                if (await ExistePorNombreYDepartamentoAsync(ciudadDto.Nombre, ciudadDto.DepartamentoId))
                {
                    return RespuestaDto.ParametrosIncorrectos(
                        "Creación fallida",
                        $"Ya existe una ciudad con el nombre '{ciudadDto.Nombre}' en el departamento seleccionado");
                }

                var ciudad = new Ciudade
                {
                    Nombre = ciudadDto.Nombre,
                    DepartamentoId = ciudadDto.DepartamentoId,
                    CodigoPostal = ciudadDto.CodigoPostal,
                    Activo = true,
                    FechaCreacion = DateTime.Now,
                    CreadoPorId = usuarioId
                };

                await _context.Ciudades.AddAsync(ciudad);
                await _context.SaveChangesAsync();

                // Obtener información adicional para la respuesta
                var infoDepartamento = await _context.Departamentos
                    .Where(d => d.Id == ciudadDto.DepartamentoId)
                    .Include(d => d.Pais)
                    .Select(d => new { Departamento = d.Nombre, Pais = d.Pais.Nombre, PaisId = d.PaisId })
                    .FirstOrDefaultAsync();

                return RespuestaDto.Exitoso(
                    "Ciudad creada",
                    $"La ciudad '{ciudad.Nombre}' ha sido creada correctamente",
                    new CiudadDto
                    {
                        Id = ciudad.Id,
                        Nombre = ciudad.Nombre,
                        DepartamentoId = ciudad.DepartamentoId,
                        Departamento = infoDepartamento?.Departamento,
                        PaisId = infoDepartamento?.PaisId,
                        Pais = infoDepartamento?.Pais,
                        CodigoPostal = ciudad.CodigoPostal,
                        Activo = ciudad.Activo,
                        FechaCreacion = ciudad.FechaCreacion,
                        CreadoPorId = ciudad.CreadoPorId
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear ciudad {Nombre}", ciudadDto.Nombre);
                return RespuestaDto.ErrorInterno(ex.Message);
            }
        }

        /// <summary>
        /// Actualiza una ciudad existente
        /// </summary>
        public async Task<RespuestaDto> ActualizarAsync(int id, CiudadDto ciudadDto, Guid usuarioId)
        {
            try
            {
                _logger.LogInformation("Actualizando ciudad con ID: {Id}", id);

                var ciudad = await _context.Ciudades.FindAsync(id);
                if (ciudad == null)
                {
                    return RespuestaDto.NoEncontrado("Ciudad");
                }

                // Validar que el departamento exista
                var departamentoExiste = await _context.Departamentos
                    .AnyAsync(d => d.Id == ciudadDto.DepartamentoId && d.Activo);

                if (!departamentoExiste)
                {
                    return RespuestaDto.ParametrosIncorrectos(
                        "Actualización fallida",
                        "El departamento especificado no existe o no está activo");
                }

                // Validar que no exista otra ciudad con el mismo nombre en el mismo departamento
                if (ciudad.Nombre != ciudadDto.Nombre &&
                    await _context.Ciudades.AnyAsync(c => c.Nombre == ciudadDto.Nombre &&
                                                         c.DepartamentoId == ciudadDto.DepartamentoId &&
                                                         c.Id != id))
                {
                    return RespuestaDto.ParametrosIncorrectos(
                        "Actualización fallida",
                        $"Ya existe una ciudad con el nombre '{ciudadDto.Nombre}' en el departamento seleccionado");
                }

                ciudad.Nombre = ciudadDto.Nombre;
                ciudad.DepartamentoId = ciudadDto.DepartamentoId;
                ciudad.CodigoPostal = ciudadDto.CodigoPostal;
                ciudad.Activo = ciudadDto.Activo;
                ciudad.FechaModificacion = DateTime.Now;
                ciudad.ModificadoPorId = usuarioId;

                await _context.SaveChangesAsync();

                // Obtener información adicional para la respuesta
                var infoDepartamento = await _context.Departamentos
                    .Where(d => d.Id == ciudadDto.DepartamentoId)
                    .Include(d => d.Pais)
                    .Select(d => new { Departamento = d.Nombre, Pais = d.Pais.Nombre, PaisId = d.PaisId })
                    .FirstOrDefaultAsync();

                return RespuestaDto.Exitoso(
                    "Ciudad actualizada",
                    $"La ciudad '{ciudad.Nombre}' ha sido actualizada correctamente",
                    new CiudadDto
                    {
                        Id = ciudad.Id,
                        Nombre = ciudad.Nombre,
                        DepartamentoId = ciudad.DepartamentoId,
                        Departamento = infoDepartamento?.Departamento,
                        PaisId = infoDepartamento?.PaisId,
                        Pais = infoDepartamento?.Pais,
                        CodigoPostal = ciudad.CodigoPostal,
                        Activo = ciudad.Activo,
                        FechaCreacion = ciudad.FechaCreacion,
                        FechaModificacion = ciudad.FechaModificacion,
                        CreadoPorId = ciudad.CreadoPorId,
                        ModificadoPorId = ciudad.ModificadoPorId
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar ciudad {Id}", id);
                return RespuestaDto.ErrorInterno(ex.Message);
            }
        }

        /// <summary>
        /// Elimina una ciudad (desactivación lógica)
        /// </summary>
        public async Task<RespuestaDto> EliminarAsync(int id)
        {
            try
            {
                _logger.LogInformation("Eliminando ciudad con ID: {Id}", id);

                var ciudad = await _context.Ciudades.FindAsync(id);
                if (ciudad == null)
                {
                    return RespuestaDto.NoEncontrado("Ciudad");
                }

                ciudad.Activo = false;
                ciudad.FechaModificacion = DateTime.Now;

                await _context.SaveChangesAsync();

                return RespuestaDto.Exitoso(
                    "Ciudad eliminada",
                    $"La ciudad '{ciudad.Nombre}' ha sido eliminada correctamente",
                    null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar ciudad {Id}", id);
                return RespuestaDto.ErrorInterno(ex.Message);
            }
        }

        /// <summary>
        /// Verifica si existe una ciudad con el ID especificado
        /// </summary>
        public async Task<bool> ExisteAsync(int id)
        {
            return await _context.Ciudades.AnyAsync(c => c.Id == id);
        }

        /// <summary>
        /// Verifica si existe una ciudad con el nombre especificado en un departamento específico
        /// </summary>
        public async Task<bool> ExistePorNombreYDepartamentoAsync(string nombre, int departamentoId)
        {
            return await _context.Ciudades.AnyAsync(c => c.Nombre == nombre && c.DepartamentoId == departamentoId);
        }

        /// <summary>
        /// Obtiene una lista paginada de ciudades
        /// </summary>
        public async Task<PaginacionDto<CiudadDto>> ObtenerPaginadoAsync(
              int pagina,
              int elementosPorPagina,
              int? departamentoId = null,
              string? busqueda = null)
        {
            _logger.LogInformation(
                "Obteniendo ciudades paginadas. Página: {Pagina}, Elementos: {Elementos}, Departamento: {DepartamentoId}, Búsqueda: {Busqueda}",
                pagina, elementosPorPagina, departamentoId, busqueda);

            // 1. Query base con todas las navegaciones necesarias
            IQueryable<Ciudade> query = _context.Ciudades
                .Include(c => c.Departamento)
                    .ThenInclude(d => d.Pais)
                .Include(c => c.CreadoPor)
                .Include(c => c.ModificadoPor);

            // 2. Filtro por departamento
            if (departamentoId.HasValue)
                query = query.Where(c => c.DepartamentoId == departamentoId);

            // 3. Filtro de búsqueda
            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                busqueda = busqueda.ToLower();

                query = query.Where(c =>
                    c.Nombre.ToLower().Contains(busqueda) ||
                    (c.CodigoPostal != null && c.CodigoPostal.ToLower().Contains(busqueda)) ||
                    c.Departamento.Nombre.ToLower().Contains(busqueda) ||
                    c.Departamento.Pais.Nombre.ToLower().Contains(busqueda));
            }

            // 4. Totales de paginación
            int totalRegistros = await query.CountAsync();
            int totalPaginas = (int)Math.Ceiling((double)totalRegistros / elementosPorPagina);

            // 5. Traer entidades paginadas
            List<Ciudade> ciudades = await query
                .OrderBy(c => c.Nombre)
                .Skip((pagina - 1) * elementosPorPagina)
                .Take(elementosPorPagina)
                .AsNoTracking()
                .ToListAsync();

            // 6. Mapear a DTO
            List<CiudadDto> ciudadesDto =
                Mapping.ConvertirLista<Ciudade, CiudadDto>(ciudades);

            // 7. Completar campos derivados
            for (int i = 0; i < ciudades.Count; i++)
            {
                var entidad = ciudades[i];
                var dto = ciudadesDto[i];

                dto.Departamento = entidad.Departamento?.Nombre;
                dto.Pais = entidad.Departamento?.Pais?.Nombre;

                dto.CreadoPor = entidad.CreadoPor != null
                    ? $"{entidad.CreadoPor.Nombre} {entidad.CreadoPor.Apellido}"
                    : null;

                dto.ModificadoPor = entidad.ModificadoPor != null
                    ? $"{entidad.ModificadoPor.Nombre} {entidad.ModificadoPor.Apellido}"
                    : null;
            }

            // 8. Construir resultado
            return new PaginacionDto<CiudadDto>
            {
                Pagina = pagina,
                ElementosPorPagina = elementosPorPagina,
                TotalPaginas = totalPaginas,
                TotalRegistros = totalRegistros,
                Lista = ciudadesDto
            };
        }
    }
}
