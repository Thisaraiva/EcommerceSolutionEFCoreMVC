using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EcommerceSolutionEFCoreMVC.Data;
using EcommerceSolutionEFCoreMVC.Models.Entities;
using EcommerceSolutionEFCoreMVC.Models.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using EcommerceSolutionEFCoreMVC.Models.ViewModels;
using Humanizer;
using EcommerceSolutionEFCoreMVC.Services.Interfaces;

namespace EcommerceSolutionEFCoreMVC.Controllers
{
    [Authorize]
    public class ShoppingCartsController : Controller
    {
        private readonly EcommerceDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;

        public ShoppingCartsController(EcommerceDbContext context, UserManager<ApplicationUser> userManager, IEmailService emailService)
        {
            _context = context;
            _userManager = userManager;
            _emailService = emailService;
        }

        private async Task<string> GetUserIdAsync() => await Task.FromResult(_userManager.GetUserId(User));

        private async Task<ShoppingCart> GetUserCartAsync()
        {            
            var userId = await GetUserIdAsync();
            return await _context.ShoppingCarts
                                 .Include(c => c.ShoppingCartItems)
                                 .ThenInclude(ci => ci.Product)
                                 .FirstOrDefaultAsync(c => c.ApplicationUserId == userId);
        }

        // GET: ShoppingCarts
        public async Task<IActionResult> Index()
        {
            var shoppingCart = await GetUserCartAsync();
            return View(shoppingCart);
        }

        // POST: ShoppingCarts/AddProduct
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddProduct(int productId, int quantity = 1)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }
            var cart = await GetUserCartAsync();

            if (cart == null)
            {
                // Cria um novo carrinho se o usuário não tiver um.
                cart = new ShoppingCart { ApplicationUserId = await GetUserIdAsync() };
                _context.ShoppingCarts.Add(cart);
                await _context.SaveChangesAsync();
            }

            var cartItem = cart.ShoppingCartItems.FirstOrDefault(i => i.ProductId == productId);
            var product = await _context.Products.FindAsync(productId);

            if (product == null)
            {
                return NotFound("Produto não encontrado.");
            }

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

            var cart = await GetUserCartAsync();

            if (cart == null) return NotFound();

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
            var cart = await GetUserCartAsync();

            if (cart == null) return NotFound();

            var cartItem = cart.ShoppingCartItems.FirstOrDefault(i => i.ProductId == productId);
            if (cartItem != null)
            {
                _context.ShoppingCartItems.Remove(cartItem);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }        

        public IActionResult SelectPaymentMethod()
        {
            // Exibir opções de pagamento
            return View();
        }

        [HttpPost, ActionName("SelectPaymentMethod")]
        [ValidateAntiForgeryToken]
        public IActionResult SelectPaymentMethodConfirm(int paymentMethod)
        {
            // Verificar se o valor recebido é um valor válido de PaymentMethod
            if (!Enum.IsDefined(typeof(PaymentMethod), paymentMethod))
            {
                ModelState.AddModelError(string.Empty, "Método de pagamento inválido.");
                return View("SelectPaymentMethod");
            }
            // Armazenar o método de pagamento selecionado
            TempData["SelectedPaymentMethodId"] = paymentMethod;
            TempData.Keep("SelectedPaymentMethodId");

            // Prosseguir para a revisão do pedido
            return RedirectToAction("ReviewOrder", "ShoppingCarts");
        }


        public async Task<IActionResult> ReviewOrder()
        {
            var cart = await GetUserCartAsync();

            if (cart == null || !cart.ShoppingCartItems.Any())
            {
                return RedirectToAction(nameof(Index));
            }                        

            var user = await _userManager.GetUserAsync(User);

            // Recuperar o endereço selecionado e método de pagamento do TempData
            int? selectedAddressId = TempData["SelectedAddressId"] as int?;
            int? selectedPaymentMethodId = TempData["SelectedPaymentMethodId"] as int?;
            TempData.Keep("SelectedAddressId");
            TempData.Keep("SelectedPaymentMethodId");

            // Verificação adicional para evitar exceção de valor nulo
            if (selectedAddressId == null || selectedPaymentMethodId == null)
            {
                // Redireciona para a seleção de endereço e pagamento se algum estiver ausente
                return RedirectToAction("Index", "ShoppingCarts");
            }

            var selectedAddress = await _context.Addresses
                                                .FirstOrDefaultAsync(a => a.ApplicationUserId == user.Id && a.AddressId == selectedAddressId);

                  
            var orderSummary = new OrderSummaryViewModel
            {
                ApplicationUserId = user.Id,
                UserName = user?.FullName,
                SelectedAddressId = selectedAddressId.Value,
                SelectedAddress = selectedAddress, // Adicione esta linha
                ShoppingCart = cart,
                TotalAmount = cart.ShoppingCartItems.Sum(i => i.Subtotal),
                SelectedPaymentMethod = (PaymentMethod)selectedPaymentMethodId, // ou outro método selecionado
                OrderItems = cart.ShoppingCartItems.Select(i => new OrderSummaryViewModel.OrderItemSummary
                {
                    ProductId = i.ProductId,
                    ProductName = i.Product.Name,
                    UnitPrice = i.UnitPrice,
                    Quantity = i.Quantity,
                }).ToList(),
                OrderDate = DateTime.Now
                
            };            

            return View(orderSummary);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmOrder()
        {
            var cart = await GetUserCartAsync();

            if (cart == null || !cart.ShoppingCartItems.Any())
            {
                return RedirectToAction("Index", "ShoppingCarts");
            }            

            // Verificar se o endereço e método de pagamento foram selecionados
            if (!(TempData["SelectedAddressId"] is int selectedAddressId) ||
                !(TempData["SelectedPaymentMethodId"] is int selectedPaymentMethodId))
            {
                return RedirectToAction("SelectDeliveryAddress", "Addresses");
            }

            // Criação do novo pedido
            var order = new Order
            {
                ApplicationUserId = await GetUserIdAsync(),
                OrderDate = DateTime.Now,
                Status = OrderStatus.Created,                
                PaymentMethod = (PaymentMethod)selectedPaymentMethodId, // Atribui o método de pagamento
                //TotalAmount = cart.ShoppingCartItems.Sum(i => i.Subtotal),
                AddressId = selectedAddressId,
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
            // Limpa o carrinho do usuário após a confirmação do pedido
            _context.ShoppingCartItems.RemoveRange(cart.ShoppingCartItems);

            await _context.SaveChangesAsync();

            // Enviar e-mail de confirmação
            var user = await _userManager.GetUserAsync(User);
            await _emailService.SendOrderConfirmationEmail(user.Email, order);

            // Redireciona para a página de detalhes do pedido confirmado
            return RedirectToAction("Details", "Orders", new { id = order.OrderId });
        }

    }
}
