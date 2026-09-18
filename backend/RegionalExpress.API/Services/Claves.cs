using Microsoft.AspNetCore.Identity;
using System.Security.Cryptography;
using System.Text;

namespace RegionalExpress.API.Services;

public static class Claves
{
    private const string Prefijo = "RE$1$";
    private static readonly PasswordHasher<string> Hasher = new();
    public static bool EsHash(string valor) => valor.StartsWith(Prefijo, StringComparison.Ordinal);
    public static string Crear(string password) => Prefijo + Hasher.HashPassword("", password);

    public static bool Verificar(string almacenada, string password, bool permitirLegado, out bool actualizar)
    {
        actualizar = false;
        if (EsHash(almacenada))
        {
            try
            {
                var resultado = Hasher.VerifyHashedPassword("", almacenada[Prefijo.Length..], password);
                actualizar = resultado == PasswordVerificationResult.SuccessRehashNeeded;
                return resultado != PasswordVerificationResult.Failed;
            }
            catch (FormatException) { return false; }
        }
        if (!permitirLegado) return false;
        var coincide = CryptographicOperations.FixedTimeEquals(
            SHA256.HashData(Encoding.UTF8.GetBytes(almacenada)),
            SHA256.HashData(Encoding.UTF8.GetBytes(password)));
        actualizar = coincide;
        return coincide;
    }
}
