using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EcommerceSolutionEFCoreMVC.Data;
using EcommerceSolutionEFCoreMVC.Models.Entities;
using EcommerceSolutionEFCoreMVC.Models.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using EcommerceSolutionEFCoreMVC.Services.Interfaces;

namespace EcommerceSolutionEFCoreMVC.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly EcommerceDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;

        public OrdersController(EcommerceDbContext context, UserManager<ApplicationUser> userManager, IEmailService emailService)
        {
            _context = context;
            _userManager = userManager;
            _emailService = emailService;
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
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Include(o => o.ApplicationUser)
                .Include(o => o.Address)
                .FirstOrDefaultAsync(m => m.OrderId == id);

            if (order == null || (!User.IsInRole("Admin") && order.ApplicationUserId != _userManager.GetUserId(User)))
            {
                return Unauthorized();
            }

            return View(order);
        }

        //GET: Orders/Edit/5
        [Authorize(Roles = "Admin")]
        // GET: Products/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {                
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);            

            var order = await _context.Orders
                .Include(o => o.ApplicationUser)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null)
            {                
                return NotFound();
            }

            ViewBag.StatusOptions = Enum.GetValues(typeof(OrderStatus))
            .Cast<OrderStatus>()
            .Select(status => new SelectListItem
            {
                Value = ((int)status).ToString(),
                Text = status.ToString(),
                Selected = order.Status == status
            }).ToList();

            ViewData["ApplicationUserId"] = order.ApplicationUserId;
            ViewData["ApplicationUser"] = User;
            ViewData["UserName"] = order.ApplicationUser?.FullName ?? "Unknown User";

            return View(order);
        }


        // POST: Orders/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("OrderId,ApplicationUserId,OrderDate,Status,TotalAmount,PaymentMethod,AddressId")]
            Order order)
        {
            if (id != order.OrderId)
            {                
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);           

            // Validação adicional
            if (string.IsNullOrEmpty(order.ApplicationUserId))
            {
                ModelState.AddModelError(nameof(order.ApplicationUserId), "ApplicationUserId is required.");
            }

            ViewData["ApplicationUserId"] = order.ApplicationUserId;
            ViewData["ApplicationUser"] = User;


            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(order);
                    await _context.SaveChangesAsync();

                    // Enviar e-mail de atualização de status
                    var user = await _userManager.FindByIdAsync(order.ApplicationUserId);
                    await _emailService.SendOrderStatusUpdateEmail(user.Email, order);

                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderExists(order.OrderId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }                
            }            
            return View(order);
        }        

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null || (!User.IsInRole("Admin") && order.ApplicationUserId != _userManager.GetUserId(User)))
            {
                return Unauthorized();
            }

            // Verificar se o pedido já foi cancelado ou entregue
            if (order.Status == OrderStatus.Delivered ||
                order.Status == OrderStatus.Shipped ||
                order.Status == OrderStatus.Cancelled)
            {
                ModelState.AddModelError("", "This order cannot be cancelled.");
                return RedirectToAction(nameof(Details), new { id });
            }

            // Atualizar o status para "Cancelled"
            order.Status = OrderStatus.Cancelled;
            _context.Update(order);
            await _context.SaveChangesAsync();

            // Enviar e-mail de atualização de status
            var user = await _userManager.FindByIdAsync(order.ApplicationUserId);
            await _emailService.SendOrderStatusUpdateEmail(user.Email, order);

            TempData["SuccessMessage"] = "Order has been successfully cancelled.";
            return RedirectToAction(nameof(Details), new { id });
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
