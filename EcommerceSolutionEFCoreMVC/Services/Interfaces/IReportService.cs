using EcommerceSolutionEFCoreMVC.Areas.Admin.Models.ViewModels;

namespace EcommerceSolutionEFCoreMVC.Services.Interfaces
{
    public interface IReportService
    {
        ReportDataViewModel GetDefaultReports();
        ReportDataViewModel GetReportsByFilters(ReportFilterViewModel filters);
        /*ReportDataViewModel GetSalesByProduct();
        ReportDataViewModel GetSalesByCategory();
        ReportDataViewModel GetSalesByState();
        public ReportDataViewModel GetSalesTotalByProduct();
        ReportDataViewModel GetOrdersByStatus();
        ReportDataViewModel GetTotalValueByStatus();*/
    }

}