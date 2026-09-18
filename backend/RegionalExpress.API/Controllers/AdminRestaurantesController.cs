using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RegionalExpress.API.Models;

namespace RegionalExpress.API.Controllers
{
    [Route("api/Admin/restaurantes")]
    [ApiController]
    [Authorize(Roles = "ADMIN_GENERAL")]
    public class AdminRestaurantesController : ControllerBase
    {
        private readonly RegionalExpressContext _context;

        public AdminRestaurantesController(
            RegionalExpressContext context)
        {
            _context = context;
        }


        // =========================================================
        // GET: api/Admin/restaurantes
        // LISTAR RESTAURANTES
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> GetRestaurantes()
        {
            var restaurantes =
                await _context.Restaurantes
                    .AsNoTracking()
                    .OrderByDescending(x => x.Activo)
                    .ThenBy(x => x.Nombre)
                    .Select(x => new
                    {
                        x.IdRestaurante,
                        x.Nombre,
                        x.Descripcion,
                        x.Direccion,
                        x.Telefono,
                        x.Correo,
                        x.Imagen,
                        x.HorarioApertura,
                        x.HorarioCierre,
                        x.Latitud,
                        x.Longitud,
                        x.Activo,
                        x.FechaRegistro,

                        CantidadProductos =
                            x.CategoriasMenus
                                .SelectMany(c => c.Productos)
                                .Count()
                    })
                    .ToListAsync();

            return Ok(restaurantes);
        }


        // =========================================================
        // GET: api/Admin/restaurantes/1
        // OBTENER RESTAURANTE
        // =========================================================

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetRestaurante(
            int id)
        {
            var restaurante =
                await _context.Restaurantes
                    .AsNoTracking()
                    .Where(x =>
                        x.IdRestaurante == id)
                    .Select(x => new
                    {
                        x.IdRestaurante,
                        x.Nombre,
                        x.Descripcion,
                        x.Direccion,
                        x.Telefono,
                        x.Correo,
                        x.Imagen,
                        x.HorarioApertura,
                        x.HorarioCierre,
                        x.Latitud,
                        x.Longitud,
                        x.Activo,
                        x.FechaRegistro
                    })
                    .FirstOrDefaultAsync();

            if (restaurante == null)
            {
                return NotFound(new
                {
                    mensaje =
                        "El restaurante no existe."
                });
            }

            return Ok(restaurante);
        }


        // =========================================================
        // POST: api/Admin/restaurantes
        // CREAR RESTAURANTE
        // =========================================================

        [HttpPost]
        public async Task<IActionResult> CrearRestaurante(
            CrearRestauranteRequest request)
        {
            if (string.IsNullOrWhiteSpace(
                request.Nombre))
            {
                return BadRequest(new
                {
                    mensaje =
                        "El nombre del restaurante es obligatorio."
                });
            }

            if (string.IsNullOrWhiteSpace(
                request.Direccion))
            {
                return BadRequest(new
                {
                    mensaje =
                        "La dirección es obligatoria."
                });
            }


            var nombre =
                request.Nombre.Trim();


            var existe =
                await _context.Restaurantes
                    .AnyAsync(x =>
                        x.Nombre == nombre);

            if (existe)
            {
                return BadRequest(new
                {
                    mensaje =
                        "Ya existe un restaurante con ese nombre."
                });
            }


            var restaurante =
                new Restaurante
                {
                    Nombre =
                        nombre,

                    Descripcion =
                        LimpiarTexto(
                            request.Descripcion),

                    Direccion =
                        request.Direccion.Trim(),

                    Telefono =
                        LimpiarTexto(
                            request.Telefono),

                    Correo =
                        LimpiarTexto(
                            request.Correo)
                            ?.ToLowerInvariant(),

                    Imagen =
                        LimpiarTexto(
                            request.Imagen),

                    HorarioApertura =
                        request.HorarioApertura,

                    HorarioCierre =
                        request.HorarioCierre,

                    Latitud =
                        request.Latitud,

                    Longitud =
                        request.Longitud,

                    Activo = true,

                    FechaRegistro =
                        DateTime.Now
                };


            _context.Restaurantes.Add(
                restaurante);

            await _context.SaveChangesAsync();


            return Ok(new
            {
                mensaje =
                    "Restaurante creado correctamente.",

                idRestaurante =
                    restaurante.IdRestaurante,

                restaurante.Nombre
            });
        }


        // =========================================================
        // PUT: api/Admin/restaurantes/1
        // EDITAR RESTAURANTE
        // =========================================================

