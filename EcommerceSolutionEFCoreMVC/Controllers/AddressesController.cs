using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EcommerceSolutionEFCoreMVC.Data;
using EcommerceSolutionEFCoreMVC.Models.Entities;
using Microsoft.AspNetCore.Identity;
using EcommerceSolutionEFCoreMVC.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using EcommerceSolutionEFCoreMVC.Models.ErrorViewModel;
using System.Diagnostics;

namespace EcommerceSolutionEFCoreMVC.Controllers
{
    [Authorize]
    public class AddressesController : Controller
    {
        private readonly EcommerceDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<AddressesController> _logger;

        public AddressesController(EcommerceDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<AddressesController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        private async Task<ApplicationUser> GetCurrentUserAsync()
        {
            return await _userManager.GetUserAsync(User);
        }


        // GET: Addresses
        public async Task<IActionResult> Index()
        {
            var applicationUser = await GetCurrentUserAsync();

            if (applicationUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var userAddresses = _context.Addresses
                .Where(a => a.ApplicationUserId == applicationUser.Id)
                .Include(a => a.ApplicationUser);

            return View(await userAddresses.ToListAsync());
        }


        // GET: Addresses/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var applicationUser = await GetCurrentUserAsync();

            var address = await _context.Addresses
                .Include(a => a.ApplicationUser)
                .FirstOrDefaultAsync(m => m.AddressId == id);

            if (address == null)
            {
                return NotFound("Address not found for this user.");
            }

            return View(address);
        }

        // GET: Addresses/Create
        public async Task<IActionResult> Create()
        {
            var applicationUser = await GetCurrentUserAsync();

            if (applicationUser == null)
            {
                _logger.LogWarning("Tentativa de criação de endereço sem usuário logado.");
                return NotFound("There is no user found");
            }

            _logger.LogInformation("Página de criação de endereço carregada para o usuário {UserId}", applicationUser.Id);
            ViewData["ApplicationUserFullName"] = applicationUser.FullName ?? "Nome Não encontrado";

            return View();
        }

        // POST: Addresses/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FullName,Street,Number,Complement,Neighborhood,City,State,Country,ZipCode")] string applicationUserId, AddressViewModel addressVM)
        {
            var applicationUser = await GetCurrentUserAsync();

            if (applicationUser == null)
            {
                _logger.LogWarning("Tentativa de criação de endereço sem usuário logado.");
                return NotFound("There is no user found");
            }

            addressVM.ApplicationUserId = applicationUser.Id;
            _logger.LogInformation($"Usuario atribuido na view model para criar endereço: ID {addressVM.ApplicationUserId}");

            if (!ModelState.IsValid)
            {
                foreach (var modelState in ModelState)
                {
                    foreach (var error in modelState.Value.Errors)
                    {
                        _logger.LogWarning("Erro no campo {Field}: {Error}", modelState.Key, error.ErrorMessage);
                    }
                }
                _logger.LogWarning("Modelo de endereço inválido para o usuário {UserId}. Erros: {Errors}", applicationUser.Id, ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return View(addressVM);
            }

            var address = new Address
            {
                ApplicationUserId = applicationUser.Id,
                Street = addressVM.Street,
                Number = addressVM.Number,
                Complement = addressVM.Complement,
                Neighborhood = addressVM.Neighborhood,
                City = addressVM.City,
                State = addressVM.State,
                Country = addressVM.Country,
                ZipCode = addressVM.ZipCode
            };

            var submittedValues = HttpContext.Request.Form.ToDictionary(x => x.Key, x => x.Value.FirstOrDefault());
            _logger.LogInformation("Valores enviados pelo formulário: {Values}", submittedValues);            

            try
            {
                _context.Add(address);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Endereço criado com sucesso para o usuário {UserId}", applicationUser.Id);

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao tentar criar endereço para o usuário {UserId}", applicationUser.Id);
                ModelState.AddModelError("", "Ocorreu um erro ao salvar o endereço. Tente novamente.");

                ViewData["ApplicationUserFullName"] = applicationUser.FullName;                
            }
            return View(addressVM);
        }

        // GET: Addresses/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var applicationUser = await GetCurrentUserAsync();

            var address = await _context.Addresses
                .Where(a => a.ApplicationUserId == applicationUser.Id && a.AddressId == id)
                .FirstOrDefaultAsync();

            if (address == null)
            {
                return NotFound("Address not found for this user.");
            }
            ViewData["ApplicationUserFullName"] = applicationUser.FullName;

            return View(address);
        }

        // POST: Addresses/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("AddressId,Street,Number,Complement,Neighborhood,City,State,Country,ZipCode")] AddressViewModel addressVM)
        {
            var applicationUser = await GetCurrentUserAsync();

            if (applicationUser == null)
            {
                return NotFound("Usuário não encontrado.");
            }

            if (id != addressVM.AddressId)
            {
                return NotFound("Endereço não encontrado para este usuário.");
            }

            addressVM.ApplicationUserId = applicationUser.Id;

            if (ModelState.IsValid)
            {
                try
                {
                    var address = await _context.Addresses.FindAsync(id);
                    if (address == null || address.ApplicationUserId != applicationUser.Id)
                    {
                        return NotFound("Endereço não encontrado para este usuário.");
                    }

                    // Atualiza as propriedades do endereço
                    address.Street = addressVM.Street;
                    address.Number = addressVM.Number;
                    address.Complement = addressVM.Complement;
                    address.Neighborhood = addressVM.Neighborhood;
                    address.City = addressVM.City;
                    address.State = addressVM.State;
                    address.Country = addressVM.Country;
                    address.ZipCode = addressVM.ZipCode;

                    _context.Update(address);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AddressExists(addressVM.AddressId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["ApplicationUserId"] = new List<SelectListItem>();
            ViewData["ApplicationUserFullName"] = applicationUser.FullName;

            return View(addressVM);
        }

        // GET: Addresses/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var applicationUser = await GetCurrentUserAsync();

            var address = await _context.Addresses
                .Include(a => a.ApplicationUser)
                .FirstOrDefaultAsync(m => m.AddressId == id && m.ApplicationUserId == applicationUser.Id);

            if (address == null)
            {
                return NotFound("Address not found for this user.");
            }

            return View(address);
        }

        // POST: Addresses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var applicationUser = await GetCurrentUserAsync();

            var address = await _context.Addresses
                .Where(a => a.AddressId == id && a.ApplicationUserId == applicationUser.Id)
                .FirstOrDefaultAsync();

            if (address != null)
            {
                _context.Addresses.Remove(address);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool AddressExists(int id)
        {
            return _context.Addresses.Any(e => e.AddressId == id);
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}