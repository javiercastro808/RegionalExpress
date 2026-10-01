using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RegionalExpress.API.Models;
using RegionalExpress.API.Services;
using System.Security.Claims;
namespace RegionalExpress.API.Controllers;
[ApiController]
[Route("api/Ubicaciones")]
public class UbicacionesController(RegionalExpressContext context) : ControllerBase
{
    public record UbicacionRequest(decimal? Latitud, decimal? Longitud);
    public record PuntoMapa(decimal Latitud, decimal Longitud);
    private static PuntoMapa? Punto(decimal? lat,decimal? lng) => ReglasUbicacion.Valida(lat,lng) ? new(lat!.Value,lng!.Value) : null;
    [Authorize(Roles="MOTORISTA")]
    [HttpPost("servicios/{id:int}")]
    public async Task<IActionResult> Guardar(int id, UbicacionRequest request)
    {
        if (!ReglasUbicacion.Valida(request.Latitud,request.Longitud)) return BadRequest(new {mensaje="Coordenadas inválidas."});
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier),out var usuario)) return Unauthorized();
        var motorista=await context.Motoristas.FirstOrDefaultAsync(m=>m.IdUsuario==usuario&&m.Activo);
        if(motorista==null)return Forbid();
        if(!await context.Servicios.AnyAsync(s=>s.IdServicio==id&&s.IdMotorista==motorista.IdMotorista&&s.Activo&&s.FechaFinalizacion==null&&(s.Pedido==null||s.Pedido.ModalidadEntrega!="RECOGER")))
            return Conflict(new {mensaje="El servicio terminó o dejó de estar asignado a este motorista."});
        context.UbicacionesMotorista.Add(new UbicacionesMotoristum{IdServicio=id,IdMotorista=motorista.IdMotorista,Latitud=request.Latitud!.Value,Longitud=request.Longitud!.Value,FechaHora=DateTime.UtcNow});
        await context.SaveChangesAsync();return Ok(new {mensaje="Ubicación actualizada."});
    }
    [HttpGet("rastreo/{codigo}")]
    public async Task<IActionResult> Consultar(string codigo)
    {
        var s=await context.Servicios.AsNoTracking().Include(s=>s.Pedido).ThenInclude(p=>p!.IdRestauranteNavigation).Include(s=>s.Envio).FirstOrDefaultAsync(s=>s.CodigoRastreo==codigo.Trim().ToUpper());
        if(s==null)return NotFound();
        if(!await AccesoServicio.PuedeVerDatos(User,s,context))return Ok(new {disponible=false,mensaje="Inicia sesión con la cuenta del servicio para ver el mapa."});
        var origen=s.Pedido!=null?Punto(s.Pedido.IdRestauranteNavigation.Latitud,s.Pedido.IdRestauranteNavigation.Longitud):Punto(s.Envio?.LatitudOrigen,s.Envio?.LongitudOrigen);
        var destino=s.Pedido!=null?Punto(s.Pedido.LatitudEntrega,s.Pedido.LongitudEntrega):Punto(s.Envio?.LatitudDestino,s.Envio?.LongitudDestino);
        var activo=s.Activo&&s.FechaFinalizacion==null&&s.IdMotorista!=null&&s.Pedido?.ModalidadEntrega!="RECOGER";
        var u=activo?await context.UbicacionesMotorista.AsNoTracking().Where(u=>u.IdServicio==s.IdServicio&&u.IdMotorista==s.IdMotorista).OrderByDescending(u=>u.FechaHora).FirstOrDefaultAsync():null;
        return Ok(new {disponible=u!=null,origen,destino,latitud=u?.Latitud,longitud=u?.Longitud,
            fechaHora=u!=null?(DateTime?)DateTime.SpecifyKind(u.FechaHora,DateTimeKind.Utc):null,
            reciente=u!=null&&u.FechaHora>=DateTime.UtcNow.AddMinutes(-2),
            mensaje=origen==null?"El origen aún no tiene coordenadas registradas.":null});
    }
}
