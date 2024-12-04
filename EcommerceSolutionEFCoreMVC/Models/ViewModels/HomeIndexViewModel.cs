using EcommerceSolutionEFCoreMVC.Models.Entities;

namespace EcommerceSolutionEFCoreMVC.Models.ViewModels
{
    public class HomeIndexViewModel
    {
        public IEnumerable<Product> AllProducts { get; set; }
        public IEnumerable<Product> PromotionalProducts { get; set; }
    }
}