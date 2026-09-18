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
