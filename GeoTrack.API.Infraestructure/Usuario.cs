using System;
using System.Collections.Generic;

namespace GeoTrack.API.Infrastructure;

public partial class Usuario
{
    public Guid Id { get; set; }

    public string NombreUsuario { get; set; } = null!;

    public string Contraseña { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public string Apellido { get; set; } = null!;

    public string Email { get; set; } = null!;

    public int RolId { get; set; }

    public bool Activo { get; set; }

    public DateTime FechaCreacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public DateTime? FechaUltimoAcceso { get; set; }

    public virtual ICollection<Ciudade> CiudadeCreadoPors { get; set; } = new List<Ciudade>();

    public virtual ICollection<Ciudade> CiudadeModificadoPors { get; set; } = new List<Ciudade>();

    public virtual ICollection<Departamento> DepartamentoCreadoPors { get; set; } = new List<Departamento>();

    public virtual ICollection<Departamento> DepartamentoModificadoPors { get; set; } = new List<Departamento>();

    public virtual ICollection<Log> Logs { get; set; } = new List<Log>();

    public virtual ICollection<Paise> PaiseCreadoPors { get; set; } = new List<Paise>();

    public virtual ICollection<Paise> PaiseModificadoPors { get; set; } = new List<Paise>();

    public virtual Role Rol { get; set; } = null!;

    public virtual ICollection<Token> Tokens { get; set; } = new List<Token>();

    public virtual ICollection<TokensExpirado> TokensExpirados { get; set; } = new List<TokensExpirado>();
}
