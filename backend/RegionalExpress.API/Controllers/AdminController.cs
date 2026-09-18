using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RegionalExpress.API.Models;

namespace RegionalExpress.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "ADMIN_GENERAL")]
    public class AdminController : ControllerBase
    {
        private readonly RegionalExpressContext _context;

        public AdminController(
            RegionalExpressContext context
        )
        {
            _context = context;
        }


        // ====================================================
        // DASHBOARD
        // GET api/Admin/dashboard
        // ====================================================

        [HttpGet("dashboard")]
        public async Task<IActionResult> Dashboard()
        {
            var totalUsuarios =
                await _context.Usuarios.CountAsync();

            var usuariosActivos =
                await _context.Usuarios
                    .CountAsync(x => x.Activo);

            var totalRestaurantes =
                await _context.Restaurantes.CountAsync();

            var totalMotoristas =
                await _context.Motoristas.CountAsync();

            var totalServicios =
                await _context.Servicios.CountAsync();

            var serviciosActivos =
                await _context.Servicios
                    .CountAsync(x => x.Activo);

            var totalDelivery =
                await _context.Servicios
                    .CountAsync(
                        x => x.TipoServicio == "DELIVERY"
                    );

            var totalEnvios =
                await _context.Servicios
                    .CountAsync(
                        x => x.TipoServicio == "ENVIO"
                    );

            var ingresos =
                await _context.Servicios
                    .SumAsync(
                        x => (decimal?)x.Total
                    )
                ?? 0;

            return Ok(new
            {
                totalUsuarios,
                usuariosActivos,
                totalRestaurantes,
                totalMotoristas,
                totalServicios,
                serviciosActivos,
                totalDelivery,
                totalEnvios,
                ingresos
            });
        }


        // ====================================================
        // USUARIOS
        // GET api/Admin/usuarios
        // ====================================================

        [HttpGet("usuarios")]
        public async Task<IActionResult> Usuarios()
        {
            var usuarios =
                await _context.Usuarios
                    .AsNoTracking()
                    .Include(x => x.IdRolNavigation)
                    .OrderBy(x => x.IdUsuario)
                    .Select(x => new
                    {
                        idUsuario =
                            x.IdUsuario,

                        nombre =
                            x.Nombre,

                        correo =
                            x.Correo,

                        idRol =
                            x.IdRol,

                        rol =
                            x.IdRolNavigation.NombreRol,

                        activo =
                            x.Activo
                    })
                    .ToListAsync();

            return Ok(usuarios);
        }


        // ====================================================
        // ROLES
        // GET api/Admin/roles
        // ====================================================

        [HttpGet("roles")]
        public async Task<IActionResult> Roles()
        {
            var roles =
                await _context.Roles
                    .AsNoTracking()
                    .Where(x => x.Activo)
                    .OrderBy(x => x.IdRol)
                    .Select(x => new
                    {
                        idRol =
                            x.IdRol,

                        nombreRol =
                            x.NombreRol,

                        descripcion =
                            x.Descripcion
                    })
                    .ToListAsync();

            return Ok(roles);
        }


        // ====================================================
        // CREAR USUARIO
        // POST api/Admin/usuarios
        // ====================================================

        public class CrearUsuarioRequest
        {
            public string Nombre { get; set; } = "";

            public string Correo { get; set; } = "";

            public string Password { get; set; } = "";

            public int IdRol { get; set; }
        }


        [HttpPost("usuarios")]
        public async Task<IActionResult> CrearUsuario(
            CrearUsuarioRequest request
        )
        {
            string nombre =
                request.Nombre.Trim();

            string correo =
                request.Correo
                    .Trim()
                    .ToLower();


            if (string.IsNullOrWhiteSpace(nombre))
            {
                return BadRequest(new
                {
                    mensaje =
                        "Ingrese el nombre."
                });
            }


            if (string.IsNullOrWhiteSpace(correo))
            {
                return BadRequest(new
                {
                    mensaje =
                        "Ingrese el correo."
                });
            }


            if (string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new
                {
                    mensaje =
                        "Ingrese una contraseña."
                });
            }


            var existe =
                await _context.Usuarios
                    .AnyAsync(
                        x =>
                            x.Correo.ToLower()
                            ==
                            correo
                    );

            if (existe)
            {
                return BadRequest(new
                {
                    mensaje =
                        "Ya existe un usuario con ese correo."
                });
            }


            var rol =
                await _context.Roles
                    .FirstOrDefaultAsync(
                        x =>
                            x.IdRol == request.IdRol
                            &&
                            x.Activo
                    );

            if (rol == null)
            {
                return BadRequest(new
                {
                    mensaje =
                        "El rol seleccionado no es válido."
                });
            }


            var usuario =
                new Usuario
                {
                    Nombre =
                        nombre,

                    Correo =
                        correo,

                    IdRol =
                        request.IdRol,

                    PasswordHash =
                        RegionalExpress.API.Services.Claves.Crear(request.Password),

                    Activo =
                        true
                };


            _context.Usuarios.Add(usuario);

            await _context.SaveChangesAsync();


            return Ok(new
            {
                mensaje =
                    "Usuario creado correctamente.",

                idUsuario =
                    usuario.IdUsuario
            });
        }


        // ====================================================
        // EDITAR USUARIO
        // PUT api/Admin/usuarios/{id}
        // ====================================================

        public class EditarUsuarioRequest
        {
            public string Nombre { get; set; } = "";

            public string Correo { get; set; } = "";

            public int IdRol { get; set; }

            public string? Password { get; set; }
        }


        [HttpPut("usuarios/{id:int}")]
        public async Task<IActionResult> EditarUsuario(
            int id,
            EditarUsuarioRequest request
        )
        {
            var usuario =
                await _context.Usuarios
                    .FirstOrDefaultAsync(
                        x => x.IdUsuario == id
                    );

            if (usuario == null)
            {
                return NotFound(new
                {
                    mensaje =
                        "Usuario no encontrado."
                });
            }


            string nombre =
                request.Nombre.Trim();

            string correo =
                request.Correo
                    .Trim()
                    .ToLower();


            if (
                string.IsNullOrWhiteSpace(nombre)
                ||
                string.IsNullOrWhiteSpace(correo)
            )
            {
                return BadRequest(new
                {
                    mensaje =
                        "Nombre y correo son obligatorios."
                });
            }


            var correoExiste =
                await _context.Usuarios
                    .AnyAsync(
                        x =>
                            x.IdUsuario != id
                            &&
                            x.Correo.ToLower()
                            ==
                            correo
                    );

            if (correoExiste)
            {
                return BadRequest(new
                {
                    mensaje =
                        "El correo ya está registrado."
                });
            }


            var rolExiste =
                await _context.Roles
                    .AnyAsync(
                        x =>
                            x.IdRol == request.IdRol
                            &&
                            x.Activo
                    );

            if (!rolExiste)
            {
                return BadRequest(new
                {
                    mensaje =
                        "El rol seleccionado no es válido."
                });
            }


            usuario.Nombre =
                nombre;

            usuario.Correo =
                correo;

            usuario.IdRol =
                request.IdRol;


            if (
                !string.IsNullOrWhiteSpace(
                    request.Password
                )
            )
            {
                usuario.PasswordHash =
                    RegionalExpress.API.Services.Claves.Crear(request.Password);
            }


            await _context.SaveChangesAsync();


            return Ok(new
            {
                mensaje =
                    "Usuario actualizado correctamente."
            });
        }


        // ====================================================
        // ACTIVAR / DESACTIVAR
        // PATCH api/Admin/usuarios/{id}/estado
        // ====================================================

        public class CambiarEstadoRequest
        {
            public bool Activo { get; set; }
        }


        [HttpPatch("usuarios/{id:int}/estado")]
        public async Task<IActionResult> CambiarEstadoUsuario(
            int id,
            CambiarEstadoRequest request
        )
        {
            var usuario =
                await _context.Usuarios
                    .FirstOrDefaultAsync(
                        x => x.IdUsuario == id
                    );

            if (usuario == null)
            {
                return NotFound(new
                {
                    mensaje =
                        "Usuario no encontrado."
                });
            }


            usuario.Activo =
                request.Activo;


            await _context.SaveChangesAsync();


            return Ok(new
            {
                mensaje =
                    request.Activo
                        ? "Usuario activado correctamente."
                        : "Usuario desactivado correctamente."
            });
        }


        // ====================================================
        // SERVICIOS
        // GET api/Admin/servicios
        // ====================================================

        [HttpGet("servicios")]
        public async Task<IActionResult> Servicios()
        {
            var servicios =
                await _context.Servicios
                    .AsNoTracking()
                    .Include(x => x.IdClienteNavigation)
                    .Include(x => x.IdEstadoActualNavigation)
                    .OrderByDescending(x => x.FechaCreacion)
                    .Select(x => new
                    {
                        idMotorista = x.IdMotorista,
                        motorista = x.IdMotoristaNavigation != null ? x.IdMotoristaNavigation.IdUsuarioNavigation.Nombre : null,
                        modalidad = x.Pedido != null ? x.Pedido.ModalidadEntrega : null,
                        fechaFinalizacion = x.FechaFinalizacion,
                        idServicio =
                            x.IdServicio,

                        codigoRastreo =
                            x.CodigoRastreo,

                        tipoServicio =
                            x.TipoServicio,

                        cliente =
                            x.IdClienteNavigation.Nombre,

                        estado =
                            x.IdEstadoActualNavigation.NombreEstado,

                        total =
                            x.Total,

                        fechaCreacion =
                            x.FechaCreacion,

                        activo =
                            x.Activo
                    })
                    .ToListAsync();

            return Ok(servicios);
        }
    }
}