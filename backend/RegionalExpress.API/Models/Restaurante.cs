using System;
using System.Collections.Generic;

namespace RegionalExpress.API.Models;

public partial class Restaurante
{
    public int IdRestaurante { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public string Direccion { get; set; } = null!;

    public string? Telefono { get; set; }

    public string? Correo { get; set; }

    public string? Imagen { get; set; }

    public TimeOnly? HorarioApertura { get; set; }

    public TimeOnly? HorarioCierre { get; set; }

    public decimal? Latitud { get; set; }

    public decimal? Longitud { get; set; }

    public bool Activo { get; set; }

    public DateTime FechaRegistro { get; set; }

    public virtual ICollection<CategoriasMenu> CategoriasMenus { get; set; } = new List<CategoriasMenu>();

    public virtual ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();

    public virtual ICollection<UsuarioRestaurante> UsuarioRestaurantes { get; set; } = new List<UsuarioRestaurante>();
}
