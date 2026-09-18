using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RegionalExpress.API.Models;

namespace RegionalExpress.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class RestaurantesController : ControllerBase
{
    private readonly RegionalExpressContext _context;

    public RestaurantesController(RegionalExpressContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetRestaurantes()
    {
        var restaurantes = await _context.Restaurantes
            .Where(r => r.Activo == true)
            .ToListAsync();

        return Ok(restaurantes);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetRestaurante(int id)
    {
        var restaurante = await _context.Restaurantes
            .FirstOrDefaultAsync(r => r.IdRestaurante == id);

        if (restaurante == null)
            return NotFound(new { mensaje = "Restaurante no encontrado" });

        return Ok(restaurante);
    }
}
