using GeoTrack.API.Domain.Contracts.PaisRepository;
using GeoTrack.API.Infrastructure;
using GeoTrack.API.Shared.GeneralDTO;
using GeoTrack.API.Shared.InDTO.PaisInDto;
using GeoTrack.API.Util;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GeoTrack.API.Domain.Services.PaisService
{
    /// <summary>
    /// Implementación del repositorio de países
    /// </summary>
    public class PaisRepository : IPaisRepository
    {
        private readonly DBContext _context;
        private readonly ILogger<PaisRepository> _logger;

        public PaisRepository(DBContext context, ILogger<PaisRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene todos los países activos
        /// </summary>
        public async Task<List<PaisDto>> ObtenerTodosAsync()
        {
            return await _context.Paises
                .Where(p => p.Activo)
                .Select(p => new PaisDto
                {
                    Id = p.Id,
                    Nombre = p.Nombre,
                    CodigoISO = p.CodigoIso,
                    Activo = p.Activo,
                    FechaCreacion = p.FechaCreacion,
                    FechaModificacion = p.FechaModificacion,
                    CreadoPorId = p.CreadoPorId,
                    ModificadoPorId = p.ModificadoPorId,
                    DepartamentosCount = p.Departamentos.Count(d => d.Activo)
                })
                .OrderBy(p => p.Nombre)
                .ToListAsync();
        }

        /// <summary>
        /// Obtiene un país por su ID
        /// </summary>
        public async Task<PaisDto?> ObtenerPorIdAsync(int id)
        {
            return await _context.Paises
                .Where(p => p.Id == id)
                .Select(p => new PaisDto
                {
                    Id = p.Id,
                    Nombre = p.Nombre,
                    CodigoISO = p.CodigoIso,
                    Activo = p.Activo,
                    FechaCreacion = p.FechaCreacion,
                    FechaModificacion = p.FechaModificacion,
                    CreadoPorId = p.CreadoPorId,
                    ModificadoPorId = p.ModificadoPorId,
                    CreadoPor = p.CreadoPor != null ? $"{p.CreadoPor.Nombre} {p.CreadoPor.Apellido}" : null,
                    ModificadoPor = p.ModificadoPor != null ? $"{p.ModificadoPor.Nombre} {p.ModificadoPor.Apellido}" : null,
                    DepartamentosCount = p.Departamentos.Count(d => d.Activo)
                })
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Crea un nuevo país
        /// </summary>
        public async Task<RespuestaDto> CrearAsync(PaisDto paisDto, Guid usuarioId)
        {
            try
            {
                // Validar que no exista un país con el mismo nombre o código
                if (await ExistePorNombreAsync(paisDto.Nombre))
                {
                    return RespuestaDto.ParametrosIncorrectos(
                        "Creación fallida",
                        $"Ya existe un país con el nombre '{paisDto.Nombre}'");
                }

                if (await ExistePorCodigoAsync(paisDto.CodigoISO))
                {
                    return RespuestaDto.ParametrosIncorrectos(
                        "Creación fallida",
                        $"Ya existe un país con el código '{paisDto.CodigoISO}'");
                }

                var pais = new Paise
                {
                    Nombre = paisDto.Nombre,
                    CodigoIso = paisDto.CodigoISO.ToUpper(),
                    Activo = true,
                    FechaCreacion = DateTime.Now,
                    CreadoPorId = usuarioId
                };

                await _context.Paises.AddAsync(pais);
                await _context.SaveChangesAsync();

                return RespuestaDto.Exitoso(
                    "País creado",
                    $"El país '{pais.Nombre}' ha sido creado correctamente",
                    new PaisDto
                    {
                        Id = pais.Id,
                        Nombre = pais.Nombre,
                        CodigoISO = pais.CodigoIso,
                        Activo = pais.Activo,
                        FechaCreacion = pais.FechaCreacion,
                        CreadoPorId = pais.CreadoPorId
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear país {Nombre}", paisDto.Nombre);
                return RespuestaDto.ErrorInterno(ex.Message);
            }
        }

        /// <summary>
        /// Actualiza un país existente
        /// </summary>
        public async Task<RespuestaDto> ActualizarAsync(int id, PaisDto paisDto, Guid usuarioId)
        {
            try
            {
                var pais = await _context.Paises.FindAsync(id);
                if (pais == null)
                {
                    return RespuestaDto.NoEncontrado("País");
                }

                // Validar que no exista otro país con el mismo nombre o código
                if (pais.Nombre != paisDto.Nombre && await _context.Paises.AnyAsync(p => p.Nombre == paisDto.Nombre && p.Id != id))
                {
                    return RespuestaDto.ParametrosIncorrectos(
                        "Actualización fallida",
                        $"Ya existe un país con el nombre '{paisDto.Nombre}'");
                }

                if (pais.CodigoIso != paisDto.CodigoISO && await _context.Paises.AnyAsync(p => p.CodigoIso == paisDto.CodigoISO && p.Id != id))
                {
                    return RespuestaDto.ParametrosIncorrectos(
                        "Actualización fallida",
                        $"Ya existe un país con el código '{paisDto.CodigoISO}'");
                }

                pais.Nombre = paisDto.Nombre;
                pais.CodigoIso = paisDto.CodigoISO.ToUpper();
                pais.Activo = paisDto.Activo;
                pais.FechaModificacion = DateTime.Now;
                pais.ModificadoPorId = usuarioId;

                await _context.SaveChangesAsync();

                return RespuestaDto.Exitoso(
                    "País actualizado",
                    $"El país '{pais.Nombre}' ha sido actualizado correctamente",
                    new PaisDto
                    {
                        Id = pais.Id,
                        Nombre = pais.Nombre,
                        CodigoISO = pais.CodigoIso,
                        Activo = pais.Activo,
                        FechaCreacion = pais.FechaCreacion,
                        FechaModificacion = pais.FechaModificacion,
                        CreadoPorId = pais.CreadoPorId,
                        ModificadoPorId = pais.ModificadoPorId
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar país {Id}", id);
                return RespuestaDto.ErrorInterno(ex.Message);
            }
        }

        /// <summary>
        /// Elimina un país (desactivación lógica)
        /// </summary>
        public async Task<RespuestaDto> EliminarAsync(int id)
        {
            try
            {
                var pais = await _context.Paises.FindAsync(id);
                if (pais == null)
                {
                    return RespuestaDto.NoEncontrado("País");
                }

                // Verificar si tiene departamentos asociados
                var tieneDepartamentos = await _context.Departamentos.AnyAsync(d => d.PaisId == id && d.Activo);
                if (tieneDepartamentos)
                {
                    return RespuestaDto.ParametrosIncorrectos(
                        "Eliminación fallida",
                        $"No se puede eliminar el país '{pais.Nombre}' porque tiene departamentos asociados");
                }

                pais.Activo = false;
                pais.FechaModificacion = DateTime.Now;

                await _context.SaveChangesAsync();

                return RespuestaDto.Exitoso(
                    "País eliminado",
                    $"El país '{pais.Nombre}' ha sido eliminado correctamente",
                    null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar país {Id}", id);
                return RespuestaDto.ErrorInterno(ex.Message);
            }
        }

        /// <summary>
        /// Verifica si existe un país con el ID especificado
        /// </summary>
        public async Task<bool> ExisteAsync(int id)
        {
            return await _context.Paises.AnyAsync(p => p.Id == id);
        }

        /// <summary>
        /// Verifica si existe un país con el nombre especificado
        /// </summary>
        public async Task<bool> ExistePorNombreAsync(string nombre)
        {
            return await _context.Paises.AnyAsync(p => p.Nombre == nombre);
        }

        /// <summary>
        /// Verifica si existe un país con el código ISO especificado
        /// </summary>
        public async Task<bool> ExistePorCodigoAsync(string codigo)
        {
            return await _context.Paises.AnyAsync(p => p.CodigoIso == codigo);
        }

        /// <summary>
        /// Obtiene una lista paginada de países
        /// </summary>
        public async Task<PaginacionDto<PaisDto>> ObtenerPaginadoAsync(
            int pagina,
            int elementosPorPagina,
            string? busqueda = null)
        {
            // 1. Query base con navegación
            IQueryable<Paise> query = _context.Paises
                .Include(p => p.CreadoPor)
                .Include(p => p.ModificadoPor)
                .Include(p => p.Departamentos);

            // 2. Filtro de búsqueda
            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                busqueda = busqueda.ToLower();
                query = query.Where(p =>
                    p.Nombre.ToLower().Contains(busqueda) ||
                    p.CodigoIso.ToLower().Contains(busqueda));
            }

            // 3. Totales para la paginación
            int totalRegistros = await query.CountAsync();
            int totalPaginas = (int)Math.Ceiling((double)totalRegistros / elementosPorPagina);

            // 4. Recuperar entidades paginadas
            List<Paise> paises = await query
                .OrderBy(p => p.Nombre)
                .Skip((pagina - 1) * elementosPorPagina)
                .Take(elementosPorPagina)
                .AsNoTracking()
                .ToListAsync();

            // 5. Mapear a DTO con tu helper Mapping
            List<PaisDto> paisesDto = Mapping.ConvertirLista<Paise, PaisDto>(paises);

            // 6. Completar campos calculados que no tienen mapeo 1‑a‑1
            for (int i = 0; i < paises.Count; i++)
            {
                Paise entidad = paises[i];
                PaisDto dto = paisesDto[i];

                dto.CreadoPor = entidad.CreadoPor != null
                                 ? $"{entidad.CreadoPor.Nombre} {entidad.CreadoPor.Apellido}"
                                 : null;

                dto.ModificadoPor = entidad.ModificadoPor != null
                                    ? $"{entidad.ModificadoPor.Nombre} {entidad.ModificadoPor.Apellido}"
                                    : null;

                dto.DepartamentosCount = entidad.Departamentos?
                                                 .Count(d => d.Activo) ?? 0;
            }

            // 7. Construir y devolver el resultado paginado
            return new PaginacionDto<PaisDto>
            {
                Pagina = pagina,
                ElementosPorPagina = elementosPorPagina,
                TotalPaginas = totalPaginas,
                TotalRegistros = totalRegistros,
                Lista = paisesDto
            };
        }
    }
}
