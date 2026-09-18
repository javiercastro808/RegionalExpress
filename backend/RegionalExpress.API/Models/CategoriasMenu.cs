using System;
using System.Collections.Generic;

namespace RegionalExpress.API.Models;

public partial class CategoriasMenu
{
    public int IdCategoria { get; set; }

    public int IdRestaurante { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public bool Activo { get; set; }

    public virtual Restaurante IdRestauranteNavigation { get; set; } = null!;

    public virtual ICollection<Producto> Productos { get; set; } = new List<Producto>();
}
