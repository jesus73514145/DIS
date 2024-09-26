using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.UI.Services;
using MimeKit;
using MailKit.Net.Smtp;
using MimeKit.Text;
using MailKit.Security;
namespace proyecto.Models
{
    public class EmailSender : IMyEmailSender
    {
        private readonly string _smtpServer;
        private readonly int _smtpPort;
        private readonly string _smtpUsername;
        private readonly string _smtpPassword;

        public EmailSender(string smtpServer, int smtpPort, string smtpUsername, string smtpPassword)
        {
            _smtpServer = smtpServer;
            _smtpPort = smtpPort;
            _smtpUsername = smtpUsername;
            _smtpPassword = smtpPassword;
        }


        public async Task SendEmailAsync(string recipient, string subject, string body)
        {
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress("Empresa Textil", "yisusoria@gmail.com"));
            emailMessage.To.Add(new MailboxAddress("", recipient));
            emailMessage.Subject = subject;

            // Obtener la ruta completa de la imagen
            var imagePath = "wwwroot/images/ALZEN_logo.png";

            // Crear el cuerpo del correo electrónico con texto y un archivo adjunto
            var textPart = new TextPart(TextFormat.Plain) { Text = body };

            // Abrir un stream de la imagen
            using var imageStream = File.OpenRead(imagePath);

            var attachment = new MimePart("image", "jpeg")
            {
                Content = new MimeContent(imageStream),
                ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                ContentTransferEncoding = ContentEncoding.Base64,
                FileName = Path.GetFileName(imagePath)
            };

            var multipart = new Multipart("mixed");
            multipart.Add(textPart);
            multipart.Add(attachment);
            emailMessage.Body = multipart;

            using var client = new SmtpClient();
            await client.ConnectAsync("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
            await client.AuthenticateAsync("yisusoria@gmail.com", "wefnwxwiwcwotxxu");
            await client.SendAsync(emailMessage);

            await client.DisconnectAsync(true);
        }


    }
}