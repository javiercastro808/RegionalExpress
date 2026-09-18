using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using RegionalExpress.API.Models;

namespace RegionalExpress.API.Services;

public static class AccesoServicio
{
    public static async Task<bool> PuedeVerDatos(ClaimsPrincipal usuario, Servicio servicio, RegionalExpressContext db)
    {
        if (usuario.Identity?.IsAuthenticated != true || !int.TryParse(usuario.FindFirstValue(ClaimTypes.NameIdentifier), out var id)) return false;
        if (usuario.IsInRole("ADMIN_GENERAL") || servicio.IdCliente == id) return true;
        if (usuario.IsInRole("MOTORISTA"))
            return await db.Motoristas.AnyAsync(m => m.IdMotorista == servicio.IdMotorista && m.IdUsuario == id && m.Activo);
        if (usuario.IsInRole("ADMIN_RESTAURANTE"))
            return await db.UsuarioRestaurantes.AnyAsync(a => a.IdUsuario == id && a.Activo
                && db.Pedidos.Any(p => p.IdServicio == servicio.IdServicio && p.IdRestaurante == a.IdRestaurante));
        return false;
    }
}
