using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RegionalExpress.API.Models;

namespace RegionalExpress.API.Controllers
{
    [Route("api/Admin/motoristas")]
    [ApiController]
    [Authorize(Roles = "ADMIN_GENERAL")]
    public class AdminMotoristasController : ControllerBase
    {
        private readonly RegionalExpressContext _context;

        public AdminMotoristasController(
            RegionalExpressContext context)
        {
            _context = context;
        }


        // =========================================================
        // GET: api/Admin/motoristas
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> GetMotoristas()
        {
            var motoristas =
                await _context.Motoristas
                    .AsNoTracking()
                    .Include(x =>
                        x.IdUsuarioNavigation)
                    .OrderByDescending(x =>
                        x.Activo)
                    .ThenBy(x =>
                        x.IdUsuarioNavigation.Nombre)
                    .Select(x => new
                    {
                        x.IdMotorista,

                        x.IdUsuario,

                        Nombre =
                            x.IdUsuarioNavigation.Nombre,

                        Correo =
                            x.IdUsuarioNavigation.Correo,

                        UsuarioActivo =
                            x.IdUsuarioNavigation.Activo,

                        x.NumeroLicencia,

                        x.Disponible,

                        x.Activo,

                        x.FechaRegistro,

                        CantidadServicios =
                            x.Servicios.Count(),

                        CantidadVehiculos =
                            x.Vehiculos.Count()
                    })
                    .ToListAsync();


            return Ok(motoristas);
        }


        // =========================================================
        // GET: api/Admin/motoristas/usuarios-disponibles
        //
        // Usuarios con rol MOTORISTA que todavía no tienen
        // registro en tabla Motoristas.
        // =========================================================

        [HttpGet("usuarios-disponibles")]
        public async Task<IActionResult>
            GetUsuariosDisponibles()
        {
            var usuarios =
                await _context.Usuarios
                    .AsNoTracking()
                    .Where(u =>
                        u.Activo
                        &&
                        u.IdRolNavigation.NombreRol
                            == "MOTORISTA"
                        &&
                        !_context.Motoristas
                            .Any(m =>
                                m.IdUsuario
                                == u.IdUsuario))
                    .OrderBy(u =>
                        u.Nombre)
                    .Select(u => new
                    {
                        u.IdUsuario,
                        u.Nombre,
                        u.Correo
                    })
                    .ToListAsync();


            return Ok(usuarios);
        }


        // =========================================================
        // GET: api/Admin/motoristas/1
        // =========================================================

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetMotorista(
            int id)
        {
            var motorista =
                await _context.Motoristas
                    .AsNoTracking()
                    .Where(x =>
                        x.IdMotorista == id)
                    .Select(x => new
                    {
                        x.IdMotorista,

                        x.IdUsuario,

                        Nombre =
                            x.IdUsuarioNavigation.Nombre,

                        Correo =
                            x.IdUsuarioNavigation.Correo,

                        x.NumeroLicencia,

                        x.Disponible,

                        x.Activo,

                        x.FechaRegistro
                    })
                    .FirstOrDefaultAsync();


            if (motorista == null)
            {
                return NotFound(new
                {
                    mensaje =
                        "El motorista no existe."
                });
            }


            return Ok(motorista);
        }


        // =========================================================
        // POST: api/Admin/motoristas
        // =========================================================

        [HttpPost]
        public async Task<IActionResult> CrearMotorista(
            CrearMotoristaRequest request)
        {
            if (request.IdUsuario <= 0)
            {
                return BadRequest(new
                {
                    mensaje =
                        "Seleccione un usuario motorista."
                });
            }


            var usuario =
                await _context.Usuarios
                    .Include(x =>
                        x.IdRolNavigation)
                    .FirstOrDefaultAsync(x =>
                        x.IdUsuario
                        == request.IdUsuario);


            if (usuario == null)
            {
                return NotFound(new
                {
                    mensaje =
                        "El usuario no existe."
                });
            }


            if (!usuario.Activo)
            {
                return BadRequest(new
                {
                    mensaje =
                        "El usuario seleccionado está inactivo."
                });
            }


            if (
                usuario.IdRolNavigation.NombreRol
                != "MOTORISTA"
            )
            {
                return BadRequest(new
                {
                    mensaje =
                        "El usuario seleccionado no tiene rol MOTORISTA."
                });
            }


            var existe =
                await _context.Motoristas
                    .AnyAsync(x =>
                        x.IdUsuario
                        == request.IdUsuario);


            if (existe)
            {
                return BadRequest(new
                {
                    mensaje =
                        "El usuario ya está registrado como motorista."
                });
            }


            var motorista =
                new Motorista
                {
                    IdUsuario =
                        request.IdUsuario,

                    NumeroLicencia =
                        LimpiarTexto(
                            request.NumeroLicencia),

                    Disponible =
                        request.Disponible,

                    Activo = true,

                    FechaRegistro =
                        DateTime.Now
                };


            _context.Motoristas.Add(
                motorista);

            await _context.SaveChangesAsync();


            return Ok(new
            {
                mensaje =
                    "Motorista registrado correctamente.",

                motorista.IdMotorista,

                motorista.IdUsuario,

                usuario.Nombre
            });
        }


