using System;
using System.Collections.Generic;

namespace RegionalExpress.API.Models;

public partial class DireccionesUsuario
{
    public int IdDireccion { get; set; }

    public int IdUsuario { get; set; }

    public string? NombreDireccion { get; set; }

    public string Direccion { get; set; } = null!;

    public string? Referencia { get; set; }

    public decimal? Latitud { get; set; }

    public decimal? Longitud { get; set; }

    public bool Predeterminada { get; set; }

    public bool Activo { get; set; }

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
}
