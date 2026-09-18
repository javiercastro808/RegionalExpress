using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RegionalExpress.API.Models;
using System.Security.Claims;

namespace RegionalExpress.API.Controllers;

[ApiController]
[Route("api/Ubicaciones")]
public class UbicacionesController(RegionalExpressContext context) : ControllerBase
{
    public record UbicacionRequest(decimal Latitud, decimal Longitud);

    [Authorize(Roles = "MOTORISTA")]
    [HttpPost("servicios/{id:int}")]
    public async Task<IActionResult> Guardar(int id, UbicacionRequest request)
    {
        if (request.Latitud is < -90 or > 90 || request.Longitud is < -180 or > 180)
            return BadRequest(new { mensaje = "Coordenadas inválidas." });
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("idUsuario")?.Value ?? User.FindFirst("sub")?.Value;
        if (!int.TryParse(claim, out var idUsuario)) return Unauthorized();
        var motorista = await context.Motoristas.FirstOrDefaultAsync(m => m.IdUsuario == idUsuario && m.Activo);
        if (motorista == null) return Forbid();
        if (!await context.Servicios.AnyAsync(s => s.IdServicio == id && s.IdMotorista == motorista.IdMotorista && s.Activo && s.FechaFinalizacion == null))
            return Conflict(new { mensaje = "El servicio ya finalizó o dejó de estar asignado a este motorista." });
        context.UbicacionesMotorista.Add(new UbicacionesMotoristum {
            IdServicio = id, IdMotorista = motorista.IdMotorista,
            Latitud = request.Latitud, Longitud = request.Longitud, FechaHora = DateTime.UtcNow
        });
        await context.SaveChangesAsync();
        return Ok(new { mensaje = "Ubicación actualizada." });
    }

    // El código de rastreo permite consultar únicamente la última posición del servicio activo.
    [HttpGet("rastreo/{codigo}")]
    public async Task<IActionResult> Consultar(string codigo)
    {
        var servicio = await context.Servicios.AsNoTracking().FirstOrDefaultAsync(s => s.CodigoRastreo == codigo.Trim().ToUpper());
        if (servicio == null) return NotFound();
        if (!await RegionalExpress.API.Services.AccesoServicio.PuedeVerDatos(User, servicio, context))
            return Ok(new { disponible = false, mensaje = "Inicia sesión con la cuenta del servicio para consultar la ubicación del motorista." });
        if (!servicio.Activo || servicio.FechaFinalizacion != null || servicio.IdMotorista == null)
            return Ok(new { disponible = false });
        var ubicacion = await context.UbicacionesMotorista.AsNoTracking()
            .Where(u => u.IdServicio == servicio.IdServicio && u.IdMotorista == servicio.IdMotorista)
            .OrderByDescending(u => u.FechaHora).FirstOrDefaultAsync();
        if (ubicacion == null) return Ok(new { disponible = false });
        return Ok(new { disponible = true, ubicacion.Latitud, ubicacion.Longitud,
            fechaHora = DateTime.SpecifyKind(ubicacion.FechaHora, DateTimeKind.Utc),
            reciente = ubicacion.FechaHora >= DateTime.UtcNow.AddMinutes(-2) });
    }
}
