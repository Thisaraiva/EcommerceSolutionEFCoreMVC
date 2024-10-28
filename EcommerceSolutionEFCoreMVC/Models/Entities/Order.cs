using Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceSolutionEFCoreMVC.Models.Entities
{
    public class Order
    {
        public int OrderId { get; set; }
        public string ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }
        public DateTime OrderDate { get; set; }
        public OrderStatus Status { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalAmount { get; set; }
        public List<OrderItem>? OrderItems { get; set; }
    }

}
