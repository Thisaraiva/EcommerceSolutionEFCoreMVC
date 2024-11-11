using EcommerceSolutionEFCoreMVC.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceSolutionEFCoreMVC.Models.Entities
{
    public class Order
    {
        public int OrderId { get; set; }
        public string ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }
        [Display(Name = "Date")]
        public DateTime OrderDate { get; set; }
        public OrderStatus Status { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        [Display(Name = "Total")]
        public decimal TotalAmount { get; set; }
        public List<OrderItem>? OrderItems { get; set; }

        public void CalculateTotalAmount()
        {
            TotalAmount = OrderItems?.Sum(item => item.Subtotal) ?? 0;
        }
    }

}
