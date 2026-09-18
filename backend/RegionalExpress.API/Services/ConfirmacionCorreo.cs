using System.Net;
using System.Net.Mail;
using RegionalExpress.API.Models;
using Microsoft.EntityFrameworkCore;

namespace RegionalExpress.API.Services;

public class ConfirmacionCorreo(IConfiguration config, IWebHostEnvironment entorno, ILogger<ConfirmacionCorreo> logger, RegionalExpressContext context)
{
    public async Task<string> Enviar(Servicio servicio)
    {
        // Un fallo de correo nunca revierte un servicio que ya fue confirmado.
        try
        {
            var modo = config["Correo:Modo"] ?? "Desactivado";
            if (modo == "Desactivado") return "No configurado";
            var destinatario = await context.Usuarios.Where(u => u.IdUsuario == servicio.IdCliente).Select(u => u.Correo).FirstOrDefaultAsync();
            if (string.IsNullOrWhiteSpace(destinatario)) return "Sin correo del cliente";
            using var mensaje = new MailMessage(config["Correo:Remitente"] ?? "pruebas@regionalexpress.local", destinatario);
            mensaje.Subject = $"Regional Express — {servicio.CodigoRastreo}";
            mensaje.Body = $"Tu servicio fue registrado correctamente.\nCódigo de rastreo: {servicio.CodigoRastreo}\nTotal: Q {servicio.Total:0.00}\nConsulta el avance en la sección Rastreo de Regional Express.";
            using var smtp = new SmtpClient();
            if (modo == "Local")
            {
                var carpeta = Path.Combine(entorno.ContentRootPath, "App_Data", "correos");
                Directory.CreateDirectory(carpeta);
                smtp.DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory;
                smtp.PickupDirectoryLocation = carpeta;
            }
            else if (modo == "Smtp")
            {
                smtp.Host = config["Correo:Host"] ?? throw new InvalidOperationException("Falta Correo:Host");
                smtp.Port = config.GetValue("Correo:Puerto", 587);
                smtp.EnableSsl = true;
                smtp.Credentials = new NetworkCredential(config["Correo:Usuario"], config["Correo:Password"]);
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            }
            else return "Configuración inválida";
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            await smtp.SendMailAsync(mensaje, timeout.Token);
            return modo == "Local" ? "Generado en modo local" : "Enviado";
        }
        catch (Exception ex)
        {
            logger.LogWarning("No se pudo enviar la confirmación del servicio {Id}: {Tipo}", servicio.IdServicio, ex.GetType().Name);
            return "No enviado; el servicio sí fue registrado";
        }
    }
}
