using EcommerceSolutionEFCoreMVC.Data;
using EcommerceSolutionEFCoreMVC.Models.Entities;
using EcommerceSolutionEFCoreMVC.Models.ErrorViewModel;
using EcommerceSolutionEFCoreMVC.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace EcommerceSolutionEFCoreMVC.Controllers
{
    
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly EcommerceDbContext _context;

        public HomeController(ILogger<HomeController> logger, EcommerceDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Carregar todos os produtos
            var products = await _context.Products
                                 .Include(p => p.Category) // Carrega as categorias relacionadas
                                 .ToListAsync();

            // Filtrar produtos em promoção
            var promotionalProducts = products
                                      .Where(p => p.Category != null && p.Category.Name == "Promotion")
                                      .ToList();

            // ViewModel com os dois conjuntos de produtos
            var viewModel = new HomeIndexViewModel
            {
                AllProducts = products,
                PromotionalProducts = promotionalProducts
            };

            return View(viewModel);
        }


        [Authorize]
        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
