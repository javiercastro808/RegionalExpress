using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RegionalExpress.API.Models;
using System.Security.Claims;

namespace RegionalExpress.API.Controllers
{
    [Route("api/AdminRestaurante")]
    [ApiController]
    [Authorize(Roles = "ADMIN_RESTAURANTE")]
    public class AdminRestauranteController : ControllerBase
    {
        private readonly RegionalExpressContext _context;

        public AdminRestauranteController(
            RegionalExpressContext context)
        {
            _context = context;
        }

        // =========================================================
        // OBTENER ID DEL USUARIO DEL TOKEN
        // =========================================================

        private int ObtenerIdUsuario()
        {
            var valor =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst("idUsuario")?.Value
                ?? User.FindFirst("IdUsuario")?.Value
                ?? User.FindFirst("sub")?.Value;

            if (int.TryParse(valor, out var idUsuario))
            {
                return idUsuario;
            }

            return 0;
        }

        // =========================================================
        // OBTENER RESTAURANTE DEL ADMINISTRADOR
        // =========================================================

        private async Task<UsuarioRestaurante?>
            ObtenerAsignacionRestaurante()
        {
            var idUsuario = ObtenerIdUsuario();

            if (idUsuario <= 0)
            {
                return null;
            }

            return await _context.UsuarioRestaurantes
                .Include(x => x.IdRestauranteNavigation)
                .FirstOrDefaultAsync(x =>
                    x.IdUsuario == idUsuario &&
                    x.Activo);
        }

        // =========================================================
        [HttpGet("historial")]
        public async Task<IActionResult> Historial()
        {
            var asignacion = await ObtenerAsignacionRestaurante();
            if(asignacion == null) return NotFound(new { mensaje = "No tienes un restaurante asignado." });
            var historial = await _context.HistorialEstados.AsNoTracking()
                .Where(h => h.IdServicioNavigation.Pedido != null && h.IdServicioNavigation.Pedido.IdRestaurante == asignacion.IdRestaurante)
                .OrderByDescending(h => h.FechaHora)
                .Select(h => new { h.IdHistorial, h.IdServicio, codigoRastreo=h.IdServicioNavigation.CodigoRastreo,
                    estado=h.IdEstadoNavigation.NombreEstado, usuario=h.IdUsuarioNavigation.Nombre, h.FechaHora, h.Observacion }).ToListAsync();
            return Ok(historial);
        }

        // DASHBOARD
        // GET api/AdminRestaurante/dashboard
        // =========================================================

        [HttpGet("dashboard")]
        public async Task<IActionResult> Dashboard()
        {
            var asignacion =
                await ObtenerAsignacionRestaurante();

            if (asignacion == null)
            {
                return NotFound(new
                {
                    mensaje =
                        "El usuario no tiene un restaurante asignado."
                });
            }

            var idRestaurante =
                asignacion.IdRestaurante;

            var totalCategorias =
                await _context.CategoriasMenus
                    .CountAsync(x =>
                        x.IdRestaurante == idRestaurante &&
                        x.Activo);

            var totalProductos =
                await _context.Productos
                    .CountAsync(x =>
                        x.IdCategoriaNavigation.IdRestaurante
                            == idRestaurante &&
                        x.Activo);

            var productosDisponibles =
                await _context.Productos
                    .CountAsync(x =>
                        x.IdCategoriaNavigation.IdRestaurante
                            == idRestaurante &&
                        x.Activo &&
                        x.Disponible);

            var totalPedidos =
                await _context.Pedidos
                    .CountAsync(x =>
                        x.IdRestaurante == idRestaurante);

            var pedidosHoy =
                await _context.Pedidos
                    .CountAsync(x =>
                        x.IdRestaurante == idRestaurante &&
                        x.FechaPedido.Date == DateTime.Today);

            var ventasHoy =
                await _context.Pedidos
                    .Where(x =>
                        x.IdRestaurante == idRestaurante &&
                        x.FechaPedido.Date == DateTime.Today)
                    .SumAsync(x => (decimal?)x.Total)
                ?? 0;

            return Ok(new
            {
                restaurante = new
                {
                    asignacion.IdRestauranteNavigation.IdRestaurante,
                    asignacion.IdRestauranteNavigation.Nombre,
                    asignacion.IdRestauranteNavigation.Direccion,
                    asignacion.IdRestauranteNavigation.Telefono,
                    asignacion.IdRestauranteNavigation.Correo,
                    asignacion.IdRestauranteNavigation.Imagen,
                    asignacion.IdRestauranteNavigation.Activo
                },

                estadisticas = new
                {
                    totalCategorias,
                    totalProductos,
                    productosDisponibles,
                    totalPedidos,
                    pedidosHoy,
                    ventasHoy
                }
            });
        }

