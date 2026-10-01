using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RegionalExpress.API.Models;
namespace RegionalExpress.API.Controllers;
[Route("api/[controller]")]
[ApiController]
public class ProductosController(RegionalExpressContext context) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetProductos([FromQuery] int? idRestaurante)
    {
        if (idRestaurante is <= 0) return BadRequest(new { mensaje = "Restaurante inválido." });
        var consulta = context.Productos.AsNoTracking().Where(p => p.Activo && p.IdCategoriaNavigation.Activo && p.IdCategoriaNavigation.IdRestauranteNavigation.Activo);
        if (idRestaurante.HasValue) consulta = consulta.Where(p => p.IdCategoriaNavigation.IdRestaurante == idRestaurante.Value);
        return Ok(await consulta.OrderBy(p => p.IdCategoriaNavigation.Nombre).ThenBy(p => p.Nombre).Select(p => new {
            p.IdProducto, p.IdCategoria, idRestaurante = p.IdCategoriaNavigation.IdRestaurante,
            categoria = p.IdCategoriaNavigation.Nombre, p.Nombre, p.Descripcion, p.Precio, p.Imagen, p.Disponible, p.Activo
        }).ToListAsync());
    }
    [HttpGet("categoria/{idCategoria:int}")]
    public async Task<IActionResult> GetProductosPorCategoria(int idCategoria) => Ok(await context.Productos.AsNoTracking()
        .Where(p => p.IdCategoria == idCategoria && p.Activo && p.Disponible && p.IdCategoriaNavigation.Activo && p.IdCategoriaNavigation.IdRestauranteNavigation.Activo)
        .Select(p => new {p.IdProducto,p.IdCategoria,idRestaurante=p.IdCategoriaNavigation.IdRestaurante,categoria=p.IdCategoriaNavigation.Nombre,p.Nombre,p.Descripcion,p.Precio,p.Imagen,p.Disponible}).ToListAsync());
}
