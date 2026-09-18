using System;
using System.Collections.Generic;

namespace RegionalExpress.API.Models;

public partial class Pedido
{
    public int IdPedido { get; set; }

    public int IdServicio { get; set; }

    public int IdRestaurante { get; set; }

    public string ModalidadEntrega { get; set; } = null!;

    public string? DireccionEntrega { get; set; }

    public string? ReferenciaEntrega { get; set; }

    public decimal? LatitudEntrega { get; set; }

    public decimal? LongitudEntrega { get; set; }

    public decimal CostoProductos { get; set; }

    public decimal CostoDelivery { get; set; }

    public decimal Total { get; set; }

    public string? Observaciones { get; set; }

    public DateTime FechaPedido { get; set; }

    public virtual ICollection<DetallePedido> DetallePedidos { get; set; } = new List<DetallePedido>();

    public virtual Restaurante IdRestauranteNavigation { get; set; } = null!;

    public virtual Servicio IdServicioNavigation { get; set; } = null!;
}
