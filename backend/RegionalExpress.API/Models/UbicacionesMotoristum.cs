using System;
using System.Collections.Generic;

namespace RegionalExpress.API.Models;

public partial class UbicacionesMotoristum
{
    public int IdUbicacion { get; set; }

    public int IdServicio { get; set; }

    public int IdMotorista { get; set; }

    public decimal Latitud { get; set; }

    public decimal Longitud { get; set; }

    public DateTime FechaHora { get; set; }

    public virtual Motorista IdMotoristaNavigation { get; set; } = null!;

    public virtual Servicio IdServicioNavigation { get; set; } = null!;
}
