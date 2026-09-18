using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RegionalExpress.API.Models;
using System.Security.Claims;

namespace RegionalExpress.API.Controllers
{
    [Route("api/Motorista")]
    [ApiController]
    [Authorize(Roles = "MOTORISTA")]
    public class MotoristaController : ControllerBase
    {
        private readonly RegionalExpressContext _context;

        public MotoristaController(RegionalExpressContext context)
        {
            _context = context;
        }

        // =========================================================
        // USUARIO DEL TOKEN
        // =========================================================

        private int ObtenerIdUsuario()
        {
            var valor =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst("idUsuario")?.Value
                ?? User.FindFirst("IdUsuario")?.Value
                ?? User.FindFirst("sub")?.Value;

            return int.TryParse(valor, out var idUsuario)
                ? idUsuario
                : 0;
        }

        // =========================================================
        // MOTORISTA DEL USUARIO
        // =========================================================

        private async Task<Motorista?> ObtenerMotorista()
        {
            var idUsuario = ObtenerIdUsuario();

            if (idUsuario <= 0)
            {
                return null;
            }

            return await _context.Motoristas
                .Include(x => x.IdUsuarioNavigation)
                .FirstOrDefaultAsync(x =>
                    x.IdUsuario == idUsuario &&
                    x.Activo);
        }

        // =========================================================
        // DASHBOARD
        // =========================================================

        [HttpGet("dashboard")]
        public async Task<IActionResult> Dashboard()
        {
            var motorista = await ObtenerMotorista();

            if (motorista == null)
            {
                return NotFound(new
                {
                    mensaje = "No existe un perfil de motorista activo para este usuario."
                });
            }

            var servicios = await _context.Servicios
                .Where(x => x.IdMotorista == motorista.IdMotorista)
                .ToListAsync();

            var totalServicios = servicios.Count;

            var activos = servicios.Count(x =>
                x.Activo &&
                x.FechaFinalizacion == null);

            var finalizados = servicios.Count(x =>
                x.FechaFinalizacion != null);

            var delivery = servicios.Count(x =>
                x.TipoServicio == "DELIVERY");

            var envios = servicios.Count(x =>
                x.TipoServicio == "ENVIO");

            return Ok(new
            {
                motorista = new
                {
                    motorista.IdMotorista,
                    motorista.IdUsuario,
                    motorista.IdUsuarioNavigation.Nombre,
                    motorista.IdUsuarioNavigation.Correo,
                    motorista.NumeroLicencia,
                    motorista.Disponible,
                    motorista.Activo
                },

                estadisticas = new
                {
                    totalServicios,
                    activos,
                    finalizados,
                    delivery,
                    envios
                }
            });
        }

        // =========================================================
        // SERVICIOS ASIGNADOS
        // =========================================================

        [HttpGet("servicios")]
        public async Task<IActionResult> Servicios()
        {
            var motorista = await ObtenerMotorista();

            if (motorista == null)
            {
                return NotFound(new
                {
                    mensaje = "No existe un perfil de motorista activo."
                });
            }

            var servicios = await _context.Servicios
                .Where(x =>
                    x.IdMotorista == motorista.IdMotorista)
                .OrderByDescending(x => x.FechaCreacion)
                .Select(x => new
                {
                    x.IdServicio,
                    x.CodigoRastreo,
                    x.TipoServicio,
                    x.IdEstadoActual,

                    estado =
                        x.IdEstadoActualNavigation.NombreEstado,

                    ordenEstado =
                        x.IdEstadoActualNavigation.OrdenEstado,

                    cliente =
                        x.IdClienteNavigation.Nombre,

                    x.Total,
                    x.FechaCreacion,
                    x.FechaFinalizacion,
                    x.Activo,

                    modalidad =
                        x.Pedido != null
                            ? x.Pedido.ModalidadEntrega
                            : null,

                    origen =
                        x.Envio != null
                            ? x.Envio.DireccionOrigen
                            : x.Pedido != null
                                ? "Restaurante"
                                : null,

                    destino =
                        x.Envio != null
                            ? x.Envio.DireccionDestino
                            : x.Pedido != null
                                ? x.Pedido.DireccionEntrega
                                : null
                })
                .ToListAsync();

            return Ok(servicios);
        }

