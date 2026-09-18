using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RegionalExpress.API.Models;

namespace RegionalExpress.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProductosController : ControllerBase
{
    private readonly RegionalExpressContext _context;

    public ProductosController(RegionalExpressContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetProductos()
    {
        var productos = await _context.Productos
            .Where(p => p.Activo == true)
            .ToListAsync();

        return Ok(productos);
    }

    [HttpGet("categoria/{idCategoria}")]
    public async Task<IActionResult> GetProductosPorCategoria(int idCategoria)
    {
        var productos = await _context.Productos
            .Where(p =>
                p.IdCategoria == idCategoria &&
                p.Activo == true &&
                p.Disponible == true)
            .ToListAsync();

        return Ok(productos);
    }
}
