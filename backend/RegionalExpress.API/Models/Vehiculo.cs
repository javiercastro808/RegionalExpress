using System;
using System.Collections.Generic;

namespace RegionalExpress.API.Models;

public partial class Vehiculo
{
    public int IdVehiculo { get; set; }

    public int IdMotorista { get; set; }

    public string TipoVehiculo { get; set; } = null!;

    public string? Marca { get; set; }

    public string? Modelo { get; set; }

    public string? Placa { get; set; }

    public string? Color { get; set; }

    public bool Activo { get; set; }

    public virtual Motorista IdMotoristaNavigation { get; set; } = null!;
}