        // =========================================================
        // DATOS DEL RESTAURANTE
        // GET api/AdminRestaurante/restaurante
        // =========================================================

        [HttpGet("restaurante")]
        public async Task<IActionResult> ObtenerRestaurante()
        {
            var asignacion =
                await ObtenerAsignacionRestaurante();

            if (asignacion == null)
            {
                return NotFound(new
                {
                    mensaje =
                        "No tienes un restaurante asignado."
                });
            }

            var restaurante =
                asignacion.IdRestauranteNavigation;

            return Ok(new
            {
                restaurante.IdRestaurante,
                restaurante.Nombre,
                restaurante.Descripcion,
                restaurante.Direccion,
                restaurante.Telefono,
                restaurante.Correo,
                restaurante.Imagen,
                restaurante.HorarioApertura,
                restaurante.HorarioCierre,
                restaurante.Latitud,
                restaurante.Longitud,
                restaurante.Activo,
                restaurante.FechaRegistro
            });
        }

        // =========================================================
        // CATEGORÍAS
        // GET api/AdminRestaurante/categorias
        // =========================================================

        [HttpGet("categorias")]
        public async Task<IActionResult> ObtenerCategorias()
        {
            var asignacion =
                await ObtenerAsignacionRestaurante();

            if (asignacion == null)
            {
                return NotFound(new
                {
                    mensaje =
                        "No tienes un restaurante asignado."
                });
            }

            var categorias =
                await _context.CategoriasMenus
                    .Where(x =>
                        x.IdRestaurante ==
                        asignacion.IdRestaurante)
                    .OrderBy(x => x.Nombre)
                    .Select(x => new
                    {
                        x.IdCategoria,
                        x.IdRestaurante,
                        x.Nombre,
                        x.Descripcion,
                        x.Activo,

                        totalProductos =
                            x.Productos.Count()
                    })
                    .ToListAsync();

            return Ok(categorias);
        }

        // =========================================================
        // CREAR CATEGORÍA
        // POST api/AdminRestaurante/categorias
        // =========================================================

        public class CategoriaRequest
        {
            public string Nombre { get; set; } = "";
            public string? Descripcion { get; set; }
        }

