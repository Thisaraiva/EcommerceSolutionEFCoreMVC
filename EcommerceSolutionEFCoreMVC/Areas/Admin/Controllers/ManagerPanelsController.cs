using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EcommerceSolutionEFCoreMVC.Areas.Admin.Models.ViewModels; // ViewModels para os relatórios
using EcommerceSolutionEFCoreMVC.Services.Interfaces;
using EcommerceSolutionEFCoreMVC.Models.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations; // Serviços para obter dados do banco

namespace EcommerceSolutionEFCoreMVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ManagerPanelController : Controller
    {
        private readonly IReportService _reportService;

        public ManagerPanelController(IReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var defaultData = _reportService.GetDefaultReports() ?? new ReportDataViewModel();

            // Envia as opções de OrderStatus para a View
            ViewData["OrderStatusList"] = Enum.GetValues(typeof(OrderStatus))
                .Cast<OrderStatus>()
                .Select(status => new SelectListItem
                {
                    Value = ((int)status).ToString(),
                    Text = status.GetType()
                        .GetMember(status.ToString())
                        .First()
                        .GetCustomAttributes(typeof(DisplayAttribute), false)
                        .FirstOrDefault() is DisplayAttribute display
                        ? display.Name
                        : status.ToString()
                })
                .ToList();

            return View(defaultData);
        }

        [HttpPost]
        public IActionResult FilterReports([FromBody] ReportFilterViewModel filters)
        {
            if (filters == null)
            {
                return BadRequest("Invalid filters.");
            }

            // Gera os relatórios com base nos filtros recebidos
            var filteredData = _reportService.GetReportsByFilters(filters);

            return PartialView("_ReportsPartial", filteredData);
        }
    }
}
