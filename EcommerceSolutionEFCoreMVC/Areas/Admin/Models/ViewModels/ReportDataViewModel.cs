namespace EcommerceSolutionEFCoreMVC.Areas.Admin.Models.ViewModels
{
    public class ReportDataViewModel
    {
        public int TotalOrders { get; set; }
        public decimal TotalSales { get; set; }
        public decimal TotalValue { get; set; }
        public List<KeyValuePair<string, decimal>> SalesByCategory { get; set; } = new List<KeyValuePair<string, decimal>>();
        public List<KeyValuePair<string, int>> SalesByProduct { get; set; } = new List<KeyValuePair<string, int>>();
        public List<KeyValuePair<string, decimal>> SalesByState { get; set; } = new List<KeyValuePair<string, decimal>>();
        public List<KeyValuePair<string, decimal>> SalesTotalByProduct { get; set; } = new List<KeyValuePair<string, decimal>>();
        public List<KeyValuePair<string, int>> OrdersByStatus { get; set; } = new List<KeyValuePair<string, int>>();
        public List<KeyValuePair<string, decimal>> TotalValueByStatus { get; set; } = new List<KeyValuePair<string, decimal>>();
    }

}