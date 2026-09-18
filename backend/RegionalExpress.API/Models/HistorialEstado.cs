using System;
using System.Collections.Generic;

namespace RegionalExpress.API.Models;

public partial class HistorialEstado
{
    public int IdHistorial { get; set; }

    public int IdServicio { get; set; }

    public int IdEstado { get; set; }

    public int IdUsuario { get; set; }

    public DateTime FechaHora { get; set; }

    public string? Observacion { get; set; }

    public virtual EstadosServicio IdEstadoNavigation { get; set; } = null!;

    public virtual Servicio IdServicioNavigation { get; set; } = null!;

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
}
