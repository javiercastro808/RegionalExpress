using System;
using System.Collections.Generic;

namespace RegionalExpress.API.Models;

public partial class Tarifa
{
    public int IdTarifa { get; set; }

    public string TipoServicio { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public decimal TarifaBase { get; set; }

    public decimal CostoPorKm { get; set; }

    public decimal CostoPorLibra { get; set; }

    public decimal MontoMinimo { get; set; }

    public bool Activo { get; set; }

    public DateTime FechaInicio { get; set; }

    public DateTime? FechaFin { get; set; }
}
