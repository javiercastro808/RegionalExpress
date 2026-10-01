namespace RegionalExpress.API.Services;
public static class ReglasUbicacion
{
    public static bool Valida(decimal? latitud, decimal? longitud) => latitud is >= -90 and <= 90 && longitud is >= -180 and <= 180;
}
