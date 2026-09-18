using System;
using System.Collections.Generic;

namespace RegionalExpress.API.Models;

public partial class UsuarioRestaurante
{
    public int IdUsuarioRestaurante { get; set; }

    public int IdUsuario { get; set; }

    public int IdRestaurante { get; set; }

    public bool Activo { get; set; }

    public DateTime FechaAsignacion { get; set; }

    public virtual Restaurante IdRestauranteNavigation { get; set; } = null!;

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
}
