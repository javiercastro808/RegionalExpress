using RegionalExpress.API.Services;

static void Comprobar(bool resultado, string caso)
{
    if (!resultado) throw new Exception("Falló: " + caso);
    Console.WriteLine("OK: " + caso);
}
const string clave = "Prueba-solo-test-2026!";
var hash = Claves.Crear(clave);
Comprobar(hash != clave && Claves.EsHash(hash), "No se almacena la contraseña original");
Comprobar(hash != Claves.Crear(clave), "Sal aleatoria por contraseña");
Comprobar(Claves.Verificar(hash, clave, false, out _), "Acceso con clave correcta");
Comprobar(!Claves.Verificar(hash, "incorrecta", false, out _), "Rechazo de clave incorrecta");
Comprobar(!Claves.Verificar(clave, clave, false, out _), "Producción rechaza texto heredado");
Comprobar(Claves.Verificar(clave, clave, true, out var migrar) && migrar, "Desarrollo migra cuenta heredada");
Comprobar(!Claves.Verificar(clave, "incorrecta", true, out _), "Legado no acepta clave incorrecta");
Comprobar(!Claves.Verificar("RE$1$invalid", clave, false, out _), "Hash dañado no autentica");

Comprobar(ReglasUbicacion.Valida(0,0), "Coordenadas cero válidas");
Comprobar(ReglasUbicacion.Valida(14.6m,-90.5m), "Punto válido de Guatemala");
Comprobar(!ReglasUbicacion.Valida(null,-90m), "Latitud obligatoria");
Comprobar(!ReglasUbicacion.Valida(14m,null), "Longitud obligatoria");
Comprobar(!ReglasUbicacion.Valida(91m,0), "Latitud fuera de rango");
Comprobar(!ReglasUbicacion.Valida(0,-181m), "Longitud fuera de rango");

Comprobar(ReglasAsignacion.PuedeAsignar("DELIVERY","DOMICILIO","Listo para recoger",true,false,false,false),"Pedido listo admite asignación");
Comprobar(!ReglasAsignacion.PuedeAsignar("DELIVERY","DOMICILIO","Preparando pedido",true,false,false,false),"No salta preparación del restaurante");
Comprobar(!ReglasAsignacion.PuedeAsignar("DELIVERY","RECOGER","Listo para recoger",true,false,false,false),"Recoger excluido");
Comprobar(ReglasAsignacion.PuedeAsignar("ENVIO",null,"Solicitud recibida",true,false,false,false),"Envío pendiente admite asignación");
Comprobar(!ReglasAsignacion.PuedeAsignar("ENVIO",null,"Cancelado",true,false,false,false),"Cancelado excluido");
Comprobar(!ReglasAsignacion.PuedeAsignar("ENVIO",null,"Solicitud recibida",true,false,false,true),"Retiro administrativo respeta control manual");
Comprobar(!ReglasAsignacion.PuedeAsignar("ENVIO",null,"Solicitud recibida",true,false,true,false),"No reemplaza asignación existente");
Comprobar(!ReglasAsignacion.PuedeAsignar("ENVIO",null,"Entregado",false,true,false,false),"Finalizado excluido");
