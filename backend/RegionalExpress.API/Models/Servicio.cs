using System;
using System.Collections.Generic;

namespace RegionalExpress.API.Models;

public partial class Servicio
{
    public int IdServicio { get; set; }

    public string CodigoRastreo { get; set; } = null!;

    public int IdCliente { get; set; }

    public int? IdMotorista { get; set; }

    public string TipoServicio { get; set; } = null!;

    public int IdEstadoActual { get; set; }

    public decimal Total { get; set; }

    public DateTime FechaCreacion { get; set; }

    public DateTime? FechaFinalizacion { get; set; }

    public bool Activo { get; set; }

    public virtual Envio? Envio { get; set; }

    public virtual ICollection<HistorialEstado> HistorialEstados { get; set; } = new List<HistorialEstado>();

    public virtual Usuario IdClienteNavigation { get; set; } = null!;

    public virtual EstadosServicio IdEstadoActualNavigation { get; set; } = null!;

    public virtual Motorista? IdMotoristaNavigation { get; set; }

    public virtual Pedido? Pedido { get; set; }

    public virtual ICollection<UbicacionesMotoristum> UbicacionesMotorista { get; set; } = new List<UbicacionesMotoristum>();
}