        [HttpPut("{id:int}")]
        public async Task<IActionResult> EditarRestaurante(
            int id,
            EditarRestauranteRequest request)
        {
            var restaurante =
                await _context.Restaurantes
                    .FirstOrDefaultAsync(x =>
                        x.IdRestaurante == id);

            if (restaurante == null)
            {
                return NotFound(new
                {
                    mensaje =
                        "El restaurante no existe."
                });
            }


            if (string.IsNullOrWhiteSpace(
                request.Nombre))
            {
                return BadRequest(new
                {
                    mensaje =
                        "El nombre es obligatorio."
                });
            }


            if (string.IsNullOrWhiteSpace(
                request.Direccion))
            {
                return BadRequest(new
                {
                    mensaje =
                        "La dirección es obligatoria."
                });
            }


            var nombre =
                request.Nombre.Trim();


            var duplicado =
                await _context.Restaurantes
                    .AnyAsync(x =>
                        x.IdRestaurante != id
                        &&
                        x.Nombre == nombre);

            if (duplicado)
            {
                return BadRequest(new
                {
                    mensaje =
                        "Ya existe otro restaurante con ese nombre."
                });
            }


            restaurante.Nombre =
                nombre;

            restaurante.Descripcion =
                LimpiarTexto(
                    request.Descripcion);

            restaurante.Direccion =
                request.Direccion.Trim();

            restaurante.Telefono =
                LimpiarTexto(
                    request.Telefono);

            restaurante.Correo =
                LimpiarTexto(
                    request.Correo)
                    ?.ToLowerInvariant();

            restaurante.Imagen =
                LimpiarTexto(
                    request.Imagen);

            restaurante.HorarioApertura =
                request.HorarioApertura;

            restaurante.HorarioCierre =
                request.HorarioCierre;

            restaurante.Latitud =
                request.Latitud;

            restaurante.Longitud =
                request.Longitud;


            await _context.SaveChangesAsync();


            return Ok(new
            {
                mensaje =
                    "Restaurante actualizado correctamente."
            });
        }


        // =========================================================
        // PATCH: api/Admin/restaurantes/1/estado
        // ACTIVAR / DESACTIVAR
        // =========================================================

        [HttpPatch("{id:int}/estado")]
        public async Task<IActionResult> CambiarEstado(
            int id,
            CambiarEstadoRestauranteRequest request)
        {
            var restaurante =
                await _context.Restaurantes
                    .FirstOrDefaultAsync(x =>
                        x.IdRestaurante == id);

            if (restaurante == null)
            {
                return NotFound(new
                {
                    mensaje =
                        "El restaurante no existe."
                });
            }


            restaurante.Activo =
                request.Activo;


            await _context.SaveChangesAsync();


            return Ok(new
            {
                mensaje =
                    request.Activo
                        ? "Restaurante activado correctamente."
                        : "Restaurante desactivado correctamente.",

                restaurante.IdRestaurante,

                restaurante.Activo
            });
        }


        // =========================================================
        // DTO
        // =========================================================

        public class CrearRestauranteRequest
        {
            public string Nombre { get; set; } =
                string.Empty;

            public string? Descripcion { get; set; }

            public string Direccion { get; set; } =
                string.Empty;

            public string? Telefono { get; set; }

            public string? Correo { get; set; }

            public string? Imagen { get; set; }

            public TimeOnly? HorarioApertura { get; set; }

            public TimeOnly? HorarioCierre { get; set; }

            public decimal? Latitud { get; set; }

            public decimal? Longitud { get; set; }
        }


        public class EditarRestauranteRequest
        {
            public string Nombre { get; set; } =
                string.Empty;

            public string? Descripcion { get; set; }

            public string Direccion { get; set; } =
                string.Empty;

            public string? Telefono { get; set; }

            public string? Correo { get; set; }

            public string? Imagen { get; set; }

            public TimeOnly? HorarioApertura { get; set; }

            public TimeOnly? HorarioCierre { get; set; }

            public decimal? Latitud { get; set; }

            public decimal? Longitud { get; set; }
        }


        public class CambiarEstadoRestauranteRequest
        {
            public bool Activo { get; set; }
        }


        // =========================================================
        // UTILIDAD
        // =========================================================

        private static string? LimpiarTexto(
            string? texto)
        {
            if (string.IsNullOrWhiteSpace(
                texto))
            {
                return null;
            }

            return texto.Trim();
        }
    }
}