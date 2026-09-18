using System;
using System.Collections.Generic;

namespace RegionalExpress.API.Models;

public partial class Usuario
{
    public int IdUsuario { get; set; }

    public int IdRol { get; set; }

    public string Nombre { get; set; } = null!;

    public string Apellido { get; set; } = null!;

    public string Correo { get; set; } = null!;

    public string? Telefono { get; set; }

    public string PasswordHash { get; set; } = null!;

    public bool Activo { get; set; }

    public DateTime FechaRegistro { get; set; }

    public virtual ICollection<Auditorium> Auditoria { get; set; } = new List<Auditorium>();

    public virtual ICollection<DireccionesUsuario> DireccionesUsuarios { get; set; } = new List<DireccionesUsuario>();

    public virtual ICollection<HistorialEstado> HistorialEstados { get; set; } = new List<HistorialEstado>();

    public virtual Role IdRolNavigation { get; set; } = null!;

    public virtual Motorista? Motorista { get; set; }

    public virtual ICollection<Servicio> Servicios { get; set; } = new List<Servicio>();

    public virtual ICollection<UsuarioRestaurante> UsuarioRestaurantes { get; set; } = new List<UsuarioRestaurante>();
}
