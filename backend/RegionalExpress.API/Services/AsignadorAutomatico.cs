using Microsoft.EntityFrameworkCore;
using RegionalExpress.API.Models;
namespace RegionalExpress.API.Services;

public sealed class AsignadorAutomatico(IServiceScopeFactory scopes, ILogger<AsignadorAutomatico> logger) : BackgroundService
{
 public const string RetiroManual = "Asignación retirada por administrador; control manual.";
 protected override async Task ExecuteAsync(CancellationToken token)
 {
  while(!token.IsCancellationRequested){
   try { using var scope=scopes.CreateScope(); await Asignar(scope.ServiceProvider.GetRequiredService<RegionalExpressContext>(),token); }
   catch(OperationCanceledException) when(token.IsCancellationRequested){break;}
   catch(Exception ex){logger.LogError(ex,"No se pudo completar la asignación automática; se reintentará.");}
   try{await Task.Delay(TimeSpan.FromSeconds(15),token);}catch(OperationCanceledException){break;}
  }
 }
 public static async Task Asignar(RegionalExpressContext db,CancellationToken ct)
 {
  await using var tx=await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable,ct);
  var servicios=await db.Servicios.Include(s=>s.Pedido).Include(s=>s.IdEstadoActualNavigation)
   .Where(s=>s.Activo&&s.FechaFinalizacion==null&&s.IdMotorista==null&&(s.Pedido==null||s.Pedido.ModalidadEntrega!="RECOGER")
    && !db.HistorialEstados.Any(h=>h.IdServicio==s.IdServicio&&h.Observacion==RetiroManual))
   .OrderBy(s=>s.FechaCreacion).ThenBy(s=>s.IdServicio).ToListAsync(ct);
  foreach(var s in servicios){
   if(!ReglasAsignacion.PuedeAsignar(s.TipoServicio,s.Pedido?.ModalidadEntrega,s.IdEstadoActualNavigation.NombreEstado,s.Activo,s.FechaFinalizacion!=null,s.IdMotorista!=null,false))continue;
   var m=await db.Motoristas.Include(m=>m.IdUsuarioNavigation)
    .Where(m=>m.Activo&&m.Disponible&&m.IdUsuarioNavigation.Activo&&m.IdUsuarioNavigation.IdRolNavigation.Activo&&m.IdUsuarioNavigation.IdRolNavigation.NombreRol=="MOTORISTA"
     &&!db.Servicios.Any(a=>a.IdMotorista==m.IdMotorista&&a.Activo&&a.FechaFinalizacion==null))
    .OrderBy(m=>m.Servicios.Count()).ThenBy(m=>m.IdMotorista).FirstOrDefaultAsync(ct);
   if(m==null)break;
   var sistema=await db.Usuarios.FirstOrDefaultAsync(u=>u.Correo=="sistema-asignaciones@regionalexpress.invalid",ct);
   if(sistema==null){
    sistema=new Usuario{Nombre="Sistema",Apellido="Asignación automática",Correo="sistema-asignaciones@regionalexpress.invalid",IdRol=m.IdUsuarioNavigation.IdRol,Activo=false,FechaRegistro=DateTime.Now,PasswordHash=Claves.Crear(Guid.NewGuid().ToString()+Guid.NewGuid().ToString())};
    db.Usuarios.Add(sistema);await db.SaveChangesAsync(ct);
   }
   var modalidad=s.Pedido?.ModalidadEntrega;
   var estado=await db.EstadosServicios.Where(e=>e.Activo&&e.TipoServicio==s.TipoServicio&&e.NombreEstado.Contains("motorista")
    &&(s.TipoServicio=="DELIVERY"?e.Modalidad==modalidad:(e.Modalidad==null||e.Modalidad=="")))
    .OrderBy(e=>e.OrdenEstado).FirstOrDefaultAsync(ct);
   s.IdMotorista=m.IdMotorista;m.Disponible=false;
   if(estado!=null&&estado.OrdenEstado>=s.IdEstadoActualNavigation.OrdenEstado)s.IdEstadoActual=estado.IdEstado;
   db.HistorialEstados.Add(new HistorialEstado{IdServicio=s.IdServicio,IdEstado=s.IdEstadoActual,IdUsuario=sistema.IdUsuario,FechaHora=DateTime.Now,Observacion=$"Asignación automática a {m.IdUsuarioNavigation.Nombre}."});
   await db.SaveChangesAsync(ct);
  }
  await tx.CommitAsync(ct);
 }
}
