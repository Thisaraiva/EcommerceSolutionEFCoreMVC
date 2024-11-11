using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EcommerceSolutionEFCoreMVC.Data;
using EcommerceSolutionEFCoreMVC.Models.Entities;
using Microsoft.AspNetCore.Identity;

namespace EcommerceSolutionEFCoreMVC.Controllers
{
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


        // GET: Orders/Create
        public IActionResult Create()
        {
            ViewData["ApplicationUserId"] = new SelectList(_context.Users, "ApplicationUserId", "Name");
            return View();
        }

        // POST: Orders/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("OrderId,ApplicationUserId,OrderDate,Status,TotalAmount")] Order order)
        {
            if (ModelState.IsValid)
            {
                // Buscando todos os OrderItems relacionados de uma vez só
                order.OrderItems = await _context.OrderItems
                    .Where(oi => oi.OrderId == order.OrderId)
                    .ToListAsync();

                // Calcular subtotal para cada OrderItem
                foreach (var item in order.OrderItems)
                {
                    item.CalculateSubtotal();
                }

                // Calcular o TotalAmount da Order
                order.CalculateTotalAmount();

                _context.Add(order);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ApplicationUserId"] = new SelectList(_context.Users, "ApplicationUserId", "Name", order.ApplicationUserId);
            return View(order);
        }


        // GET: Orders/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null || (!User.IsInRole("Admin") && order.ApplicationUserId != _userManager.GetUserId(User)))
            {
                return Unauthorized();
            }


            ViewData["ApplicationUserId"] = new SelectList(_context.Users, "ApplicationUserId", "Name", order.ApplicationUserId);
            return View(order);
        }


        // POST: Orders/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("OrderId,ApplicationUserId,OrderDate,Status,TotalAmount")] Order order)
        {
            if (id != order.OrderId) return NotFound();

            var existingOrder = await _context.Orders.AsNoTracking().FirstOrDefaultAsync(o => o.OrderId == id);
            if (existingOrder == null || (!User.IsInRole("Admin") && existingOrder.ApplicationUserId != _userManager.GetUserId(User)))
            {
                return Unauthorized();
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
            ViewData["ApplicationUserId"] = new SelectList(_context.Users, "ApplicationUserId", "Name", order.ApplicationUserId);
            return View(order);
        }

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
