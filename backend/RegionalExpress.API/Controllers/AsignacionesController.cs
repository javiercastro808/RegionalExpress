using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RegionalExpress.API.Models;
using System.Security.Claims;

namespace RegionalExpress.API.Controllers
{
    [Route("api/Asignaciones")]
    [ApiController]
    [Authorize(Roles = "ADMIN_GENERAL")]
    public class AsignacionesController : ControllerBase
    {
        private readonly RegionalExpressContext _context;

        public AsignacionesController(
            RegionalExpressContext context)
        {
            _context = context;
        }

        private static bool EsEstadoFinal(string nombre) => new[] { "ENTREGADO", "PEDIDO ENTREGADO", "PAQUETE ENTREGADO", "RECOGIDO", "PEDIDO RECOGIDO", "FINALIZADO", "COMPLETADO", "CANCELADO", "CANCELADA" }.Contains(nombre.Trim().ToUpperInvariant());

        private int ObtenerIdUsuario()
        {
            var valor =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst("idUsuario")?.Value
                ?? User.FindFirst("IdUsuario")?.Value
                ?? User.FindFirst("sub")?.Value;

            return int.TryParse(
                valor,
                out var idUsuario)
                ? idUsuario
                : 0;
        }

        // =========================================================
        // MOTORISTAS ACTIVOS
        // =========================================================

        [HttpGet("motoristas")]
        public async Task<IActionResult> Motoristas()
        {
            var motoristas =
                await _context.Motoristas
                    .Where(x => x.Activo)
                    .OrderBy(x =>
                        x.IdUsuarioNavigation.Nombre)
                    .Select(x => new
                    {
                        x.IdMotorista,
                        x.IdUsuario,

                        nombre =
                            x.IdUsuarioNavigation.Nombre,

                        correo =
                            x.IdUsuarioNavigation.Correo,

                        x.NumeroLicencia,
                        x.Disponible,
                        x.Activo
                    })
                    .ToListAsync();

            return Ok(motoristas);
        }

        // =========================================================
        // SERVICIOS PENDIENTES DE ASIGNACIÓN
        // =========================================================

