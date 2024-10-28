namespace EcommerceSolutionEFCoreMVC.Models.Entities
{
    public class ShoppingCart
    {
        public int ShoppingCartId { get; set; }
        public string ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }

        public ICollection<ShoppingCartItem> ShoppingCartItems { get; set; } = new List<ShoppingCartItem>();
    }
}
