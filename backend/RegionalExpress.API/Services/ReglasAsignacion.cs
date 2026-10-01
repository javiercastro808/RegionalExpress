namespace RegionalExpress.API.Services;
public static class ReglasAsignacion {
 public static bool PuedeAsignar(string tipo,string? modalidad,string estado,bool activo,bool finalizado,bool asignado,bool retiroManual) {
  if(!activo||finalizado||asignado||retiroManual||modalidad=="RECOGER")return false;
  var nombre=estado.ToUpperInvariant();
  if(nombre.Contains("CANCEL")||nombre.Contains("ENTREGAD")||nombre.Contains("FINALIZ")||nombre.Contains("COMPLET"))return false;
  return tipo=="ENVIO" || (tipo=="DELIVERY"&&(nombre.Contains("LISTO")||nombre.Contains("MOTORISTA")));
 }
}