        // =========================================================
        // DETALLE DEL SERVICIO
        // =========================================================

        [HttpGet("servicios/{id}")]
        public async Task<IActionResult> Servicio(int id)
        {
            var motorista = await ObtenerMotorista();

            if (motorista == null)
            {
                return NotFound(new
                {
                    mensaje = "No existe un perfil de motorista activo."
                });
            }

            var servicio = await _context.Servicios
                .Include(x => x.IdClienteNavigation)
                .Include(x => x.IdEstadoActualNavigation)
                .Include(x => x.Pedido)
                    .ThenInclude(x => x!.IdRestauranteNavigation)
                .Include(x => x.Envio)
                .FirstOrDefaultAsync(x =>
                    x.IdServicio == id &&
                    x.IdMotorista == motorista.IdMotorista);

            if (servicio == null)
            {
                return NotFound(new
                {
                    mensaje = "El servicio no existe o no está asignado a este motorista."
                });
            }

            object? detalle;

            if (
                servicio.TipoServicio == "DELIVERY" &&
                servicio.Pedido != null)
            {
                detalle = new
                {
                    tipo = "DELIVERY",

                    restaurante =
                        servicio.Pedido.IdRestauranteNavigation.Nombre,

                    direccionRestaurante =
                        servicio.Pedido.IdRestauranteNavigation.Direccion,

                    modalidad =
                        servicio.Pedido.ModalidadEntrega,

                    direccionEntrega =
                        servicio.Pedido.DireccionEntrega,

                    referenciaEntrega =
                        servicio.Pedido.ReferenciaEntrega,

                    latitud =
                        servicio.Pedido.LatitudEntrega,

                    longitud =
                        servicio.Pedido.LongitudEntrega,

                    observaciones =
                        servicio.Pedido.Observaciones
                };
            }
            else if (
                servicio.TipoServicio == "ENVIO" &&
                servicio.Envio != null)
            {
                detalle = new
                {
                    tipo = "ENVIO",

                    remitente = new
                    {
                        nombre =
                            servicio.Envio.NombreRemitente,

                        telefono =
                            servicio.Envio.TelefonoRemitente,

                        correo =
                            servicio.Envio.CorreoRemitente
                    },

                    destinatario = new
                    {
                        nombre =
                            servicio.Envio.NombreDestinatario,

                        telefono =
                            servicio.Envio.TelefonoDestinatario,

                        correo =
                            servicio.Envio.CorreoDestinatario
                    },

                    origen = new
                    {
                        direccion =
                            servicio.Envio.DireccionOrigen,

                        referencia =
                            servicio.Envio.ReferenciaOrigen,

                        latitud =
                            servicio.Envio.LatitudOrigen,

                        longitud =
                            servicio.Envio.LongitudOrigen
                    },

                    destino = new
                    {
                        direccion =
                            servicio.Envio.DireccionDestino,

                        referencia =
                            servicio.Envio.ReferenciaDestino,

                        latitud =
                            servicio.Envio.LatitudDestino,

                        longitud =
                            servicio.Envio.LongitudDestino
                    },

                    servicio.Envio.Peso,
                    servicio.Envio.TipoPaquete,
                    servicio.Envio.DescripcionPaquete
                };
            }
            else
            {
                detalle = null;
            }

            return Ok(new
            {
                servicio.IdServicio,
                servicio.CodigoRastreo,
                servicio.TipoServicio,

                cliente = new
                {
                    servicio.IdClienteNavigation.IdUsuario,
                    servicio.IdClienteNavigation.Nombre,
                    servicio.IdClienteNavigation.Correo
                },

                estado = new
                {
                    servicio.IdEstadoActualNavigation.IdEstado,
                    servicio.IdEstadoActualNavigation.NombreEstado,
                    servicio.IdEstadoActualNavigation.OrdenEstado
                },

                servicio.Total,
                servicio.FechaCreacion,
                servicio.FechaFinalizacion,
                servicio.Activo,

                detalle
            });
        }

