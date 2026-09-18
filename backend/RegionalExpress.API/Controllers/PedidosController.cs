using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RegionalExpress.API.Models;

namespace RegionalExpress.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PedidosController : ControllerBase
    {
        private readonly RegionalExpressContext _context;
        private readonly RegionalExpress.API.Services.ConfirmacionCorreo _correo;

        public PedidosController(
            RegionalExpressContext context,
            RegionalExpress.API.Services.ConfirmacionCorreo correo
        )
        {
            _context = context;
            _correo = correo;
        }


        // ==========================================
        // MODELO QUE RECIBIREMOS DESDE ANGULAR
        // ==========================================

        public class CrearPedidoRequest
        {
            public int IdCliente { get; set; }

            public int IdEstadoActual { get; set; }

            public int IdRestaurante { get; set; }

            public string ModalidadEntrega { get; set; } = "";

            public string? DireccionEntrega { get; set; }

            public string? ReferenciaEntrega { get; set; }

            public decimal CostoProductos { get; set; }

            public decimal CostoDelivery { get; set; }

            public decimal Total { get; set; }

            public string? Observaciones { get; set; }

            public List<DetallePedidoRequest> Productos { get; set; }
                = new();
        }


        public class DetallePedidoRequest
        {
            public int IdProducto { get; set; }

            public int Cantidad { get; set; }

            public decimal PrecioUnitario { get; set; }
        }


        // ==========================================
        // POST api/Pedidos
        // ==========================================

        [Authorize(Roles = "CLIENTE")]
        [HttpPost]
        public async Task<IActionResult> CrearPedido(
            CrearPedidoRequest request
        )
        {
            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var clienteId)) return Unauthorized();
            if (!await _context.Usuarios.AnyAsync(u => u.IdUsuario == clienteId && u.Activo && u.IdRolNavigation.Activo)) return Forbid();
            request.IdCliente = clienteId;
            if (
                request.Productos == null ||
                request.Productos.Count == 0
            )
            {
                return BadRequest(
                    new
                    {
                        mensaje =
                            "Debe agregar al menos un producto."
                    }
                );
            }


            if (request.IdRestaurante <= 0)
            {
                return BadRequest(
                    new
                    {
                        mensaje =
                            "El restaurante no es válido."
                    }
                );
            }


            if (request.IdCliente <= 0)
            {
                return BadRequest(
                    new
                    {
                        mensaje =
                            "El cliente no es válido."
                    }
                );
            }


            


            // ==========================================
            // VALIDAR RESTAURANTE
            // ==========================================

            var restaurante =
                await _context.Restaurantes
                    .FirstOrDefaultAsync(
                        x =>
                            x.IdRestaurante ==
                            request.IdRestaurante
                    );


            if (restaurante == null || !restaurante.Activo)
            {
                return NotFound(
                    new
                    {
                        mensaje =
                            "El restaurante no existe."
                    }
                );
            }


            // ==========================================
            // TRANSACCIÓN
            // ==========================================

            request.ModalidadEntrega = (request.ModalidadEntrega ?? "").Trim().ToUpperInvariant();
            if (request.ModalidadEntrega != "DOMICILIO" && request.ModalidadEntrega != "RECOGER")
                return BadRequest(new { mensaje = "Seleccione DOMICILIO o RECOGER." });
            if (request.ModalidadEntrega == "DOMICILIO" && string.IsNullOrWhiteSpace(request.DireccionEntrega))
                return BadRequest(new { mensaje = "Ingrese la dirección de entrega." });
            if (request.Productos.Count > 100 || request.Productos.Any(p => p.Cantidad < 1 || p.Cantidad > 100)
                || request.Productos.Select(p => p.IdProducto).Distinct().Count() != request.Productos.Count)
                return BadRequest(new { mensaje = "Revise los productos y sus cantidades (1 a 100)." });
            var ids = request.Productos.Select(p => p.IdProducto).ToList();
            var catalogo = await _context.Productos.AsNoTracking().Where(p => ids.Contains(p.IdProducto)
                && p.Activo && p.Disponible && p.IdCategoriaNavigation.Activo
                && p.IdCategoriaNavigation.IdRestaurante == request.IdRestaurante).ToDictionaryAsync(p => p.IdProducto);
            if (catalogo.Count != ids.Count)
                return BadRequest(new { mensaje = "Un producto ya no está disponible o no pertenece a este restaurante. Actualiza el menú." });
            var estado = await _context.EstadosServicios.Where(e => e.Activo && e.TipoServicio == "DELIVERY"
                && e.Modalidad == request.ModalidadEntrega).OrderBy(e => e.OrdenEstado).FirstOrDefaultAsync();
            if (estado == null) return BadRequest(new { mensaje = "No existe un estado inicial para esta modalidad." });
            request.IdEstadoActual = estado.IdEstado;
            foreach (var item in request.Productos) item.PrecioUnitario = catalogo[item.IdProducto].Precio;
            request.CostoProductos = request.Productos.Sum(p => p.PrecioUnitario * p.Cantidad);
            request.CostoDelivery = request.ModalidadEntrega == "DOMICILIO" ? 10m : 0m;
            request.Total = request.CostoProductos + request.CostoDelivery;

            await using var transaction =
                await _context.Database
                    .BeginTransactionAsync();


            try
            {
                // ==========================================
                // GENERAR CÓDIGO ÚNICO
                // ==========================================

                string codigoRastreo;

                bool existeCodigo;


                do
                {
                    codigoRastreo =
                        GenerarCodigoRastreo();


                    existeCodigo =
                        await _context.Servicios
                            .AnyAsync(
                                x =>
                                    x.CodigoRastreo ==
                                    codigoRastreo
                            );

                }
                while (existeCodigo);


                // ==========================================
                // CREAR SERVICIO
                // ==========================================

                var servicio =
                    new Servicio
                    {
                        CodigoRastreo =
                            codigoRastreo,

                        IdCliente =
                            request.IdCliente,

                        IdMotorista =
                            null,

                        TipoServicio =
                            "DELIVERY",

                        IdEstadoActual =
                            request.IdEstadoActual,

                        Total =
                            request.Total,

                        FechaCreacion =
                            DateTime.Now,

                        FechaFinalizacion =
                            null,

                        Activo =
                            true
                    };


                _context.Servicios.Add(
                    servicio
                );


                await _context.SaveChangesAsync();


                // ==========================================
                // CREAR PEDIDO
                // ==========================================

                var pedido =
                    new Pedido
                    {
                        IdServicio =
                            servicio.IdServicio,

                        IdRestaurante =
                            request.IdRestaurante,

                        ModalidadEntrega =
                            request.ModalidadEntrega,

                        DireccionEntrega =
                            request.ModalidadEntrega ==
                            "DOMICILIO"
                                ? request.DireccionEntrega
                                : null,

                        ReferenciaEntrega =
                            request.ModalidadEntrega ==
                            "DOMICILIO"
                                ? request.ReferenciaEntrega
                                : null,

                        LatitudEntrega =
                            null,

                        LongitudEntrega =
                            null,

                        CostoProductos =
                            request.CostoProductos,

                        CostoDelivery =
                            request.ModalidadEntrega ==
                            "DOMICILIO"
                                ? request.CostoDelivery
                                : 0,

                        Total =
                            request.Total,

                        Observaciones =
                            request.Observaciones,

                        FechaPedido =
                            DateTime.Now
                    };


                _context.Pedidos.Add(
                    pedido
                );


                await _context.SaveChangesAsync();


                // ==========================================
                // CREAR DETALLE DEL PEDIDO
                // ==========================================

                foreach (
                    var producto
                    in request.Productos
                )
                {
                    if (
                        producto.Cantidad <= 0
                    )
                    {
                        continue;
                    }


                    var subtotal =
                        producto.PrecioUnitario *
                        producto.Cantidad;


                    var detalle =
                        new DetallePedido
                        {
                            IdPedido =
                                pedido.IdPedido,

                            IdProducto =
                                producto.IdProducto,

                            Cantidad =
                                producto.Cantidad,

                            PrecioUnitario =
                                producto.PrecioUnitario,

                            Subtotal =
                                subtotal
                        };


                    _context.DetallePedidos.Add(
                        detalle
                    );
                }


                await _context.SaveChangesAsync();


                // ==========================================
                // CONFIRMAR TRANSACCIÓN
                // ==========================================

                await transaction.CommitAsync();
                var correoConfirmacion = await _correo.Enviar(servicio);


                // ==========================================
                // RESPUESTA A ANGULAR
                // ==========================================

                return Ok(
                    new
                    {
                        correoConfirmacion,
                        mensaje =
                            "Pedido registrado correctamente.",

                        idServicio =
                            servicio.IdServicio,

                        idPedido =
                            pedido.IdPedido,

                        codigoRastreo =
                            servicio.CodigoRastreo,

                        modalidadEntrega =
                            pedido.ModalidadEntrega,

                        costoProductos =
                            pedido.CostoProductos,

                        costoDelivery =
                            pedido.CostoDelivery,

                        total =
                            pedido.Total,

                        fechaPedido =
                            pedido.FechaPedido
                    }
                );

            }
            catch (Exception)
            {
                await transaction
                    .RollbackAsync();


                return StatusCode(
                    500,
                    new
                    {
                        mensaje =
                            "Ocurrió un error al registrar el pedido."
                    }
                );
            }
        }


        // ==========================================
        // GENERAR CÓDIGO
        // ==========================================

        private static string GenerarCodigoRastreo()
        {
            var numero = Convert.ToHexString(System.Security.Cryptography.RandomNumberGenerator.GetBytes(10));


            return
                $"DEL-GT-{numero}";
        }
    }
}