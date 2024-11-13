using System;
using System.Collections.Generic;
using EcommerceSolutionEFCoreMVC.Models.Entities;
using EcommerceSolutionEFCoreMVC.Models.Enums;

namespace EcommerceSolutionEFCoreMVC.Models.ViewModels
{
    public class OrderSummaryViewModel
    {
        // Informações do usuário e endereço de entrega
        public string ApplicationUserId { get; set; }        
        public string UserName { get; set; }
        public int? SelectedAddressId { get; set; }
        public Address? SelectedAddress { get; set; }

        public ShoppingCart ShoppingCart { get; set; }

        // Lista de itens no carrinho para revisão
        public List<OrderItemSummary> OrderItems { get; set; } = new List<OrderItemSummary>();

        // Total do pedido
        public decimal TotalAmount { get; set; }

        // Método de pagamento selecionado
        public PaymentMethod? SelectedPaymentMethod { get; set; }

        // Data do pedido, para visualização
        public DateTime OrderDate { get; set; } = DateTime.Now;        

        public class OrderItemSummary
        {
            public int ProductId { get; set; }
            public string ProductName { get; set; }
            public decimal UnitPrice { get; set; }
            public int Quantity { get; set; }
            public decimal Subtotal => UnitPrice * Quantity;
        }
    }
}
