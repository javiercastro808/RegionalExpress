using Microsoft.EntityFrameworkCore;
using RegionalExpress.API.Models;

namespace RegionalExpress.API.Services;

public static class InicializarDemo
{
    public static async Task Ejecutar(RegionalExpressContext db, IConfiguration config)
    {
        var claves = new[] { "Admin", "Restaurante", "Motorista", "Cliente" }
            .ToDictionary(nombre => nombre, nombre => config[$"Demo:{nombre}Password"] ?? "");
        if (claves.Values.Any(c => c.Length < 12))
            throw new InvalidOperationException("Configura las cuatro claves Demo:AdminPassword, RestaurantePassword, MotoristaPassword y ClientePassword (mínimo 12 caracteres).");
        await db.Database.EnsureCreatedAsync();
        if (await db.Usuarios.AnyAsync() || await db.Restaurantes.AnyAsync() || await db.Roles.AnyAsync() || await db.EstadosServicios.AnyAsync())
            throw new InvalidOperationException("La inicialización requiere una base vacía. No se modificaron los datos existentes.");
        await using var transaccion = await db.Database.BeginTransactionAsync();
        var roles = new[] { "ADMIN_GENERAL", "ADMIN_RESTAURANTE", "MOTORISTA", "CLIENTE" }
            .ToDictionary(nombre => nombre, nombre => new Role { NombreRol = nombre, Activo = true });
        Usuario Cuenta(string nombre, string rol) => new() {
            Nombre = nombre, Apellido = "Demostración", Correo = nombre.ToLowerInvariant() + "@regionalexpress.test",
            IdRolNavigation = roles[rol], PasswordHash = Claves.Crear(claves[nombre]), Activo = true, FechaRegistro = DateTime.Now
        };
        var admin = Cuenta("Admin", "ADMIN_GENERAL");
        var encargado = Cuenta("Restaurante", "ADMIN_RESTAURANTE");
        var motorista = Cuenta("Motorista", "MOTORISTA");
        var cliente = Cuenta("Cliente", "CLIENTE");
        db.Usuarios.AddRange(admin, encargado, motorista, cliente);
        db.Motoristas.Add(new Motorista { IdUsuarioNavigation = motorista, Activo = true, Disponible = true, NumeroLicencia = "DEMO-001", FechaRegistro = DateTime.Now });
        var restaurante = new Restaurante { Nombre = "Regional Express Demo", Descripcion = "Restaurante de demostración para pruebas del proyecto estudiantil.", Direccion = "Dirección ficticia de demostración", Activo = true, FechaRegistro = DateTime.Now };
        db.UsuarioRestaurantes.Add(new UsuarioRestaurante { IdUsuarioNavigation = encargado, IdRestauranteNavigation = restaurante, Activo = true, FechaAsignacion = DateTime.Now });
        var categoria = new CategoriasMenu { Nombre = "Menú de demostración", IdRestauranteNavigation = restaurante, Activo = true };
        void Producto(string nombre, string descripcion, decimal precio, string imagen) => db.Productos.Add(new Producto {
            Nombre = nombre, Descripcion = descripcion, Precio = precio, Imagen = null,
            IdCategoriaNavigation = categoria, Activo = true, Disponible = true, FechaRegistro = DateTime.Now
        });
        Producto("Desayuno Chapín", "Huevos, frijoles, platano y tortillas", 30m, "desayuno-chapin");
        Producto("Pollo a la plancha", "Pollo a la plancha acompañado de papas y ensalada", 45m, "pollo-plancha");
        Producto("Hamburguesa Regional", "Hamburguesa de carne, queso, vegetales y papas", 40m, "hamburguesa-regional");
        Producto("Gaseosa", "Bebida gaseosa personal", 10m, "gaseosa");
        void Estados(string tipo, string? modalidad, params string[] nombres)
        {
            for (int i = 0; i < nombres.Length; i++) db.EstadosServicios.Add(new EstadosServicio {
                TipoServicio = tipo, Modalidad = modalidad, NombreEstado = nombres[i], OrdenEstado = i + 1, Activo = true
            });
        }
        Estados("DELIVERY", "DOMICILIO", "Pedido recibido", "Preparando pedido", "Listo para recoger", "Motorista asignado", "En ruta", "Entregado");
        Estados("DELIVERY", "RECOGER", "Pedido recibido", "Preparando pedido", "Listo para recoger", "Recogido");
        Estados("ENVIO", null, "Solicitud recibida", "Motorista asignado", "Recogiendo paquete", "Paquete recogido", "En ruta", "Entregado");
        await db.SaveChangesAsync();
        await transaccion.CommitAsync();
        Console.WriteLine("Base de demostración creada. Cuentas: admin, restaurante, motorista y cliente @regionalexpress.test. Claves: las configuradas privadamente.");
    }
}
