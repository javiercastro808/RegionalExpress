using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RegionalExpress.API.Models;

namespace RegionalExpress.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RastreoController : ControllerBase
    {
        private readonly RegionalExpressContext _context;

        public RastreoController(
            RegionalExpressContext context
        )
        {
            _context = context;
        }


        // =====================================================
        // GET: api/Rastreo/DEL-GT-XXXXX
        // GET: api/Rastreo/ENV-GT-XXXXX
        // =====================================================

        [HttpGet("{codigo}")]
        public async Task<IActionResult> Rastrear(
            string codigo
        )
        {
            // =================================================
            // VALIDAR CÓDIGO
            // =================================================

            if (
                string.IsNullOrWhiteSpace(
                    codigo
                )
            )
            {
                return BadRequest(
                    new
                    {
                        mensaje =
                            "Debe ingresar un código de rastreo."
                    }
                );
            }


            codigo =
                codigo
                    .Trim()
                    .ToUpper();


            // =================================================
            // BUSCAR SERVICIO
            // =================================================

            var servicio =
                await _context.Servicios

                    .Include(
                        x =>
                            x.IdEstadoActualNavigation
                    )

                    .Include(
                        x =>
                            x.Pedido
                    )

                    .ThenInclude(
                        x =>
                            x!.IdRestauranteNavigation
                    )

                    .Include(
                        x =>
                            x.Envio
                    )

                    .FirstOrDefaultAsync(
                        x =>
                            x.CodigoRastreo ==
                            codigo
                    );


            // =================================================
            // NO ENCONTRADO
            // =================================================

            if (servicio == null)
            {
                return NotFound(
                    new
                    {
                        mensaje =
                            "No se encontró ningún servicio con ese código."
                    }
                );
            }


            // =================================================
            // DELIVERY
            // =================================================

            var datosPrivados = await RegionalExpress.API.Services.AccesoServicio.PuedeVerDatos(User, servicio, _context);

            if (
                servicio.TipoServicio
                    .Trim()
                    .ToUpper() ==
                "DELIVERY"
            )
            {
                if (
                    servicio.Pedido == null
                )
                {
                    return NotFound(
                        new
                        {
                            mensaje =
                                "No se encontró la información del pedido."
                        }
                    );
                }


                var pedido =
                    servicio.Pedido;


                // =============================================
                // ESTADOS DELIVERY
                // =============================================

                var estados =
                    await _context
                        .EstadosServicios

                        .Where(
                            x =>
                                x.TipoServicio ==
                                    "DELIVERY"
                                &&
                                x.Modalidad ==
                                    pedido.ModalidadEntrega
                                &&
                                x.Activo
                        )

                        .OrderBy(
                            x =>
                                x.OrdenEstado
                        )

                        .Select(
                            x =>
                                new
                                {
                                    idEstado =
                                        x.IdEstado,

                                    nombre =
                                        x.NombreEstado,

                                    orden =
                                        x.OrdenEstado
                                }
                        )

                        .ToListAsync();


                // =============================================
                // RESPUESTA DELIVERY
                // =============================================

                return Ok(
                    new
                    {
                        datosPrivados,
                        codigoRastreo =
                            servicio.CodigoRastreo,

                        tipoServicio =
                            servicio.TipoServicio,

                        estado =
                            servicio
                                .IdEstadoActualNavigation
                                .NombreEstado,

                        ordenEstado =
                            servicio
                                .IdEstadoActualNavigation
                                .OrdenEstado,

                        activo =
                            servicio.Activo,

                        fechaCreacion =
                            servicio.FechaCreacion,

                        fechaFinalizacion =
                            servicio.FechaFinalizacion,

                        total =
                            servicio.Total,


                        // =====================================
                        // DATOS DELIVERY
                        // =====================================

                        modalidad =
                            pedido.ModalidadEntrega,

                        restaurante =
                            pedido
                                .IdRestauranteNavigation
                                ?.Nombre,

                        direccionEntrega =
                            datosPrivados ? pedido.DireccionEntrega : null,

                        referenciaEntrega =
                            datosPrivados ? pedido.ReferenciaEntrega : null,

                        costoProductos =
                            pedido.CostoProductos,

                        costoDelivery =
                            pedido.CostoDelivery,


                        // =====================================
                        // DATOS ENVIO VACÍOS
                        // =====================================

                        nombreRemitente =
                            (string?)null,

                        telefonoRemitente =
                            (string?)null,

                        correoRemitente =
                            (string?)null,

                        nombreDestinatario =
                            (string?)null,

                        telefonoDestinatario =
                            (string?)null,

                        correoDestinatario =
                            (string?)null,

                        direccionOrigen =
                            (string?)null,

                        referenciaOrigen =
                            (string?)null,

                        direccionDestino =
                            (string?)null,

                        referenciaDestino =
                            (string?)null,

                        peso =
                            (decimal?)null,

                        tipoPaquete =
                            (string?)null,

                        descripcionPaquete =
                            (string?)null,

                        tarifaBase =
                            (decimal?)null,

                        costoDistancia =
                            (decimal?)null,

                        costoPeso =
                            (decimal?)null,


                        estados =
                            estados
                    }
                );
            }


            // =================================================
            // ENVIO
            // =================================================

            if (
                servicio.TipoServicio
                    .Trim()
                    .ToUpper() ==
                "ENVIO"
            )
            {
                // =============================================
                // VALIDAR DATOS DEL ENVÍO
                // =============================================

                if (
                    servicio.Envio == null
                )
                {
                    return NotFound(
                        new
                        {
                            mensaje =
                                "El servicio existe pero no se encontró la información del envío."
                        }
                    );
                }


                var envio =
                    servicio.Envio;


                // =============================================
                // ESTADOS ENVIO
                // =============================================

                var estados =
                    await _context
                        .EstadosServicios

                        .Where(
                            x =>
                                x.TipoServicio ==
                                    "ENVIO"
                                &&
                                x.Activo
                        )

                        .OrderBy(
                            x =>
                                x.OrdenEstado
                        )

                        .Select(
                            x =>
                                new
                                {
                                    idEstado =
                                        x.IdEstado,

                                    nombre =
                                        x.NombreEstado,

                                    orden =
                                        x.OrdenEstado
                                }
                        )

                        .ToListAsync();


                // =============================================
                // RESPUESTA ENVIO
                // =============================================

                return Ok(
                    new
                    {
                        datosPrivados,
                        codigoRastreo =
                            servicio.CodigoRastreo,

                        tipoServicio =
                            servicio.TipoServicio,

                        estado =
                            servicio
                                .IdEstadoActualNavigation
                                .NombreEstado,

                        ordenEstado =
                            servicio
                                .IdEstadoActualNavigation
                                .OrdenEstado,

                        activo =
                            servicio.Activo,

                        fechaCreacion =
                            servicio.FechaCreacion,

                        fechaFinalizacion =
                            servicio.FechaFinalizacion,

                        total =
                            servicio.Total,


                        // =====================================
                        // DELIVERY VACÍO
                        // =====================================

                        modalidad =
                            (string?)null,

                        restaurante =
                            (string?)null,

                        direccionEntrega =
                            (string?)null,

                        referenciaEntrega =
                            (string?)null,

                        costoProductos =
                            (decimal?)null,

                        costoDelivery =
                            (decimal?)null,


                        // =====================================
                        // REMITENTE
                        // =====================================

                        nombreRemitente =
                            datosPrivados ? envio.NombreRemitente : null,

                        telefonoRemitente =
                            datosPrivados ? envio.TelefonoRemitente : null,

                        correoRemitente =
                            datosPrivados ? envio.CorreoRemitente : null,


                        // =====================================
                        // DESTINATARIO
                        // =====================================

                        nombreDestinatario =
                            datosPrivados ? envio.NombreDestinatario : null,

                        telefonoDestinatario =
                            datosPrivados ? envio.TelefonoDestinatario : null,

                        correoDestinatario =
                            datosPrivados ? envio.CorreoDestinatario : null,


                        // =====================================
                        // ORIGEN
                        // =====================================

                        direccionOrigen =
                            datosPrivados ? envio.DireccionOrigen : null,

                        referenciaOrigen =
                            datosPrivados ? envio.ReferenciaOrigen : null,


                        // =====================================
                        // DESTINO
                        // =====================================

                        direccionDestino =
                            datosPrivados ? envio.DireccionDestino : null,

                        referenciaDestino =
                            datosPrivados ? envio.ReferenciaDestino : null,


                        // =====================================
                        // PAQUETE
                        // =====================================

                        peso =
                            envio.Peso,

                        tipoPaquete =
                            envio.TipoPaquete,

                        descripcionPaquete =
                            datosPrivados ? envio.DescripcionPaquete : null,


                        // =====================================
                        // COSTOS
                        // =====================================

                        tarifaBase =
                            envio.TarifaBase,

                        costoDistancia =
                            envio.CostoDistancia,

                        costoPeso =
                            envio.CostoPeso,


                        // =====================================
                        // ESTADOS
                        // =====================================

                        estados =
                            estados
                    }
                );
            }


            // =================================================
            // OTRO TIPO
            // =================================================

            return BadRequest(
                new
                {
                    mensaje =
                        "El tipo de servicio no es compatible con el sistema de rastreo."
                }
            );
        }
    }
}