        [HttpPost("categorias")]
        public async Task<IActionResult> CrearCategoria(
            CategoriaRequest request)
        {
            var asignacion =
                await ObtenerAsignacionRestaurante();

            if (asignacion == null)
            {
                return NotFound(new
                {
                    mensaje =
                        "No tienes un restaurante asignado."
                });
            }

            if (string.IsNullOrWhiteSpace(request.Nombre))
            {
                return BadRequest(new
                {
                    mensaje =
                        "El nombre de la categoría es obligatorio."
                });
            }

            var nombre =
                request.Nombre.Trim();

            var existe =
                await _context.CategoriasMenus
                    .AnyAsync(x =>
                        x.IdRestaurante ==
                            asignacion.IdRestaurante &&
                        x.Nombre == nombre);

            if (existe)
            {
                return BadRequest(new
                {
                    mensaje =
                        "Ya existe una categoría con ese nombre."
                });
            }

            var categoria =
                new CategoriasMenu
                {
                    IdRestaurante =
                        asignacion.IdRestaurante,

                    Nombre =
                        nombre,

                    Descripcion =
                        request.Descripcion?.Trim(),

                    Activo =
                        true
                };

            _context.CategoriasMenus.Add(categoria);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensaje =
                    "Categoría creada correctamente.",

                categoria.IdCategoria,
                categoria.Nombre
            });
        }

        // =========================================================
        // EDITAR CATEGORÍA
        // PUT api/AdminRestaurante/categorias/{id}
        // =========================================================

        [HttpPut("categorias/{id}")]
        public async Task<IActionResult> EditarCategoria(
            int id,
            CategoriaRequest request)
        {
            var asignacion =
                await ObtenerAsignacionRestaurante();

            if (asignacion == null)
            {
                return NotFound(new
                {
                    mensaje =
                        "No tienes un restaurante asignado."
                });
            }

            var categoria =
                await _context.CategoriasMenus
                    .FirstOrDefaultAsync(x =>
                        x.IdCategoria == id &&
                        x.IdRestaurante ==
                            asignacion.IdRestaurante);

            if (categoria == null)
            {
                return NotFound(new
                {
                    mensaje =
                        "La categoría no existe o no pertenece a tu restaurante."
                });
            }

            if (string.IsNullOrWhiteSpace(request.Nombre))
            {
                return BadRequest(new
                {
                    mensaje =
                        "El nombre es obligatorio."
                });
            }

            var nombre =
                request.Nombre.Trim();

            var duplicado =
                await _context.CategoriasMenus
                    .AnyAsync(x =>
                        x.IdRestaurante ==
                            asignacion.IdRestaurante &&
                        x.IdCategoria != id &&
                        x.Nombre == nombre);

            if (duplicado)
            {
                return BadRequest(new
                {
                    mensaje =
                        "Ya existe otra categoría con ese nombre."
                });
            }

            categoria.Nombre =
                nombre;

            categoria.Descripcion =
                request.Descripcion?.Trim();

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensaje =
                    "Categoría actualizada correctamente."
            });
        }

        // =========================================================
        // ACTIVAR / DESACTIVAR CATEGORÍA
        // PATCH api/AdminRestaurante/categorias/{id}/estado
        // =========================================================

        public class EstadoRequest
        {
            public bool Activo { get; set; }
        }

        [HttpPatch("categorias/{id}/estado")]
        public async Task<IActionResult> EstadoCategoria(
            int id,
            EstadoRequest request)
        {
            var asignacion =
                await ObtenerAsignacionRestaurante();

            if (asignacion == null)
            {
                return NotFound(new
                {
                    mensaje =
                        "No tienes un restaurante asignado."
                });
            }

            var categoria =
                await _context.CategoriasMenus
                    .FirstOrDefaultAsync(x =>
                        x.IdCategoria == id &&
                        x.IdRestaurante ==
                            asignacion.IdRestaurante);

            if (categoria == null)
            {
                return NotFound(new
                {
                    mensaje =
                        "Categoría no encontrada."
                });
            }

            categoria.Activo =
                request.Activo;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensaje =
                    request.Activo
                        ? "Categoría activada correctamente."
                        : "Categoría desactivada correctamente."
            });
        }

        // =========================================================
        // PRODUCTOS
        // GET api/AdminRestaurante/productos
        // =========================================================

        [HttpGet("productos")]
        public async Task<IActionResult> ObtenerProductos()
        {
            var asignacion =
                await ObtenerAsignacionRestaurante();

            if (asignacion == null)
            {
                return NotFound(new
                {
                    mensaje =
                        "No tienes un restaurante asignado."
                });
            }

            var productos =
                await _context.Productos
                    .Where(x =>
                        x.IdCategoriaNavigation.IdRestaurante ==
                        asignacion.IdRestaurante)
                    .OrderBy(x =>
                        x.IdCategoriaNavigation.Nombre)
                    .ThenBy(x =>
                        x.Nombre)
                    .Select(x => new
                    {
                        x.IdProducto,
                        x.IdCategoria,

                        categoria =
                            x.IdCategoriaNavigation.Nombre,

                        x.Nombre,
                        x.Descripcion,
                        x.Precio,
                        x.Imagen,
                        x.Disponible,
                        x.Activo,
                        x.FechaRegistro
                    })
                    .ToListAsync();

            return Ok(productos);
        }

        // =========================================================
        // CREAR PRODUCTO
        // POST api/AdminRestaurante/productos
        // =========================================================

        public class ProductoRequest
        {
            public int IdCategoria { get; set; }

            public string Nombre { get; set; } = "";

            public string? Descripcion { get; set; }

            public decimal Precio { get; set; }

            public string? Imagen { get; set; }

            public bool Disponible { get; set; } = true;
        }

        [HttpPost("productos")]
        public async Task<IActionResult> CrearProducto(
            ProductoRequest request)
        {
            var asignacion =
                await ObtenerAsignacionRestaurante();

            if (asignacion == null)
            {
                return NotFound(new
                {
                    mensaje =
                        "No tienes un restaurante asignado."
                });
            }

            if (string.IsNullOrWhiteSpace(request.Nombre))
            {
                return BadRequest(new
                {
                    mensaje =
                        "El nombre del producto es obligatorio."
                });
            }

            if (request.Precio <= 0)
            {
                return BadRequest(new
                {
                    mensaje =
                        "El precio debe ser mayor que cero."
                });
            }

            var categoria =
                await _context.CategoriasMenus
                    .FirstOrDefaultAsync(x =>
                        x.IdCategoria ==
                            request.IdCategoria &&
                        x.IdRestaurante ==
                            asignacion.IdRestaurante);

            if (categoria == null)
            {
                return BadRequest(new
                {
                    mensaje =
                        "La categoría seleccionada no pertenece a tu restaurante."
                });
            }

            var nombre =
                request.Nombre.Trim();

            var duplicado =
                await _context.Productos
                    .AnyAsync(x =>
                        x.IdCategoria ==
                            request.IdCategoria &&
                        x.Nombre == nombre);

            if (duplicado)
            {
                return BadRequest(new
                {
                    mensaje =
                        "Ya existe un producto con ese nombre en la categoría."
                });
            }

            var producto =
                new Producto
                {
                    IdCategoria =
                        request.IdCategoria,

                    Nombre =
                        nombre,

                    Descripcion =
                        request.Descripcion?.Trim(),

                    Precio =
                        request.Precio,

                    Imagen =
                        request.Imagen?.Trim(),

                    Disponible =
                        request.Disponible,

                    Activo =
                        true,

                    FechaRegistro =
                        DateTime.Now
                };

            _context.Productos.Add(producto);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensaje =
                    "Producto creado correctamente.",

                producto.IdProducto,
                producto.Nombre
            });
        }

        // =========================================================
        // EDITAR PRODUCTO
        // PUT api/AdminRestaurante/productos/{id}
        // =========================================================

        [HttpPut("productos/{id}")]
        public async Task<IActionResult> EditarProducto(
            int id,
            ProductoRequest request)
        {
            var asignacion =
                await ObtenerAsignacionRestaurante();

            if (asignacion == null)
            {
                return NotFound(new
                {
                    mensaje =
                        "No tienes un restaurante asignado."
                });
            }

            var producto =
                await _context.Productos
                    .Include(x =>
                        x.IdCategoriaNavigation)
                    .FirstOrDefaultAsync(x =>
                        x.IdProducto == id &&
                        x.IdCategoriaNavigation.IdRestaurante ==
                            asignacion.IdRestaurante);

            if (producto == null)
            {
                return NotFound(new
                {
                    mensaje =
                        "Producto no encontrado."
                });
            }

            var categoria =
                await _context.CategoriasMenus
                    .FirstOrDefaultAsync(x =>
                        x.IdCategoria ==
                            request.IdCategoria &&
                        x.IdRestaurante ==
                            asignacion.IdRestaurante);

            if (categoria == null)
            {
                return BadRequest(new
                {
                    mensaje =
                        "La categoría seleccionada no pertenece a tu restaurante."
                });
            }

            if (string.IsNullOrWhiteSpace(request.Nombre))
            {
                return BadRequest(new
                {
                    mensaje =
                        "El nombre es obligatorio."
                });
            }

            if (request.Precio <= 0)
            {
                return BadRequest(new
                {
                    mensaje =
                        "El precio debe ser mayor que cero."
                });
            }

            var nombre =
                request.Nombre.Trim();

            var duplicado =
                await _context.Productos
                    .AnyAsync(x =>
                        x.IdProducto != id &&
                        x.IdCategoria ==
                            request.IdCategoria &&
                        x.Nombre == nombre);

            if (duplicado)
            {
                return BadRequest(new
                {
                    mensaje =
                        "Ya existe otro producto con ese nombre."
                });
            }

            producto.IdCategoria =
                request.IdCategoria;

            producto.Nombre =
                nombre;

            producto.Descripcion =
                request.Descripcion?.Trim();

            producto.Precio =
                request.Precio;

            producto.Imagen =
                request.Imagen?.Trim();

            producto.Disponible =
                request.Disponible;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensaje =
                    "Producto actualizado correctamente."
            });
        }

        // =========================================================
        // ACTIVAR / DESACTIVAR PRODUCTO
        // PATCH api/AdminRestaurante/productos/{id}/estado
        // =========================================================

        [HttpPatch("productos/{id}/estado")]
        public async Task<IActionResult> EstadoProducto(
            int id,
            EstadoRequest request)
        {
            var asignacion =
                await ObtenerAsignacionRestaurante();

            if (asignacion == null)
            {
                return NotFound(new
                {
                    mensaje =
                        "No tienes un restaurante asignado."
                });
            }

            var producto =
                await _context.Productos
                    .Include(x =>
                        x.IdCategoriaNavigation)
                    .FirstOrDefaultAsync(x =>
                        x.IdProducto == id &&
                        x.IdCategoriaNavigation.IdRestaurante ==
                            asignacion.IdRestaurante);

            if (producto == null)
            {
                return NotFound(new
                {
                    mensaje =
                        "Producto no encontrado."
                });
            }

            producto.Activo =
                request.Activo;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensaje =
                    request.Activo
                        ? "Producto activado correctamente."
                        : "Producto desactivado correctamente."
            });
        }

        // =========================================================
        // DISPONIBILIDAD PRODUCTO
        // PATCH api/AdminRestaurante/productos/{id}/disponibilidad
        // =========================================================

        public class DisponibilidadRequest
        {
            public bool Disponible { get; set; }
        }

        [HttpPatch("productos/{id}/disponibilidad")]
        public async Task<IActionResult>
            DisponibilidadProducto(
                int id,
                DisponibilidadRequest request)
        {
            var asignacion =
                await ObtenerAsignacionRestaurante();

            if (asignacion == null)
            {
                return NotFound(new
                {
                    mensaje =
                        "No tienes un restaurante asignado."
                });
            }

            var producto =
                await _context.Productos
                    .Include(x =>
                        x.IdCategoriaNavigation)
                    .FirstOrDefaultAsync(x =>
                        x.IdProducto == id &&
                        x.IdCategoriaNavigation.IdRestaurante ==
                            asignacion.IdRestaurante);

            if (producto == null)
            {
                return NotFound(new
                {
                    mensaje =
                        "Producto no encontrado."
                });
            }

            producto.Disponible =
                request.Disponible;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensaje =
                    request.Disponible
                        ? "Producto disponible."
                        : "Producto marcado como no disponible."
            });
        }

        // =========================================================
        // PEDIDOS
        // GET api/AdminRestaurante/pedidos
        // =========================================================

        [HttpGet("pedidos")]
        public async Task<IActionResult> ObtenerPedidos()
        {
            var asignacion =
                await ObtenerAsignacionRestaurante();

            if (asignacion == null)
            {
                return NotFound(new
                {
                    mensaje =
                        "No tienes un restaurante asignado."
                });
            }

            var pedidos =
                await _context.Pedidos
                    .Where(x =>
                        x.IdRestaurante ==
                            asignacion.IdRestaurante)
                    .OrderByDescending(x =>
                        x.FechaPedido)
                    .Select(x => new
                    {
                        x.IdPedido,
                        x.IdServicio,

                        codigoRastreo =
                            x.IdServicioNavigation.CodigoRastreo,

                        cliente =
                            x.IdServicioNavigation
                                .IdClienteNavigation.Nombre,

                        x.ModalidadEntrega,

                        estado =
                            x.IdServicioNavigation
                                .IdEstadoActualNavigation
                                .NombreEstado,

                        idEstado =
                            x.IdServicioNavigation.IdEstadoActual,

                        x.CostoProductos,
                        x.CostoDelivery,
                        x.Total,
                        x.FechaPedido,

                        x.DireccionEntrega,
                        x.ReferenciaEntrega
                    })
                    .ToListAsync();

            return Ok(pedidos);
        }

        // =========================================================
        // DETALLE PEDIDO
        // GET api/AdminRestaurante/pedidos/{id}
        // =========================================================

        [HttpGet("pedidos/{id}")]
        public async Task<IActionResult> ObtenerPedido(
            int id)
        {
            var asignacion =
                await ObtenerAsignacionRestaurante();

            if (asignacion == null)
            {
                return NotFound(new
                {
                    mensaje =
                        "No tienes un restaurante asignado."
                });
            }

            var pedido =
                await _context.Pedidos
                    .Where(x =>
                        x.IdPedido == id &&
                        x.IdRestaurante ==
                            asignacion.IdRestaurante)
                    .Select(x => new
                    {
                        x.IdPedido,
                        x.IdServicio,

                        codigoRastreo =
                            x.IdServicioNavigation.CodigoRastreo,

                        cliente = new
                        {
                            idUsuario =
                                x.IdServicioNavigation.IdCliente,

                            nombre =
                                x.IdServicioNavigation
                                    .IdClienteNavigation.Nombre,

                            correo =
                                x.IdServicioNavigation
                                    .IdClienteNavigation.Correo
                        },

                        x.ModalidadEntrega,
                        x.DireccionEntrega,
                        x.ReferenciaEntrega,

                        estado = new
                        {
                            idEstado =
                                x.IdServicioNavigation.IdEstadoActual,

                            nombre =
                                x.IdServicioNavigation
                                    .IdEstadoActualNavigation
                                    .NombreEstado
                        },

                        x.CostoProductos,
                        x.CostoDelivery,
                        x.Total,
                        x.Observaciones,
                        x.FechaPedido,

                        productos =
                            x.DetallePedidos.Select(d => new
                            {
                                d.IdDetallePedido,
                                d.IdProducto,

                                producto =
                                    d.IdProductoNavigation.Nombre,

                                d.Cantidad,
                                d.PrecioUnitario,
                                d.Subtotal
                            })
                    })
                    .FirstOrDefaultAsync();

            if (pedido == null)
            {
                return NotFound(new
                {
                    mensaje =
                        "Pedido no encontrado."
                });
            }

            return Ok(pedido);
        }

        // =========================================================
        // ESTADOS DISPONIBLES PARA UN PEDIDO
        // GET api/AdminRestaurante/pedidos/{id}/estados
        // =========================================================

        [HttpGet("pedidos/{id}/estados")]
        public async Task<IActionResult>
            ObtenerEstadosPedido(int id)
        {
            var asignacion =
                await ObtenerAsignacionRestaurante();

            if (asignacion == null)
            {
                return NotFound(new
                {
                    mensaje =
                        "No tienes un restaurante asignado."
                });
            }

            var pedido =
                await _context.Pedidos
                    .FirstOrDefaultAsync(x =>
                        x.IdPedido == id &&
                        x.IdRestaurante ==
                            asignacion.IdRestaurante);

            if (pedido == null)
            {
                return NotFound(new
                {
                    mensaje =
                        "Pedido no encontrado."
                });
            }

            var estados =
                await _context.EstadosServicios
                    .Where(x =>
                        x.TipoServicio == "DELIVERY" &&
                        x.Modalidad ==
                            pedido.ModalidadEntrega &&
                        x.Activo)
                    .OrderBy(x =>
                        x.OrdenEstado)
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
        // CAMBIAR ESTADO DEL PEDIDO
        // PATCH api/AdminRestaurante/pedidos/{id}/estado
        // =========================================================

        public class CambiarEstadoPedidoRequest
        {
            public int IdEstado { get; set; }

            public string? Observacion { get; set; }
        }

        [HttpPatch("pedidos/{id}/estado")]
        public async Task<IActionResult> CambiarEstadoPedido(
            int id,
            CambiarEstadoPedidoRequest request)
        {
            var idUsuario =
                ObtenerIdUsuario();

            if (idUsuario <= 0)
            {
                return Unauthorized(new
                {
                    mensaje =
                        "No fue posible identificar al usuario."
                });
            }

            var asignacion =
                await ObtenerAsignacionRestaurante();

            if (asignacion == null)
            {
                return NotFound(new
                {
                    mensaje =
                        "No tienes un restaurante asignado."
                });
            }

            var pedido =
                await _context.Pedidos
                    .Include(x =>
                        x.IdServicioNavigation)
                    .ThenInclude(x =>
                        x.IdEstadoActualNavigation)
                    .FirstOrDefaultAsync(x =>
                        x.IdPedido == id &&
                        x.IdRestaurante ==
                            asignacion.IdRestaurante);

            if (pedido == null)
            {
                return NotFound(new
                {
                    mensaje =
                        "Pedido no encontrado."
                });
            }

            var nuevoEstado =
                await _context.EstadosServicios
                    .FirstOrDefaultAsync(x =>
                        x.IdEstado ==
                            request.IdEstado &&
                        x.TipoServicio ==
                            "DELIVERY" &&
                        x.Modalidad ==
                            pedido.ModalidadEntrega &&
                        x.Activo);

            if (nuevoEstado == null)
            {
                return BadRequest(new
                {
                    mensaje =
                        "El estado seleccionado no corresponde a este pedido."
                });
            }

            var estadoActual =
                pedido.IdServicioNavigation
                    .IdEstadoActualNavigation;

            if (estadoActual != null)
            {
                if (nuevoEstado.OrdenEstado <
                    estadoActual.OrdenEstado)
                {
                    return BadRequest(new
                    {
                        mensaje =
                            "No se puede regresar el pedido a un estado anterior."
                    });
                }

                if (nuevoEstado.IdEstado ==
                    estadoActual.IdEstado)
                {
                    return BadRequest(new
                    {
                        mensaje =
                            "El pedido ya se encuentra en ese estado."
                    });
                }
            }

            await using var transaction =
                await _context.Database
                    .BeginTransactionAsync();

            try
            {
                pedido.IdServicioNavigation
                    .IdEstadoActual =
                    nuevoEstado.IdEstado;

                var historial =
                    new HistorialEstado
                    {
                        IdServicio =
                            pedido.IdServicio,

                        IdEstado =
                            nuevoEstado.IdEstado,

                        IdUsuario =
                            idUsuario,

                        FechaHora =
                            DateTime.Now,

                        Observacion =
                            string.IsNullOrWhiteSpace(
                                request.Observacion)
                            ? $"Estado actualizado a {nuevoEstado.NombreEstado}"
                            : request.Observacion.Trim()
                    };

                _context.HistorialEstados.Add(
                    historial);

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return Ok(new
                {
                    mensaje =
                        "Estado del pedido actualizado correctamente.",

                    pedido.IdPedido,

                    pedido.IdServicio,

                    estado = new
                    {
                        nuevoEstado.IdEstado,
                        nuevoEstado.NombreEstado,
                        nuevoEstado.OrdenEstado
                    },

                    fechaHora =
                        historial.FechaHora
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                return StatusCode(500, new
                {
                    mensaje =
                        "Ocurrió un error al actualizar el pedido.",

                    error =
                        ex.InnerException?.Message
                        ?? ex.Message
                });
            }
        }
    }
}