        // =========================================================
        // PUT: api/Admin/motoristas/1
        // =========================================================

        [HttpPut("{id:int}")]
        public async Task<IActionResult> EditarMotorista(
            int id,
            EditarMotoristaRequest request)
        {
            var motorista =
                await _context.Motoristas
                    .FirstOrDefaultAsync(x =>
                        x.IdMotorista == id);


            if (motorista == null)
            {
                return NotFound(new
                {
                    mensaje =
                        "El motorista no existe."
                });
            }


            motorista.NumeroLicencia =
                LimpiarTexto(
                    request.NumeroLicencia);

            motorista.Disponible =
                request.Disponible;


            await _context.SaveChangesAsync();


            return Ok(new
            {
                mensaje =
                    "Motorista actualizado correctamente."
            });
        }


        // =========================================================
        // PATCH: api/Admin/motoristas/1/estado
        // =========================================================

        [HttpPatch("{id:int}/estado")]
        public async Task<IActionResult> CambiarEstado(
            int id,
            CambiarEstadoMotoristaRequest request)
        {
            var motorista =
                await _context.Motoristas
                    .FirstOrDefaultAsync(x =>
                        x.IdMotorista == id);


            if (motorista == null)
            {
                return NotFound(new
                {
                    mensaje =
                        "El motorista no existe."
                });
            }


            motorista.Activo =
                request.Activo;


            if (!request.Activo)
            {
                motorista.Disponible =
                    false;
            }


            await _context.SaveChangesAsync();


            return Ok(new
            {
                mensaje =
                    request.Activo
                        ? "Motorista activado correctamente."
                        : "Motorista desactivado correctamente.",

                motorista.IdMotorista,

                motorista.Activo,

                motorista.Disponible
            });
        }


        // =========================================================
        // PATCH: api/Admin/motoristas/1/disponibilidad
        // =========================================================

        [HttpPatch("{id:int}/disponibilidad")]
        public async Task<IActionResult>
            CambiarDisponibilidad(
                int id,
                CambiarDisponibilidadRequest request)
        {
            var motorista =
                await _context.Motoristas
                    .FirstOrDefaultAsync(x =>
                        x.IdMotorista == id);


            if (motorista == null)
            {
                return NotFound(new
                {
                    mensaje =
                        "El motorista no existe."
                });
            }


            if (!motorista.Activo)
            {
                return BadRequest(new
                {
                    mensaje =
                        "No se puede cambiar la disponibilidad de un motorista inactivo."
                });
            }


            motorista.Disponible =
                request.Disponible;


            await _context.SaveChangesAsync();


            return Ok(new
            {
                mensaje =
                    request.Disponible
                        ? "Motorista marcado como disponible."
                        : "Motorista marcado como no disponible.",

                motorista.IdMotorista,

                motorista.Disponible
            });
        }


        // =========================================================
        // DTO
        // =========================================================

        public class CrearMotoristaRequest
        {
            public int IdUsuario { get; set; }

            public string? NumeroLicencia
            {
                get;
                set;
            }

            public bool Disponible
            {
                get;
                set;
            } = true;
        }


        public class EditarMotoristaRequest
        {
            public string? NumeroLicencia
            {
                get;
                set;
            }

            public bool Disponible
            {
                get;
                set;
            }
        }


        public class CambiarEstadoMotoristaRequest
        {
            public bool Activo
            {
                get;
                set;
            }
        }


        public class CambiarDisponibilidadRequest
        {
            public bool Disponible
            {
                get;
                set;
            }
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