        [HttpGet("pendientes")]
        public async Task<IActionResult> Pendientes()
        {
            var servicios =
                await _context.Servicios
                    .Where(x =>
                        x.IdMotorista == null &&
                        x.Activo)
                    .OrderByDescending(x =>
                        x.FechaCreacion)
                    .Select(x => new
                    {
                        x.IdServicio,
                        x.CodigoRastreo,
                        x.TipoServicio,

                        cliente =
                            x.IdClienteNavigation.Nombre,

                        estado =
                            x.IdEstadoActualNavigation.NombreEstado,

                        x.Total,
                        x.FechaCreacion,

                        modalidad =
                            x.Pedido != null
                                ? x.Pedido.ModalidadEntrega
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
        // DTO EXCLUSIVO DE ASIGNACIONES
        // =========================================================

        public class AsignarMotoristaRequest
        {
            public int IdMotorista { get; set; }

            public string? Observacion { get; set; }
        }

        // =========================================================
        // ASIGNAR MOTORISTA
        // =========================================================

        [HttpPatch("servicios/{idServicio}/motorista")]
        public async Task<IActionResult> Asignar(
            int idServicio,
            AsignarMotoristaRequest request)
        {
            var idUsuarioAdmin =
                ObtenerIdUsuario();

            if (idUsuarioAdmin <= 0)
            {
                return Unauthorized(new
                {
                    mensaje =
                        "No fue posible identificar al administrador."
                });
            }

            await using var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
            var servicio =
                await _context.Servicios
                    .Include(x => x.Pedido)
                    .Include(x => x.IdEstadoActualNavigation)
                    .FirstOrDefaultAsync(x =>
                        x.IdServicio == idServicio);

            if (servicio == null)
            {
                return NotFound(new
                {
                    mensaje =
                        "Servicio no encontrado."
                });
            }

            if (!servicio.Activo || servicio.FechaFinalizacion != null || EsEstadoFinal(servicio.IdEstadoActualNavigation.NombreEstado))
            {
                return BadRequest(new
                {
                    mensaje =
                        "El servicio ya está finalizado."
                });
            }

            if (servicio.TipoServicio == "DELIVERY" && servicio.Pedido?.ModalidadEntrega == "RECOGER")
                return BadRequest(new { mensaje = "Los pedidos para recoger no requieren motorista." });
            if (servicio.IdMotorista != null)
                return Conflict(new { mensaje = "El servicio ya tiene motorista. Quite la asignación antes de reasignar." });

            var motorista =
                await _context.Motoristas
                    .Include(x =>
                        x.IdUsuarioNavigation)
                    .FirstOrDefaultAsync(x =>
                        x.IdMotorista ==
                            request.IdMotorista &&
                        x.Activo);

            if (motorista == null)
            {
                return NotFound(new
                {
                    mensaje =
                        "Motorista no encontrado."
                });
            }

            if (!motorista.Disponible || await _context.Servicios.AnyAsync(x => x.IdMotorista == motorista.IdMotorista && x.Activo && x.FechaFinalizacion == null))
            {
                return BadRequest(new
                {
                    mensaje =
                        "El motorista seleccionado no está disponible."
                });
            }

            var consultaEstado =
                _context.EstadosServicios
                    .Where(x =>
                        x.TipoServicio ==
                            servicio.TipoServicio &&
                        x.Activo &&
                        x.NombreEstado
                            .ToLower()
                            .Contains("motorista"));

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

            var estadoMotorista =
                await consultaEstado
                    .OrderBy(x =>
                        x.OrdenEstado)
                    .FirstOrDefaultAsync();


            try
            {
                servicio.IdMotorista =
                    motorista.IdMotorista;

                motorista.Disponible =
                    false;

                if (estadoMotorista != null && estadoMotorista.OrdenEstado >= servicio.IdEstadoActualNavigation.OrdenEstado)
                {
                    servicio.IdEstadoActual =
                        estadoMotorista.IdEstado;

                    var historial =
                        new HistorialEstado
                        {
                            IdServicio =
                                servicio.IdServicio,

                            IdEstado =
                                estadoMotorista.IdEstado,

                            IdUsuario =
                                idUsuarioAdmin,

                            FechaHora =
                                DateTime.Now,

                            Observacion =
                                string.IsNullOrWhiteSpace(
                                    request.Observacion)
                                    ? $"Motorista asignado: {motorista.IdUsuarioNavigation.Nombre}"
                                    : request.Observacion.Trim()
                        };

                    _context.HistorialEstados.Add(
                        historial);
                }

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return Ok(new
                {
                    mensaje =
                        "Motorista asignado correctamente.",

                    servicio.IdServicio,
                    servicio.CodigoRastreo,

                    motorista = new
                    {
                        motorista.IdMotorista,
                        motorista.IdUsuarioNavigation.Nombre,
                        motorista.NumeroLicencia
                    },

                    estado =
                        servicio.IdEstadoActual == estadoMotorista?.IdEstado ? estadoMotorista.NombreEstado : servicio.IdEstadoActualNavigation.NombreEstado
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                return StatusCode(500, new
                {
                    mensaje =
                        "No fue posible asignar el motorista.",

                    error =
                        ex.InnerException?.Message
                        ?? ex.Message
                });
            }
        }

        // =========================================================
        // QUITAR MOTORISTA
        // =========================================================

        [HttpDelete("servicios/{idServicio}/motorista")]
        public async Task<IActionResult> QuitarMotorista(
            int idServicio)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
            var servicio =
                await _context.Servicios
                    .Include(x => x.Pedido)
                    .Include(x => x.IdEstadoActualNavigation)
                    .FirstOrDefaultAsync(x =>
                        x.IdServicio == idServicio);

            if (servicio == null)
            {
                return NotFound(new
                {
                    mensaje =
                        "Servicio no encontrado."
                });
            }

            if (!servicio.Activo || servicio.FechaFinalizacion != null || EsEstadoFinal(servicio.IdEstadoActualNavigation.NombreEstado))
                return BadRequest(new { mensaje = "No se puede quitar el motorista de un servicio finalizado." });
            if (servicio.TipoServicio == "DELIVERY" && servicio.Pedido?.ModalidadEntrega == "RECOGER")
                return BadRequest(new { mensaje = "Los pedidos para recoger no requieren motorista." });

            if (servicio.IdMotorista == null)
            {
                return BadRequest(new
                {
                    mensaje =
                        "El servicio no tiene motorista asignado."
                });
            }

            var motorista =
                await _context.Motoristas
                    .FirstOrDefaultAsync(x =>
                        x.IdMotorista ==
                            servicio.IdMotorista);

            if (motorista != null)
            {
                motorista.Disponible = !await _context.Servicios.AnyAsync(x =>
                    x.IdMotorista == motorista.IdMotorista && x.IdServicio != idServicio && x.Activo && x.FechaFinalizacion == null);
            }

            servicio.IdMotorista =
                null;

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            return Ok(new
            {
                mensaje =
                    "Motorista retirado del servicio."
            });
        }
    }
}
