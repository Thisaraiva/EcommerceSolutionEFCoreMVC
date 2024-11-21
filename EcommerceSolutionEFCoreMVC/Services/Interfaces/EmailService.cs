using EcommerceSolutionEFCoreMVC.Models.Configurations;
using Microsoft.Extensions.Options;
using System.Net.Mail;
using System.Net;
using EcommerceSolutionEFCoreMVC.Models.Entities;

namespace EcommerceSolutionEFCoreMVC.Services.Interfaces
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        //private readonly IEmailService _emailService;

        public EmailService(IOptions<EmailSettings> emailSettings/*, IEmailService emailService*/)
        {
            _emailSettings = emailSettings.Value;
            //_emailService = emailService;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            using (var smtpClient = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.Port))
            {
                smtpClient.Credentials = new NetworkCredential(_emailSettings.UserName, _emailSettings.Password);
                smtpClient.EnableSsl = true;
                smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtpClient.UseDefaultCredentials = false;

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_emailSettings.SenderEmail, _emailSettings.SenderName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true,
                };
                mailMessage.To.Add(to);

                try
                {
                    await smtpClient.SendMailAsync(mailMessage);
                }
                catch (SmtpException ex)
                {
                    Console.WriteLine($"StatusCode: {ex.StatusCode}");
                    Console.WriteLine($"Erro completo: {ex}");
                    throw new Exception($"Erro ao enviar e-mail: {ex.Message}", ex);
                }
            }
        }


        public async Task SendOrderConfirmationEmail(string to, Order order)
        {
            var subject = $"Confirmação do Pedido #{order.OrderId}";
            var body = $@"
                        <h1>Confirmação do Pedido</h1>
                        <p>Olá, {order.ApplicationUser.FullName},</p>
                        <p>Seu pedido foi confirmado com sucesso.</p>
                        <p><strong>Total:</strong> {order.TotalAmount:C}</p>
                        <p><strong>Status:</strong> {order.Status}</p>
                        <h2>Itens:</h2>
                        <ul>
                        {string.Join("", order.OrderItems.Select(item => $"<li>{item.Product.Name} - {item.Quantity} x {item.UnitPrice:C}</li>"))}
                        </ul>
                        <p>Obrigado por comprar conosco!</p>";
            await SendEmailAsync(to, subject, body);
        }


        public async Task SendOrderStatusUpdateEmail(string to, Order order)
        {
            var subject = $"Atualização do Status do Pedido #{order.OrderId}";
            var body = $@"
                       <h1>Atualização de Pedido</h1>
                       <p>Olá, {order.ApplicationUser.FullName},</p>
                       <p>O status do seu pedido foi atualizado para: <strong>{order.Status}</strong>.</p>
                       <p><strong>Pedido:</strong> #{order.OrderId}</p>
                       <p>Obrigado por comprar conosco!</p>";
            await SendEmailAsync(to, subject, body);
        }

    }

}
