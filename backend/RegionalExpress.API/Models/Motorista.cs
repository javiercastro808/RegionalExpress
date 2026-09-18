using System;
using System.Collections.Generic;

namespace RegionalExpress.API.Models;

public partial class Motorista
{
    public int IdMotorista { get; set; }

    public int IdUsuario { get; set; }

    public string? NumeroLicencia { get; set; }

    public bool Disponible { get; set; }

    public bool Activo { get; set; }

    public DateTime FechaRegistro { get; set; }

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;

    public virtual ICollection<Servicio> Servicios { get; set; } = new List<Servicio>();

    public virtual ICollection<UbicacionesMotoristum> UbicacionesMotorista { get; set; } = new List<UbicacionesMotoristum>();

    public virtual ICollection<Vehiculo> Vehiculos { get; set; } = new List<Vehiculo>();
}
