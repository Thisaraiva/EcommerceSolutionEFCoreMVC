using EcommerceSolutionEFCoreMVC.Areas.Admin.Models.ViewModels;
using EcommerceSolutionEFCoreMVC.Data;
using EcommerceSolutionEFCoreMVC.Models.Enums;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion; // Contexto do banco



namespace EcommerceSolutionEFCoreMVC.Services.Interfaces
{
    public class ReportService : IReportService
    {
        private readonly EcommerceDbContext _context;

        public ReportService(EcommerceDbContext context)
        {
            _context = context;
        }

        public ReportDataViewModel GetDefaultReports()
        {
            return new ReportDataViewModel
            {
                TotalOrders = _context.Orders.Count(),
                TotalSales = _context.Orders.Sum(o => o.TotalAmount),

                SalesByCategory = _context.OrderItems
                    .GroupBy(oi => oi.Product.Category.Name)
                    .Select(g => new KeyValuePair<string, decimal>(g.Key, g.Sum(oi => oi.Subtotal)))
                    .ToList(),

                SalesByProduct = _context.OrderItems // Adicione SalesByProduct aqui
                    .GroupBy(oi => oi.Product.Name)
                    .Select(g => new KeyValuePair<string, int>(g.Key, g.Sum(oi => oi.Quantity)))
                    .ToList(),

                SalesByState = _context.Orders // Adicione SalesByState aqui
                    .GroupBy(o => o.Address.State)
                    .Select(g => new KeyValuePair<string, decimal>(g.Key, g.Sum(o => o.TotalAmount)))
                    .ToList(),

                SalesTotalByProduct = _context.OrderItems
                    .GroupBy(oi => oi.Product.Name)
                    .Select(g => new KeyValuePair<string, decimal>(g.Key, g.Sum(oi => oi.Subtotal)))
                    .ToList(),
                
                OrdersByStatus = _context.Orders
                    .GroupBy(o => o.Status)
                    .Select(g => new KeyValuePair<string, int>(g.Key.ToString(), g.Count()))
                    .ToList(),

                TotalValueByStatus = _context.Orders
                    .GroupBy(o => o.Status)
                    .Select(g => new KeyValuePair<string, decimal>(g.Key.ToString(), g.Sum(o => o.TotalAmount)))
                    .ToList()
            };
        }

        public ReportDataViewModel GetReportsByFilters(ReportFilterViewModel filters)
        {
            // Lógica de filtro com base nos parâmetros
            var query = _context.Orders.AsQueryable();

            // Aplicar datas padrão (últimos 30 dias) se StartDate ou EndDate forem nulos
            var defaultStartDate = DateTime.UtcNow.AddDays(-30);
            var defaultEndDate = DateTime.UtcNow;

            var startDate = filters.StartDate ?? defaultStartDate;
            var endDate = filters.EndDate ?? defaultEndDate;

            if (!string.IsNullOrEmpty(filters.Status) && Enum.TryParse<OrderStatus>(filters.Status, out var statusEnum))            
                query = query.Where(o => o.Status == statusEnum);

            if (filters.StartDate != null)
                query = query.Where(o => o.OrderDate >= filters.StartDate);

            if (filters.EndDate != null)
                query = query.Where(o => o.OrderDate <= filters.EndDate);

            return new ReportDataViewModel
            {
                TotalOrders = query.Count(),
                TotalSales = query.Sum(o => o.TotalAmount),

                SalesByCategory = query.SelectMany(oi => oi.OrderItems)
                    .GroupBy(oi => oi.Product.Category.Name)
                    .Select(g => new KeyValuePair<string, decimal>(g.Key, g.Sum(oi => oi.Subtotal)))
                    .ToList(),

                SalesByProduct = query.SelectMany(o => o.OrderItems)
                    .GroupBy(oi => oi.Product.Name)
                    .Select(g => new KeyValuePair<string, int>(g.Key, g.Sum(oi => oi.Quantity)))
                    .ToList(),

                SalesByState = query.Where(o => o.Address != null)
                    .GroupBy(o => o.Address.State)
                    .Select(g => new KeyValuePair<string, decimal>(g.Key ?? "Unknown", g.Sum(o => o.TotalAmount)))
                    .ToList(),

                SalesTotalByProduct = query.SelectMany(o => o.OrderItems)
                    .GroupBy(oi => oi.Product.Name)
                    .Select(g => new KeyValuePair<string, decimal>(g.Key, g.Sum(oi => oi.Subtotal)))
                    .ToList(),

                OrdersByStatus = query.Where(o => o.Status != null)
                    .GroupBy(o => o.Status)
                    .Select(g => new KeyValuePair<string, int>(g.Key.ToString(), g.Count()))
                    .ToList(),

                TotalValueByStatus = query.Where(o => o.Status != null)
                    .GroupBy(o => o.Status)
                    .Select(g => new KeyValuePair<string, decimal>(g.Key.ToString(), g.Sum(o => o.TotalAmount)))
                    .ToList()
            };
        }
        
        public ReportDataViewModel GetSalesByProduct()
        {
            return new ReportDataViewModel
            {
                SalesByProduct = _context.OrderItems
                    .GroupBy(oi => oi.Product.Name)
                    .Select(g => new KeyValuePair<string, int>(g.Key, g.Sum(oi => oi.Quantity)))
                    .ToList()
            };
        }

        public ReportDataViewModel GetSalesByCategory()
        {
            return new ReportDataViewModel
            {
                SalesByCategory = _context.OrderItems
                    .GroupBy(oi => oi.Product.Category.Name)
                    .Select(g => new KeyValuePair<string, decimal>(g.Key, g.Sum(oi => oi.Subtotal)))
                    .ToList()
            };
        }

        public ReportDataViewModel GetSalesByState()
        {
            return new ReportDataViewModel
            {
                SalesByState = _context.Orders
                    .GroupBy(o => o.Address.State)
                    .Select(g => new KeyValuePair<string, decimal>(g.Key, g.Sum(o => o.TotalAmount)))
                    .ToList()
            };
        }

        public ReportDataViewModel GetSalesTotalByProduct()
        {
            var query = _context.Orders.AsQueryable();

            return new ReportDataViewModel
            {
                SalesTotalByProduct = query.SelectMany(o => o.OrderItems)
                .GroupBy(oi => oi.Product.Name)
                .Select(g => new KeyValuePair<string, decimal>(g.Key, g.Sum(oi => oi.Subtotal)))
                .ToList()
            };
        }

        public ReportDataViewModel GetOrdersByStatus()
        {
            return new ReportDataViewModel
            {
                OrdersByStatus = _context.Orders
                    .GroupBy(o => o.Status)
                    .Select(g => new KeyValuePair<string, int>(g.Key.ToString(), g.Count()))
                    .ToList()
            };
        }

        public ReportDataViewModel GetTotalValueByStatus()
        {
            return new ReportDataViewModel
            {
                TotalValueByStatus = _context.Orders
                    .GroupBy(o => o.Status)
                    .Select(g => new KeyValuePair<string, decimal>(g.Key.ToString(), g.Sum(o => o.TotalAmount)))
                    .ToList()
            };
        }
    }
}
