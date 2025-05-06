using System;
using System.Collections.Generic;

namespace GeoTrack.API.Infrastructure;

public partial class Acceso
{
    public int Id { get; set; }

    public string Sitio { get; set; } = null!;

    public string Contraseña { get; set; } = null!;

    public DateTime FechaCreacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public bool Activo { get; set; }
}
