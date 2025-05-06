using System.ComponentModel.DataAnnotations;

namespace GeoTrack.API.Shared.InDTO.CiudadInDto
{
    /// <summary>
    /// DTO para ciudades
    /// </summary>
    public class CiudadDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre de la ciudad es requerido")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres")]
        public string Nombre { get; set; } = null!;

        [Required(ErrorMessage = "El departamento es requerido")]
        public int DepartamentoId { get; set; }

        [StringLength(20, ErrorMessage = "El código postal no puede exceder los 20 caracteres")]
        public string? CodigoPostal { get; set; }

        public bool Activo { get; set; } = true;
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaModificacion { get; set; }
        public Guid? CreadoPorId { get; set; }
        public Guid? ModificadoPorId { get; set; }

        // Propiedades de navegación para UI
        public string? Departamento { get; set; }
        public string? Pais { get; set; }
        public int? PaisId { get; set; }
        public string? CreadoPor { get; set; }
        public string? ModificadoPor { get; set; }
    }
}