        // =========================================================
        // ESTADOS DISPONIBLES
        // =========================================================

        [HttpGet("servicios/{id}/estados")]
        public async Task<IActionResult> Estados(int id)
        {
            var motorista = await ObtenerMotorista();

            if (motorista == null)
            {
                return NotFound(new
                {
                    mensaje = "No existe un perfil de motorista activo."
                });
            }

            var servicio = await _context.Servicios
                .Include(x => x.Pedido)
                .FirstOrDefaultAsync(x =>
                    x.IdServicio == id &&
                    x.IdMotorista == motorista.IdMotorista);

            if (servicio == null)
            {
                return NotFound(new
                {
                    mensaje = "Servicio no encontrado."
                });
            }

            var consulta = _context.EstadosServicios
                .Where(x =>
                    x.TipoServicio == servicio.TipoServicio &&
                    x.Activo);

            if (
                servicio.TipoServicio == "DELIVERY" &&
                servicio.Pedido != null)
            {
                var modalidad =
                    servicio.Pedido.ModalidadEntrega;

                consulta = consulta.Where(x =>
                    x.Modalidad == modalidad);
            }
            else
            {
                consulta = consulta.Where(x =>
                    x.Modalidad == null ||
                    x.Modalidad == "");
            }

            var estados = await consulta
                .OrderBy(x => x.OrdenEstado)
                .Select(x => new
                {
                    x.IdEstado,
                    x.NombreEstado,
                    x.OrdenEstado,
                    x.Modalidad
                })
                .ToListAsync();

            return Ok(estados);
        }

        // =========================================================
        // DTO EXCLUSIVO MOTORISTA
        // =========================================================

        public class CambiarEstadoMotoristaRequest
        {
            public int IdEstado { get; set; }

            public string? Observacion { get; set; }
        }

        // =========================================================
        // CAMBIAR ESTADO
        // =========================================================

