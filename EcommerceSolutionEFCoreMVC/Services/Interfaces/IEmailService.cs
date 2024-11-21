using EcommerceSolutionEFCoreMVC.Models.Entities;

namespace EcommerceSolutionEFCoreMVC.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body);
        Task SendOrderConfirmationEmail(string to, Order order);
        Task SendOrderStatusUpdateEmail(string to, Order order);
    }
}
