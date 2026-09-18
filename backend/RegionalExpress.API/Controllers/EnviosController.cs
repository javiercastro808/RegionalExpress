using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RegionalExpress.API.Models;

namespace RegionalExpress.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EnviosController : ControllerBase
    {
        private readonly RegionalExpressContext _context;
        private readonly RegionalExpress.API.Services.ConfirmacionCorreo _correo;

        private const decimal TARIFA_BASE = 15m;
        private const decimal COSTO_POR_KM = 2m;
        private const decimal COSTO_POR_LIBRA = 1m;


        public EnviosController(
            RegionalExpressContext context,
            RegionalExpress.API.Services.ConfirmacionCorreo correo
        )
        {
            _context = context;
            _correo = correo;
        }


        // ==========================================
        // REQUEST
        // ==========================================

        public class CrearEnvioRequest
        {
            public int IdCliente { get; set; }


            // REMITENTE

            public string NombreRemitente { get; set; } = "";

            public string TelefonoRemitente { get; set; } = "";

            public string? CorreoRemitente { get; set; }


            // DESTINATARIO

            public string NombreDestinatario { get; set; } = "";

            public string TelefonoDestinatario { get; set; } = "";

            public string? CorreoDestinatario { get; set; }


            // ORIGEN

            public string DireccionOrigen { get; set; } = "";

            public string? ReferenciaOrigen { get; set; }


            // DESTINO

            public string DireccionDestino { get; set; } = "";

            public string? ReferenciaDestino { get; set; }


            // PAQUETE

            public decimal Peso { get; set; }

            public decimal DistanciaKm { get; set; }

            public string TipoPaquete { get; set; } = "";

            public string? DescripcionPaquete { get; set; }
        }


        // ==========================================
        // CALCULAR TARIFA
        // GET api/Envios/calcular
        // ==========================================

        [HttpGet("calcular")]
        public IActionResult CalcularTarifa(
            decimal peso,
            decimal distanciaKm
        )
        {
            if (peso <= 0)
            {
                return BadRequest(
                    new
                    {
                        mensaje =
                            "El peso debe ser mayor a 0."
                    }
                );
            }


            if (distanciaKm < 0)
            {
                return BadRequest(
                    new
                    {
                        mensaje =
                            "La distancia no puede ser negativa."
                    }
                );
            }


            decimal costoDistancia =
                distanciaKm *
                COSTO_POR_KM;


            decimal costoPeso =
                peso *
                COSTO_POR_LIBRA;


            decimal total =
                TARIFA_BASE +
                costoDistancia +
                costoPeso;


            return Ok(
                new
                {
                    tarifaBase =
                        TARIFA_BASE,

                    distanciaKm =
                        distanciaKm,

                    costoPorKm =
                        COSTO_POR_KM,

                    costoDistancia =
                        costoDistancia,

                    peso =
                        peso,

                    costoPorLibra =
                        COSTO_POR_LIBRA,

                    costoPeso =
                        costoPeso,

                    total =
                        total
                }
            );
        }


        // ==========================================
        // POST api/Envios
        // ==========================================

        [Authorize(Roles = "CLIENTE")]
        [HttpPost]
        public async Task<IActionResult> CrearEnvio(
            CrearEnvioRequest request
        )
        {
            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var clienteId)) return Unauthorized();
            if (!await _context.Usuarios.AnyAsync(u => u.IdUsuario == clienteId && u.Activo && u.IdRolNavigation.Activo)) return Forbid();
            request.IdCliente = clienteId;
            // ==========================================
            // VALIDACIONES
            // ==========================================

            if (request.IdCliente <= 0)
            {
                return BadRequest(
                    new
                    {
                        mensaje =
                            "Cliente no válido."
                    }
                );
            }


            if (
                string.IsNullOrWhiteSpace(
                    request.NombreRemitente
                )
            )
            {
                return BadRequest(
                    new
                    {
                        mensaje =
                            "Ingrese el nombre del remitente."
                    }
                );
            }


            if (
                string.IsNullOrWhiteSpace(
                    request.TelefonoRemitente
                )
            )
            {
                return BadRequest(
                    new
                    {
                        mensaje =
                            "Ingrese el teléfono del remitente."
                    }
                );
            }


            if (
                string.IsNullOrWhiteSpace(
                    request.NombreDestinatario
                )
            )
            {
                return BadRequest(
                    new
                    {
                        mensaje =
                            "Ingrese el nombre del destinatario."
                    }
                );
            }


            if (
                string.IsNullOrWhiteSpace(
                    request.TelefonoDestinatario
                )
            )
            {
                return BadRequest(
                    new
                    {
                        mensaje =
                            "Ingrese el teléfono del destinatario."
                    }
                );
            }


            if (
                string.IsNullOrWhiteSpace(
                    request.DireccionOrigen
                )
            )
            {
                return BadRequest(
                    new
                    {
                        mensaje =
                            "Ingrese la dirección de origen."
                    }
                );
            }


            if (
                string.IsNullOrWhiteSpace(
                    request.DireccionDestino
                )
            )
            {
                return BadRequest(
                    new
                    {
                        mensaje =
                            "Ingrese la dirección de destino."
                    }
                );
            }


            if (request.Peso <= 0)
            {
                return BadRequest(
                    new
                    {
                        mensaje =
                            "El peso debe ser mayor a 0."
                    }
                );
            }


            if (request.DistanciaKm < 0)
            {
                return BadRequest(
                    new
                    {
                        mensaje =
                            "La distancia no puede ser negativa."
                    }
                );
            }


            // ==========================================
            // VALIDAR CLIENTE
            // ==========================================

            var cliente =
                await _context.Usuarios
                    .FirstOrDefaultAsync(
                        x =>
                            x.IdUsuario ==
                            request.IdCliente
                    );


            if (cliente == null)
            {
                return NotFound(
                    new
                    {
                        mensaje =
                            "El cliente no existe."
                    }
                );
            }


            // ==========================================
            // BUSCAR ESTADO INICIAL DE ENVÍO
            // ==========================================

            var estadoInicial =
                await _context
                    .EstadosServicios
                    .Where(
                        x =>
                            x.TipoServicio ==
                            "ENVIO"
                    )
                    .OrderBy(
                        x =>
                            x.OrdenEstado
                    )
                    .FirstOrDefaultAsync();


            if (estadoInicial == null)
            {
                return BadRequest(
                    new
                    {
                        mensaje =
                            "No existe un estado inicial configurado para ENVIO."
                    }
                );
            }


            // ==========================================
            // CALCULAR COSTOS
            // ==========================================

            decimal costoDistancia =
                request.DistanciaKm *
                COSTO_POR_KM;


            decimal costoPeso =
                request.Peso *
                COSTO_POR_LIBRA;


            decimal total =
                TARIFA_BASE +
                costoDistancia +
                costoPeso;


            // ==========================================
            // TRANSACCIÓN
            // ==========================================

            await using var transaction =
                await _context.Database
                    .BeginTransactionAsync();


            try
            {
                // ==========================================
                // GENERAR CÓDIGO ÚNICO
                // ==========================================

                string codigoRastreo;

                bool existe;


                do
                {
                    codigoRastreo =
                        GenerarCodigoRastreo();


                    existe =
                        await _context.Servicios
                            .AnyAsync(
                                x =>
                                    x.CodigoRastreo ==
                                    codigoRastreo
                            );

                }
                while (existe);


                // ==========================================
                // SERVICIO
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
                            "ENVIO",

                        IdEstadoActual =
                            estadoInicial.IdEstado,

                        Total =
                            total,

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


                await _context
                    .SaveChangesAsync();


                // ==========================================
                // ENVÍO
                // ==========================================

                var envio =
                    new Envio
                    {
                        IdServicio =
                            servicio.IdServicio,


                        NombreRemitente =
                            request.NombreRemitente
                                .Trim(),


                        TelefonoRemitente =
                            request.TelefonoRemitente
                                .Trim(),


                        CorreoRemitente =
                            string.IsNullOrWhiteSpace(
                                request.CorreoRemitente
                            )
                                ? null
                                : request.CorreoRemitente
                                    .Trim(),


                        NombreDestinatario =
                            request.NombreDestinatario
                                .Trim(),


                        TelefonoDestinatario =
                            request.TelefonoDestinatario
                                .Trim(),


                        CorreoDestinatario =
                            string.IsNullOrWhiteSpace(
                                request.CorreoDestinatario
                            )
                                ? null
                                : request.CorreoDestinatario
                                    .Trim(),


                        DireccionOrigen =
                            request.DireccionOrigen
                                .Trim(),


                        ReferenciaOrigen =
                            string.IsNullOrWhiteSpace(
                                request.ReferenciaOrigen
                            )
                                ? null
                                : request.ReferenciaOrigen
                                    .Trim(),


                        LatitudOrigen =
                            null,


                        LongitudOrigen =
                            null,


                        DireccionDestino =
                            request.DireccionDestino
                                .Trim(),


                        ReferenciaDestino =
                            string.IsNullOrWhiteSpace(
                                request.ReferenciaDestino
                            )
                                ? null
                                : request.ReferenciaDestino
                                    .Trim(),


                        LatitudDestino =
                            null,


                        LongitudDestino =
                            null,


                        Peso =
                            request.Peso,


                        TipoPaquete =
                            request.TipoPaquete
                                .Trim(),


                        DescripcionPaquete =
                            string.IsNullOrWhiteSpace(
                                request.DescripcionPaquete
                            )
                                ? null
                                : request.DescripcionPaquete
                                    .Trim(),


                        TarifaBase =
                            TARIFA_BASE,


                        CostoDistancia =
                            costoDistancia,


                        CostoPeso =
                            costoPeso,


                        Total =
                            total,


                        FechaSolicitud =
                            DateTime.Now
                    };


                _context.Envios.Add(
                    envio
                );


                await _context
                    .SaveChangesAsync();


                // ==========================================
                // CONFIRMAR TRANSACCIÓN
                // ==========================================

                await transaction
                    .CommitAsync();
                var correoConfirmacion = await _correo.Enviar(servicio);


                // ==========================================
                // RESPUESTA
                // ==========================================

                return Ok(
                    new
                    {
                        correoConfirmacion,
                        mensaje =
                            "Envío registrado correctamente.",

                        idServicio =
                            servicio.IdServicio,

                        idEnvio =
                            envio.IdEnvio,

                        codigoRastreo =
                            servicio.CodigoRastreo,

                        nombreRemitente =
                            envio.NombreRemitente,

                        nombreDestinatario =
                            envio.NombreDestinatario,

                        direccionOrigen =
                            envio.DireccionOrigen,

                        direccionDestino =
                            envio.DireccionDestino,

                        peso =
                            envio.Peso,

                        tarifaBase =
                            envio.TarifaBase,

                        costoDistancia =
                            envio.CostoDistancia,

                        costoPeso =
                            envio.CostoPeso,

                        total =
                            envio.Total,

                        fechaSolicitud =
                            envio.FechaSolicitud
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
                            "Ocurrió un error al registrar el envío."
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
                $"ENV-GT-{numero}";
        }
    }
}