        [HttpPatch("servicios/{id}/estado")]
        public async Task<IActionResult> CambiarEstado(
            int id,
            CambiarEstadoMotoristaRequest request)
        {
            var idUsuario =
                ObtenerIdUsuario();

            var motorista =
                await ObtenerMotorista();

            if (
                motorista == null ||
                idUsuario <= 0)
            {
                return Unauthorized(new
                {
                    mensaje = "No fue posible identificar al motorista."
                });
            }

            var servicio = await _context.Servicios
                .Include(x => x.Pedido)
                .Include(x => x.IdEstadoActualNavigation)
                .FirstOrDefaultAsync(x =>
                    x.IdServicio == id &&
                    x.IdMotorista == motorista.IdMotorista);

            if (servicio == null)
            {
                return NotFound(new
                {
                    mensaje = "Servicio no encontrado o no asignado a este motorista."
                });
            }

            var consultaEstado =
                _context.EstadosServicios
                    .Where(x =>
                        x.IdEstado == request.IdEstado &&
                        x.TipoServicio == servicio.TipoServicio &&
                        x.Activo);

            if (
                servicio.TipoServicio == "DELIVERY" &&
                servicio.Pedido != null)
            {
                var modalidad =
                    servicio.Pedido.ModalidadEntrega;

                consultaEstado =
                    consultaEstado.Where(x =>
                        x.Modalidad == modalidad);
            }
            else
            {
                consultaEstado =
                    consultaEstado.Where(x =>
                        x.Modalidad == null ||
                        x.Modalidad == "");
            }

            var nuevoEstado =
                await consultaEstado.FirstOrDefaultAsync();

            if (nuevoEstado == null)
            {
                return BadRequest(new
                {
                    mensaje = "El estado seleccionado no corresponde al servicio."
                });
            }

            var estadoActual =
                servicio.IdEstadoActualNavigation;

            if (
                nuevoEstado.IdEstado ==
                estadoActual.IdEstado)
            {
                return BadRequest(new
                {
                    mensaje = "El servicio ya se encuentra en ese estado."
                });
            }

            if (
                nuevoEstado.OrdenEstado <
                estadoActual.OrdenEstado)
            {
                return BadRequest(new
                {
                    mensaje = "No se puede regresar a un estado anterior."
                });
            }

            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                servicio.IdEstadoActual =
                    nuevoEstado.IdEstado;

                var estadosFlujo =
                    _context.EstadosServicios
                        .Where(x =>
                            x.TipoServicio ==
                                servicio.TipoServicio &&
                            x.Activo);

                if (
                    servicio.TipoServicio == "DELIVERY" &&
                    servicio.Pedido != null)
                {
                    var modalidad =
                        servicio.Pedido.ModalidadEntrega;

                    estadosFlujo =
                        estadosFlujo.Where(x =>
                            x.Modalidad == modalidad);
                }
                else
                {
                    estadosFlujo =
                        estadosFlujo.Where(x =>
                            x.Modalidad == null ||
                            x.Modalidad == "");
                }

                var ultimoOrden =
                    await estadosFlujo
                        .MaxAsync(x => x.OrdenEstado);

                if (
                    nuevoEstado.OrdenEstado ==
                    ultimoOrden)
                {
                    servicio.FechaFinalizacion =
                        DateTime.Now;

                    servicio.Activo =
                        false;

                    motorista.Disponible =
                        true;
                }

                var historial =
                    new HistorialEstado
                    {
                        IdServicio =
                            servicio.IdServicio,

                        IdEstado =
                            nuevoEstado.IdEstado,

                        IdUsuario =
                            idUsuario,

                        FechaHora =
                            DateTime.Now,

                        Observacion =
                            string.IsNullOrWhiteSpace(
                                request.Observacion)
                                ? $"Estado actualizado por motorista a {nuevoEstado.NombreEstado}"
                                : request.Observacion.Trim()
                    };

                _context.HistorialEstados.Add(
                    historial);

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return Ok(new
                {
                    mensaje =
                        "Estado actualizado correctamente.",

                    servicio.IdServicio,
                    servicio.CodigoRastreo,

                    estado = new
                    {
                        nuevoEstado.IdEstado,
                        nuevoEstado.NombreEstado,
                        nuevoEstado.OrdenEstado
                    },

                    servicio.FechaFinalizacion
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                return StatusCode(500, new
                {
                    mensaje =
                        "No fue posible actualizar el servicio.",

                    error =
                        ex.InnerException?.Message
                        ?? ex.Message
                });
            }
        }

        // =========================================================
        // HISTORIAL DEL MOTORISTA
        // =========================================================

        [HttpGet("historial")]
        public async Task<IActionResult> Historial()
        {
            var motorista =
                await ObtenerMotorista();

            if (motorista == null)
            {
                return NotFound(new
                {
                    mensaje = "No existe un perfil de motorista activo."
                });
            }

            var historial =
                await _context.HistorialEstados
                    .Where(x =>
                        x.IdServicioNavigation.IdMotorista ==
                        motorista.IdMotorista)
                    .OrderByDescending(x =>
                        x.FechaHora)
                    .Select(x => new
                    {
                        x.IdHistorial,
                        x.IdServicio,

                        codigoRastreo =
                            x.IdServicioNavigation.CodigoRastreo,

                        tipoServicio =
                            x.IdServicioNavigation.TipoServicio,

                        estado =
                            x.IdEstadoNavigation.NombreEstado,

                        usuario =
                            x.IdUsuarioNavigation.Nombre,

                        x.FechaHora,
                        x.Observacion
                    })
                    .ToListAsync();

            return Ok(historial);
        }
    }
}