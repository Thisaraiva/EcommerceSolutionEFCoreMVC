using EcommerceSolutionEFCoreMVC.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;

namespace EcommerceSolutionEFCoreMVC.Models.Entities
{
    public class Order
    {
        public int OrderId { get; set; }        
        public string ApplicationUserId { get; set; }
        public ApplicationUser? ApplicationUser { get; set; }
        [Display(Name = "Date")]
        [DataType(DataType.Date)]
        public DateTime OrderDate { get; set; }
        public OrderStatus Status { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        [Display(Name = "Total")]
        public decimal TotalAmount { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public List<OrderItem>? OrderItems { get; set; }
        public int? AddressId { get; set; }
        public Address? Address { get; set; }

        public void CalculateTotalAmount()
        {
            TotalAmount = OrderItems?.Sum(item => item.Subtotal) ?? 0;
        }
    }

}
