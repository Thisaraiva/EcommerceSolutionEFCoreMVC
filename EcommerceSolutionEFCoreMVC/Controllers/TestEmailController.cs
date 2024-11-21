using EcommerceSolutionEFCoreMVC.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceSolutionEFCoreMVC.Controllers
{
    //[Authorize(Roles = "Admin")]
    public class TestEmailController : Controller
    {
        private readonly IEmailService _emailService;

        public TestEmailController(IEmailService emailService)
        {
            _emailService = emailService;
        }

        public async Task<IActionResult> SendTestEmail()
        {
            try
            {
                await _emailService.SendEmailAsync("thiagofreitassaraiva@yahoo.com.br", "Teste de Email", "Este é um email de teste.");
                return Content("Email enviado com sucesso!");
            }
            catch (Exception ex)
            {
                return Content($"Erro ao enviar email: {ex.Message}");
            }
        }
    }
}
