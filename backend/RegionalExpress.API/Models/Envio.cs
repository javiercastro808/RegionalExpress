using System;
using System.Collections.Generic;

namespace RegionalExpress.API.Models;

public partial class Envio
{
    public int IdEnvio { get; set; }

    public int IdServicio { get; set; }

    public string NombreRemitente { get; set; } = null!;

    public string TelefonoRemitente { get; set; } = null!;

    public string? CorreoRemitente { get; set; }

    public string NombreDestinatario { get; set; } = null!;

    public string TelefonoDestinatario { get; set; } = null!;

    public string? CorreoDestinatario { get; set; }

    public string DireccionOrigen { get; set; } = null!;

    public string? ReferenciaOrigen { get; set; }

    public decimal? LatitudOrigen { get; set; }

    public decimal? LongitudOrigen { get; set; }

    public string DireccionDestino { get; set; } = null!;

    public string? ReferenciaDestino { get; set; }

    public decimal? LatitudDestino { get; set; }

    public decimal? LongitudDestino { get; set; }

    public decimal Peso { get; set; }

    public string TipoPaquete { get; set; } = null!;

    public string? DescripcionPaquete { get; set; }

    public decimal TarifaBase { get; set; }

    public decimal CostoDistancia { get; set; }

    public decimal CostoPeso { get; set; }

    public decimal Total { get; set; }

    public DateTime FechaSolicitud { get; set; }

    public virtual Servicio IdServicioNavigation { get; set; } = null!;
}
