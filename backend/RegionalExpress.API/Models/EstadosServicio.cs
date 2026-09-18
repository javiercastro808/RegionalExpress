using System;
using System.Collections.Generic;

namespace RegionalExpress.API.Models;

public partial class EstadosServicio
{
    public int IdEstado { get; set; }

    public string TipoServicio { get; set; } = null!;

    public string? Modalidad { get; set; }

    public string NombreEstado { get; set; } = null!;

    public int OrdenEstado { get; set; }

    public bool Activo { get; set; }

    public virtual ICollection<HistorialEstado> HistorialEstados { get; set; } = new List<HistorialEstado>();

    public virtual ICollection<Servicio> Servicios { get; set; } = new List<Servicio>();
}
