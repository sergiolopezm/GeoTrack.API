using System;
using System.Collections.Generic;

namespace GeoTrack.API.Infrastructure;

public partial class Ciudade
{
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    public int DepartamentoId { get; set; }

    public string? CodigoPostal { get; set; }

    public DateTime FechaCreacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public Guid? CreadoPorId { get; set; }

    public Guid? ModificadoPorId { get; set; }

    public bool Activo { get; set; }

    public virtual Usuario? CreadoPor { get; set; }

    public virtual Departamento Departamento { get; set; } = null!;

    public virtual Usuario? ModificadoPor { get; set; }
}
