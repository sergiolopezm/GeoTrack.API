using System;
using System.Collections.Generic;

namespace GeoTrack.API.Infrastructure;

public partial class Departamento
{
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    public int PaisId { get; set; }

    public DateTime FechaCreacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public Guid? CreadoPorId { get; set; }

    public Guid? ModificadoPorId { get; set; }

    public bool Activo { get; set; }

    public virtual ICollection<Ciudade> Ciudades { get; set; } = new List<Ciudade>();

    public virtual Usuario? CreadoPor { get; set; }

    public virtual Usuario? ModificadoPor { get; set; }

    public virtual Paise Pais { get; set; } = null!;
}
