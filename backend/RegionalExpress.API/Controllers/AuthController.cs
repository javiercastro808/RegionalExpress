using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RegionalExpress.API.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using RegionalExpress.API.Services;

namespace RegionalExpress.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly RegionalExpressContext _context;

        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;


        public AuthController(
            RegionalExpressContext context,
            IConfiguration configuration, IWebHostEnvironment environment
        )
        {
            _environment = environment;
            _context =
                context;

            _configuration =
                configuration;
        }


        // ====================================================
        // MODELO LOGIN
        // ====================================================

        public class LoginRequest
        {
            public string Correo { get; set; } = "";

            public string Password { get; set; } = "";
        }


        // ====================================================
        // POST api/Auth/login
        // ====================================================

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login(
            LoginRequest request
        )
        {
            // ================================================
            // VALIDAR CORREO
            // ================================================

            if (
                string.IsNullOrWhiteSpace(
                    request.Correo
                )
            )
            {
                return BadRequest(
                    new
                    {
                        mensaje =
                            "Ingrese el correo electrónico."
                    }
                );
            }


            // ================================================
            // VALIDAR PASSWORD
            // ================================================

            if (
                string.IsNullOrWhiteSpace(
                    request.Password
                )
            )
            {
                return BadRequest(
                    new
                    {
                        mensaje =
                            "Ingrese la contraseña."
                    }
                );
            }


            string correo =
                request.Correo
                    .Trim()
                    .ToLower();


            // ================================================
            // BUSCAR USUARIO + ROL
            // ================================================

            var usuario =
                await _context
                    .Usuarios

                    .Include(
                        x =>
                            x.IdRolNavigation
                    )

                    .FirstOrDefaultAsync(
                        x =>
                            x.Correo
                                .ToLower()
                            ==
                            correo
                    );


            // ================================================
            // USUARIO NO EXISTE
            // ================================================

            if (
                usuario == null
            )
            {
                return Unauthorized(
                    new
                    {
                        mensaje =
                            "Correo o contraseña incorrectos."
                    }
                );
            }


            // ================================================
            // USUARIO INACTIVO
            // ================================================

            if (
                !usuario.Activo
            )
            {
                return Unauthorized(
                    new
                    {
                        mensaje =
                            "El usuario se encuentra inactivo."
                    }
                );
            }


            // ================================================
            // ROL INACTIVO
            // ================================================

            if (
                usuario.IdRolNavigation == null
                ||
                !usuario.IdRolNavigation.Activo
            )
            {
                return Unauthorized(
                    new
                    {
                        mensaje =
                            "El rol del usuario no se encuentra activo."
                    }
                );
            }


            // ================================================
            // VALIDAR CONTRASEÑA
            //
            // Las cuentas locales antiguas migran al iniciar sesión en desarrollo.
            // ================================================

            if (!Claves.Verificar(usuario.PasswordHash, request.Password, _environment.IsDevelopment(), out var actualizarClave))
            {
                return Unauthorized(
                    new
                    {
                        mensaje =
                            "Correo o contraseña incorrectos."
                    }
                );
            }


            // ================================================
            // GENERAR TOKEN
            // ================================================

            if (actualizarClave)
            {
                usuario.PasswordHash = Claves.Crear(request.Password);
                await _context.SaveChangesAsync();
            }

            string token =
                GenerarToken(
                    usuario
                );


            // ================================================
            // RESPUESTA
            // ================================================

            return Ok(
                new
                {
                    mensaje =
                        "Inicio de sesión correcto.",

                    token =
                        token,

                    usuario =
                        new
                        {
                            idUsuario =
                                usuario.IdUsuario,

                            nombre =
                                usuario.Nombre,

                            correo =
                                usuario.Correo,

                            idRol =
                                usuario.IdRol,

                            rol =
                                usuario
                                    .IdRolNavigation
                                    .NombreRol
                        }
                }
            );
        }


        // ====================================================
        // GET api/Auth/perfil
        //
        // ESTE ENDPOINT NOS SIRVE PARA PROBAR QUE JWT FUNCIONA
        // ====================================================

        [Authorize]
        [HttpGet("perfil")]
        public IActionResult Perfil()
        {
            string? idUsuario =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier
                );


            string? nombre =
                User.FindFirstValue(
                    ClaimTypes.Name
                );


            string? correo =
                User.FindFirstValue(
                    ClaimTypes.Email
                );


            string? rol =
                User.FindFirstValue(
                    ClaimTypes.Role
                );


            return Ok(
                new
                {
                    mensaje =
                        "Token válido.",

                    usuario =
                        new
                        {
                            idUsuario,
                            nombre,
                            correo,
                            rol
                        }
                }
            );
        }


        // ====================================================
        // GENERAR TOKEN JWT
        // ====================================================

        private string GenerarToken(
            Usuario usuario
        )
        {
            // ================================================
            // CONFIGURACIÓN
            // ================================================

            string jwtKey =
                _configuration[
                    "Jwt:Key"
                ]
                ?? throw new
                    InvalidOperationException(
                        "No se encontró Jwt:Key."
                    );


            string issuer =
                _configuration[
                    "Jwt:Issuer"
                ]
                ?? throw new
                    InvalidOperationException(
                        "No se encontró Jwt:Issuer."
                    );


            string audience =
                _configuration[
                    "Jwt:Audience"
                ]
                ?? throw new
                    InvalidOperationException(
                        "No se encontró Jwt:Audience."
                    );


            int expirationMinutes =
                int.TryParse(
                    _configuration[
                        "Jwt:ExpirationMinutes"
                    ],

                    out int minutos
                )
                    ? minutos
                    : 120;


            // ================================================
            // CLAIMS
            // ================================================

            var claims =
                new List<Claim>
                {
                    new Claim(
                        ClaimTypes.NameIdentifier,

                        usuario
                            .IdUsuario
                            .ToString()
                    ),


                    new Claim(
                        ClaimTypes.Name,

                        usuario.Nombre
                    ),


                    new Claim(
                        ClaimTypes.Email,

                        usuario.Correo
                    ),


                    new Claim(
                        ClaimTypes.Role,

                        usuario
                            .IdRolNavigation
                            .NombreRol
                    )
                };


            // ================================================
            // CLAVE
            // ================================================

            var key =
                new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(
                        jwtKey
                    )
                );


            // ================================================
            // FIRMA
            // ================================================

            var credentials =
                new SigningCredentials(
                    key,

                    SecurityAlgorithms
                        .HmacSha256
                );


            // ================================================
            // CREAR TOKEN
            // ================================================

            var jwtToken =
                new JwtSecurityToken(
                    issuer:
                        issuer,

                    audience:
                        audience,

                    claims:
                        claims,

                    notBefore:
                        DateTime.UtcNow,

                    expires:
                        DateTime.UtcNow
                            .AddMinutes(
                                expirationMinutes
                            ),

                    signingCredentials:
                        credentials
                );


            // ================================================
            // CONVERTIR A STRING
            // ================================================

            return new
                JwtSecurityTokenHandler()
                    .WriteToken(
                        jwtToken
                    );
        }
    }
}