using Microsoft.EntityFrameworkCore;
using RegionalExpress.API.Models;
using System.Security.Cryptography;
namespace RegionalExpress.API.Services;
public static class CrearMotoristaPrueba {
 public static async Task Ejecutar(RegionalExpressContext db) {
  const string correo="motorista2@regionalexpress.test";
  await using var tx=await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
  if(await db.Usuarios.AnyAsync(u=>u.Correo==correo)) {Console.WriteLine("La cuenta ya existe. No se modificó su contraseña ni su perfil.");return;}
  var rol=await db.Roles.SingleAsync(r=>r.NombreRol=="MOTORISTA"&&r.Activo);
  var clave=Convert.ToHexString(RandomNumberGenerator.GetBytes(12))+"!m2";
  var usuario=new Usuario{Nombre="Motorista",Apellido="2",Correo=correo,IdRol=rol.IdRol,Activo=true,FechaRegistro=DateTime.Now,PasswordHash=Claves.Crear(clave)};
  db.Motoristas.Add(new Motorista{IdUsuarioNavigation=usuario,Activo=true,Disponible=true,FechaRegistro=DateTime.Now});
  await db.SaveChangesAsync();await tx.CommitAsync();
  Console.WriteLine("Cuenta de prueba creada: "+correo);Console.WriteLine("Contraseña: "+clave);
 }
}
