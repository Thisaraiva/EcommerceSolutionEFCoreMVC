﻿using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceSolutionEFCoreMVC.Models.Entities
{
    public class OrderItem
    {
        public int OrderItemId { get; set; }
        public int OrderId { get; set; }
        public Order Order { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; }
        public int Quantity { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal UnitPrice { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Subtotal { get; set; }

        public void CalculateSubtotal()
        {
            Subtotal = UnitPrice * Quantity;
        }
    }
}
