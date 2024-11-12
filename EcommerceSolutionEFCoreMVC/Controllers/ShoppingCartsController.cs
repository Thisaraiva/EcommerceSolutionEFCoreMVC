using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EcommerceSolutionEFCoreMVC.Data;
using EcommerceSolutionEFCoreMVC.Models.Entities;
using EcommerceSolutionEFCoreMVC.Models.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using EcommerceSolutionEFCoreMVC.Models.ViewModels;

namespace EcommerceSolutionEFCoreMVC.Controllers
{
    [Authorize]
    public class ShoppingCartsController : Controller
    {
        private readonly EcommerceDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ShoppingCartsController(EcommerceDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private async Task<ShoppingCart> GetUserCartAsync()
        {
            var userId = _userManager.GetUserId(User);
            return await _context.ShoppingCarts
                                 .Include(c => c.ShoppingCartItems)
                                 .ThenInclude(ci => ci.Product)
                                 .FirstOrDefaultAsync(c => c.ApplicationUserId == userId);
        }


        // GET: ShoppingCarts
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User); // Obtém o ID do usuário logado
            var shoppingCart = await _context.ShoppingCarts
                .Include(c => c.ShoppingCartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.ApplicationUserId == userId);

            return View(shoppingCart);
        }

        // POST: ShoppingCarts/AddProduct
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddProduct(int productId, int quantity = 1)
        {
            var userId = _userManager.GetUserId(User);
            var cart = await _context.ShoppingCarts
                                     .Include(c => c.ShoppingCartItems)
                                     .FirstOrDefaultAsync(c => c.ApplicationUserId == userId);

            if (cart == null)
            {
                // Cria um novo carrinho se o usuário não tiver um.
                cart = new ShoppingCart { ApplicationUserId = userId };
                _context.ShoppingCarts.Add(cart);
                await _context.SaveChangesAsync();
            }

            var cartItem = cart.ShoppingCartItems.FirstOrDefault(i => i.ProductId == productId);
            var product = await _context.Products.FindAsync(productId);

            if (cartItem != null)
            {
                cartItem.Quantity += quantity;
            }
            else
            {
                cartItem = new ShoppingCartItem
                {
                    ShoppingCartId = cart.ShoppingCartId,
                    ProductId = productId,
                    Quantity = quantity,
                    UnitPrice = product.Price // Atribui o preço do produto ao item

                };
                _context.ShoppingCartItems.Add(cartItem);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        // POST: ShoppingCarts/UpdateProductQuantity
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProductQuantity(int productId, int quantity)
        {
            if (quantity <= 0)
            {
                return await RemoveProduct(productId); // Remove o produto se a quantidade é zero
            }

            var userId = _userManager.GetUserId(User);
            var cart = await _context.ShoppingCarts
                                     .Include(c => c.ShoppingCartItems)
                                     .FirstOrDefaultAsync(c => c.ApplicationUserId == userId);

            if (cart == null)
                return NotFound();

            var cartItem = cart.ShoppingCartItems.FirstOrDefault(i => i.ProductId == productId);
            if (cartItem != null)
            {
                cartItem.Quantity = quantity;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: ShoppingCarts/RemoveProduct
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveProduct(int productId)
        {
            var userId = _userManager.GetUserId(User);
            var cart = await _context.ShoppingCarts
                                     .Include(c => c.ShoppingCartItems)
                                     .FirstOrDefaultAsync(c => c.ApplicationUserId == userId);

            var cartItem = cart.ShoppingCartItems.FirstOrDefault(i => i.ProductId == productId);
            if (cartItem != null)
            {
                _context.ShoppingCartItems.Remove(cartItem);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        /*// POST: ShoppingCarts/Checkout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout()
        {
            var userId = _userManager.GetUserId(User);
            var cart = await _context.ShoppingCarts
                                     .Include(c => c.ShoppingCartItems)
                                     .ThenInclude(i => i.Product)
                                     .FirstOrDefaultAsync(c => c.ApplicationUserId == userId);

            if (cart == null || !cart.ShoppingCartItems.Any())
            {
                return RedirectToAction(nameof(Index));
            }

            var order = new Order
            {
                ApplicationUserId = userId,
                OrderDate = DateTime.Now,
                Status = OrderStatus.Created,
                OrderItems = cart.ShoppingCartItems.Select(i => new OrderItem
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    UnitPrice = i.Product.Price,
                    Subtotal = i.Subtotal
                }).ToList(),
            };
            order.CalculateTotalAmount();
            
            _context.Orders.Add(order);
            _context.ShoppingCartItems.RemoveRange(cart.ShoppingCartItems);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Orders", new { id = order.OrderId });
        }*/

        public IActionResult SelectPaymentMethod()
        {
            // Exibir opções de pagamento
            return View();
        }

        [HttpPost, ActionName("SelectPaymentMethod")]
        [ValidateAntiForgeryToken]
        public IActionResult ConfirmPaymentMethod(int paymentMethodId)
        {
            // Salvar o método de pagamento selecionado
            TempData["SelectPaymentMethod"] = paymentMethodId;

            // Prosseguir para a revisão do pedido
            return RedirectToAction("SelectPaymentMethod");
        }

        public async Task<IActionResult> ReviewOrder()
        {
            var userId = _userManager.GetUserId(User);
            var cart = await _context.ShoppingCarts
                                     .Include(c => c.ShoppingCartItems)
                                     .ThenInclude(i => i.Product)
                                     .FirstOrDefaultAsync(c => c.ApplicationUserId == userId);

            if (cart == null || !cart.ShoppingCartItems.Any())
            {
                return RedirectToAction(nameof(Index));
            }

            var user = await _userManager.GetUserAsync(User);
            var selectedAddress = await _context.Addresses
                                                .FirstOrDefaultAsync(a => a.ApplicationUserId == userId);

            var orderSummary = new OrderSummaryViewModel
            {
                ApplicationUserId = userId,
                UserName = user?.UserName,
                SelectedAddress = selectedAddress,
                ShoppingCart = cart,
                TotalAmount = cart.ShoppingCartItems.Sum(i => i.Subtotal),
                SelectedPaymentMethod = PaymentMethod.CreditCard, // ou outro método selecionado
                OrderItems = cart.ShoppingCartItems.Select(i => new OrderSummaryViewModel.OrderItemSummary
                {
                    ProductId = i.ProductId,
                    ProductName = i.Product.Name,
                    UnitPrice = i.Product.Price,
                    Quantity = i.Quantity                    
                }).ToList(),
                OrderDate = DateTime.Now
            };

            return View(orderSummary);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmOrder()
        {
            var userId = _userManager.GetUserId(User);

            // Carrega o carrinho de compras do usuário
            var cart = await _context.ShoppingCarts
                                      .Include(c => c.ShoppingCartItems)
                                      .ThenInclude(i => i.Product)
                                      .FirstOrDefaultAsync(c => c.ApplicationUserId == userId);

            if (cart == null || !cart.ShoppingCartItems.Any())
            {
                return RedirectToAction("Index", "ShoppingCarts");
            }

            /*// Obtém o método de pagamento selecionado do TempData
            if (TempData["SelectPaymentMethod"] is not PaymentMethod selectedPaymentMethod)
            {
                return RedirectToAction("SelectPaymentMethod");
            }*/

            // Criação do novo pedido
            var order = new Order
            {
                ApplicationUserId = userId,
                OrderDate = DateTime.Now,
                Status = OrderStatus.Created,
                //TotalAmount = cart.ShoppingCartItems.Sum(i => i.Subtotal),
                PaymentMethod = /*selectedPaymentMethod*/ PaymentMethod.CreditCard, // Atribui o método de pagamento
                OrderItems = cart.ShoppingCartItems.Select(i => new OrderItem
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    UnitPrice = i.Product.Price,
                    Subtotal = i.Subtotal,
                }).ToList()
            };

            order.CalculateTotalAmount();

            // Salva o pedido no banco de dados
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Limpa o carrinho do usuário após a confirmação do pedido
            _context.ShoppingCartItems.RemoveRange(cart.ShoppingCartItems);
            await _context.SaveChangesAsync();

            // Redireciona para a página de detalhes do pedido confirmado
            return RedirectToAction("Details", "Orders", new { id = order.OrderId });
        }        

    }
}
