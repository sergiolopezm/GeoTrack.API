using System.ComponentModel.DataAnnotations;

namespace GeoTrack.API.Shared.InDTO.PaisInDto
{
    /// <summary>
    /// DTO para países
    /// </summary>
    public class PaisDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre del país es requerido")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres")]
        public string Nombre { get; set; } = null!;

        [Required(ErrorMessage = "El código ISO es requerido")]
        [StringLength(3, MinimumLength = 3, ErrorMessage = "El código ISO debe tener 3 caracteres")]
        public string CodigoISO { get; set; } = null!;

        public bool Activo { get; set; } = true;
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaModificacion { get; set; }
        public Guid? CreadoPorId { get; set; }
        public Guid? ModificadoPorId { get; set; }

        // Propiedades de navegación para UI
        public string? CreadoPor { get; set; }
        public string? ModificadoPor { get; set; }

        // Contador de departamentos asociados
        public int DepartamentosCount { get; set; }
    }
}
