using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using RegionalExpress.API.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ============================================================
// CONTROLADORES
// ============================================================

builder.Services.AddControllers();
builder.Services.AddScoped<RegionalExpress.API.Services.ConfirmacionCorreo>();

// ============================================================
// BASE DE DATOS
// ============================================================

builder.Services.AddDbContext<RegionalExpressContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "Server=(localdb)\\MSSQLLocalDB;Database=RegionalExpressDB;Trusted_Connection=True;TrustServerCertificate=True;"
    )
);

// ============================================================
// CORS
// ============================================================

builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirTodo", policy =>
    {
        policy
            .WithOrigins(builder.Configuration.GetSection("Cors:Origins").Get<string[]>()
                ?? (builder.Environment.IsDevelopment() ? new[] { "http://localhost:4200", "http://localhost:4201", "http://localhost:4300" } : Array.Empty<string>()))
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// ============================================================
// JWT
// ============================================================

var jwtKey =
    builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Configura Jwt:Key mediante appsettings privado o variables de entorno.");

var jwtIssuer =
    builder.Configuration["Jwt:Issuer"]
    ?? "RegionalExpress";

var jwtAudience =
    builder.Configuration["Jwt:Audience"]
    ?? "RegionalExpress";

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme =
            JwtBearerDefaults.AuthenticationScheme;

        options.DefaultChallengeScheme =
            JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                var idTexto = context.Principal?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var rol = context.Principal?.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
                if (!int.TryParse(idTexto, out var id)) { context.Fail("Sesión no válida."); return; }
                var db = context.HttpContext.RequestServices.GetRequiredService<RegionalExpressContext>();
                if (!await db.Usuarios.AnyAsync(u => u.IdUsuario == id && u.Activo
                    && u.IdRolNavigation.Activo && u.IdRolNavigation.NombreRol == rol))
                    context.Fail("La cuenta o su rol ya no están activos.");
            }
        };
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                ValidIssuer = jwtIssuer,
                ValidAudience = jwtAudience,

                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtKey)
                    ),

                ClockSkew = TimeSpan.Zero
            };
    });

builder.Services.AddAuthorization();

// ============================================================
// SWAGGER
// ============================================================

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    // IMPORTANTE:
    // Evita conflictos cuando dos controladores tienen DTO
    // con el mismo nombre.
    //
    // Ejemplo:
    // MotoristaController.CambiarEstadoMotoristaRequest
    // AdminMotoristasController.CambiarEstadoMotoristaRequest

    options.CustomSchemaIds(type =>
        (type.FullName ?? type.Name)
            .Replace("+", ".")
    );

    // JWT EN SWAGGER

    options.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            Name = "Authorization",

            Description =
                "Ingrese el token JWT. Ejemplo: Bearer eyJhbGciOi...",

            In = ParameterLocation.Header,

            Type = SecuritySchemeType.Http,

            Scheme = "bearer",

            BearerFormat = "JWT"
        }
    );

    options.AddSecurityRequirement(document =>
        new OpenApiSecurityRequirement
        {
            [
                new OpenApiSecuritySchemeReference(
                    "Bearer",
                    document
                )
            ] = new List<string>()
        }
    );
});

// ============================================================
// CONSTRUIR APLICACIÓN
// ============================================================

var app = builder.Build();

if (args.Contains("--inicializar-demo"))
{
    using var scope = app.Services.CreateScope();
    await RegionalExpress.API.Services.InicializarDemo.Ejecutar(scope.ServiceProvider.GetRequiredService<RegionalExpressContext>(), builder.Configuration);
    return;
}

// Exporta solamente la estructura; no consulta ni copia datos de clientes.
if (args.Contains("--exportar-esquema"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<RegionalExpressContext>();
    Console.WriteLine(db.Database.GenerateCreateScript());
    return;
}

// Conversión explícita e idempotente de las contraseñas heredadas de este proyecto.
if (args.Contains("--migrar-passwords"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<RegionalExpressContext>();
    var usuarios = await db.Usuarios.ToListAsync();
    var cantidad = 0;
    foreach (var usuario in usuarios)
        if (!RegionalExpress.API.Services.Claves.EsHash(usuario.PasswordHash))
        {
            usuario.PasswordHash = RegionalExpress.API.Services.Claves.Crear(usuario.PasswordHash);
            cantidad++;
        }
    await db.SaveChangesAsync();
    Console.WriteLine($"Contraseñas migradas: {cantidad}. No se modificaron las claves de acceso de los usuarios.");
    return;
}

// ============================================================
// SWAGGER EN DESARROLLO
// ============================================================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI();
}

// ============================================================
// CORS
// ============================================================

app.UseCors("PermitirTodo");

// ============================================================
// AUTENTICACIÓN / AUTORIZACIÓN
// IMPORTANTE: Authentication va antes de Authorization
// ============================================================

app.UseAuthentication();

app.UseAuthorization();

// En publicación, Angular se aloja en wwwroot junto con esta API.
app.UseDefaultFiles();
app.UseStaticFiles();

// ============================================================
// CONTROLADORES
// ============================================================

app.MapControllers();
app.MapGet("/api/salud", () => Results.Ok(new { estado = "Disponible" }));
app.MapFallback("/api/{**ruta}", () => Results.NotFound());
app.MapFallbackToFile("index.html");

// ============================================================
// EJECUTAR
// ============================================================

app.Run();
