using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EcommerceSolutionEFCoreMVC.Data;
using EcommerceSolutionEFCoreMVC.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

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
                return NotFound();
            }

            var cartItem = cart.ShoppingCartItems.FirstOrDefault(i => i.ProductId == productId);
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
                    Quantity = quantity
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
            var userId = _userManager.GetUserId(User);
            var cart = await _context.ShoppingCarts
                                     .Include(c => c.ShoppingCartItems)
                                     .FirstOrDefaultAsync(c => c.ApplicationUserId == userId);

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

        // POST: ShoppingCarts/Checkout
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
                OrderItems = cart.ShoppingCartItems.Select(i => new OrderItem
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    Price = i.Product.Price
                }).ToList()
            };

            _context.Orders.Add(order);
            _context.ShoppingCartItems.RemoveRange(cart.ShoppingCartItems);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Orders", new { id = order.OrderId });
        }

    }
}
