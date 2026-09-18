using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RegionalExpress.API.Models;
using System.Security.Claims;

namespace RegionalExpress.API.Controllers;

[ApiController]
[Route("api/Cliente")]
[Authorize(Roles = "CLIENTE")]
public class ClienteController(RegionalExpressContext context) : ControllerBase
{
    [HttpGet("servicios")]
    public async Task<IActionResult> Servicios(int pagina = 1)
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id)) return Unauthorized();
        if (pagina < 1 || pagina > 10000) return BadRequest();
        var consulta = context.Servicios.AsNoTracking().Where(s => s.IdCliente == id);
        var total = await consulta.CountAsync();
        var items = await consulta.OrderByDescending(s => s.FechaCreacion).ThenByDescending(s => s.IdServicio)
            .Skip((pagina - 1) * 20).Take(20).Select(s => new {
                s.IdServicio, s.CodigoRastreo, s.TipoServicio, s.Total, s.FechaCreacion,
                estado = s.IdEstadoActualNavigation.NombreEstado
            }).ToListAsync();
        return Ok(new { items, total, pagina, tamanoPagina = 20 });
    }
}
