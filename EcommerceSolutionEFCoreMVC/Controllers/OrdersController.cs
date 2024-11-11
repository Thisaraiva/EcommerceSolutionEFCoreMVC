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
    public class OrdersController : Controller
    {
        private readonly EcommerceDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrdersController(EcommerceDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Orders
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var isAdmin = User.IsInRole("Admin");

            var ordersQuery = _context.Orders
                .Include(o => o.ApplicationUser)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .AsQueryable();

            // Filtra os pedidos para o usuário logado, exceto se ele for Admin
            if (!isAdmin)
            {
                ordersQuery = ordersQuery.Where(o => o.ApplicationUserId == userId);
            }

            return View(await ordersQuery.ToListAsync());
        }

        // GET: Orders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var userId = _userManager.GetUserId(User);

            var order = await _context.Orders
                .Include(o => o.ApplicationUser)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(m => m.OrderId == id);

            if (order == null || (!User.IsInRole("Admin") && order.ApplicationUserId != _userManager.GetUserId(User)))
            {
                return Unauthorized();
            }

            return View(order);
        }

        // GET: Orders/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null || (!User.IsInRole("Admin") && order.ApplicationUserId != _userManager.GetUserId(User)))
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var usersList = await _context.Users
                .Where(u => u.Id != null && u.UserName != null)
                .ToListAsync();
            ViewData["ApplicationUserId"] = new SelectList(usersList, "Id", "UserName", order.ApplicationUserId);

            return View(order);
        }

        // POST: Orders/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("OrderId,ApplicationUserId,OrderDate,Status,TotalAmount")] Order order)
        {
            if (id != order.OrderId) return NotFound();

            var existingOrder = await _context.Orders.AsNoTracking().FirstOrDefaultAsync(o => o.OrderId == id);

            if (existingOrder == null || (!User.IsInRole("Admin") && existingOrder.ApplicationUserId != _userManager.GetUserId(User)))
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    order.ApplicationUserId = existingOrder.ApplicationUserId;
                    _context.Update(order);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderExists(order.OrderId)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            // Recarrega o SelectList caso a ModelState seja inválida
            ViewData["ApplicationUserId"] = new SelectList(_context.Users, "Id", "UserName", order.ApplicationUserId);
            return View(order);
        }


        [Authorize(Roles = "Admin")]
        // GET: Orders/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var order = await _context.Orders
                .Include(o => o.ApplicationUser)
                .FirstOrDefaultAsync(m => m.OrderId == id);

            if (order == null || (!User.IsInRole("Admin") && order.ApplicationUserId != _userManager.GetUserId(User)))
            {
                return Unauthorized();
            }

            return View(order);
        }

        [Authorize(Roles = "Admin")]
        // POST: Orders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null || (!User.IsInRole("Admin") && order.ApplicationUserId != _userManager.GetUserId(User)))
            {
                return Unauthorized();
            }

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OrderExists(int id)
        {
            return _context.Orders.Any(e => e.OrderId == id);
        }
    }
}
