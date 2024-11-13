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
using EcommerceSolutionEFCoreMVC.Models.Enums;

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

        // Método auxiliar para obter o usuário atual e redirecionar caso não esteja logado
        private async Task<ApplicationUser?> GetValidatedUserAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                _logger.LogWarning("Ação não autorizada sem usuário logado.");
                return null;
            }
            return user;
        }

        // Método para mapear AddressViewModel para Address
        private Address MapViewModelToModel(AddressViewModel viewModel, string userId)
        {
            return new Address
            {
                ApplicationUserId = userId,
                Street = viewModel.Street,
                Number = viewModel.Number,
                Complement = viewModel.Complement,
                Neighborhood = viewModel.Neighborhood,
                City = viewModel.City,
                State = viewModel.State,
                Country = viewModel.Country,
                ZipCode = viewModel.ZipCode
            };
        }

        // Método para tratar Address não encontrado
        private IActionResult AddressNotFound()
        {
            _logger.LogWarning("Endereço não encontrado para o usuário.");
            return NotFound("Address not found for this user.");
        }

        // GET: Addresses
        public async Task<IActionResult> Index()
        {
            var applicationUser = await GetValidatedUserAsync();
            if (applicationUser == null) return RedirectToAction("Login", "Account");

            var userAddresses = await _context.Addresses
                .Where(a => a.ApplicationUserId == applicationUser.Id)
                .ToListAsync();

            return View(userAddresses);
        }

        // GET: Addresses/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var applicationUser = await GetValidatedUserAsync();
            if (applicationUser == null) return RedirectToAction("Login", "Account");

            var address = await _context.Addresses
                .FirstOrDefaultAsync(m => m.AddressId == id && m.ApplicationUserId == applicationUser.Id);

            return address == null ? AddressNotFound() : View(address);
        }

        // GET: Addresses/Create
        public async Task<IActionResult> Create()
        {
            var applicationUser = await GetValidatedUserAsync();
            if (applicationUser == null) return RedirectToAction("Login", "Account");

            ViewData["ApplicationUserFullName"] = applicationUser.FullName;
            return View();
        }

        // POST: Addresses/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AddressViewModel addressVM)
        {
            var applicationUser = await GetValidatedUserAsync();
            if (applicationUser == null) return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
            {
                LogModelStateErrors();
                return View(addressVM);
            }

            var address = MapViewModelToModel(addressVM, applicationUser.Id);

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
                return View(addressVM);
            }
        }

        // GET: Addresses/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var applicationUser = await GetValidatedUserAsync();
            if (applicationUser == null) return RedirectToAction("Login", "Account");

            var address = await _context.Addresses
                .FirstOrDefaultAsync(a => a.AddressId == id && a.ApplicationUserId == applicationUser.Id);

            if (address == null) return AddressNotFound();

            ViewData["ApplicationUserFullName"] = applicationUser.FullName;
            return View(address);
        }

        // POST: Addresses/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AddressViewModel addressVM)
        {
            var applicationUser = await GetValidatedUserAsync();
            if (applicationUser == null) return RedirectToAction("Login", "Account");

            if (id != addressVM.AddressId) return AddressNotFound();

            if (!ModelState.IsValid)
            {
                LogModelStateErrors();
                return View(addressVM);
            }

            var address = await _context.Addresses.FindAsync(id);
            if (address == null || address.ApplicationUserId != applicationUser.Id) return AddressNotFound();

            UpdateAddressFromViewModel(address, addressVM);

            try
            {
                _context.Update(address);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AddressExists(addressVM.AddressId)) return NotFound();
                else throw;
            }
        }

        // Método para registrar erros do ModelState
        private void LogModelStateErrors()
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage);
            _logger.LogWarning("Erros de validação: {Errors}", string.Join(", ", errors));
        }

        private void UpdateAddressFromViewModel(Address address, AddressViewModel addressVM)
        {
            address.Street = addressVM.Street;
            address.Number = addressVM.Number;
            address.Complement = addressVM.Complement;
            address.Neighborhood = addressVM.Neighborhood;
            address.City = addressVM.City;
            address.State = addressVM.State;
            address.Country = addressVM.Country;
            address.ZipCode = addressVM.ZipCode;
        }

        // GET: Addresses/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var applicationUser = await GetValidatedUserAsync();

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
            var applicationUser = await GetValidatedUserAsync();

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

        public async Task<IActionResult> SelectDeliveryAddress()
        {
            var userId = _userManager.GetUserId(User);
            var addresses = await _context.Addresses
                                          .Where(a => a.ApplicationUserId == userId)
                                          .Select(a => new AddressViewModel
                                          {
                                              AddressId = a.AddressId,
                                              ApplicationUserId = a.ApplicationUserId,
                                              Street = a.Street,
                                              Number = a.Number,
                                              Complement = a.Complement,
                                              Neighborhood = a.Neighborhood,
                                              City = a.City,
                                              State = a.State,
                                              Country = a.Country,
                                              ZipCode = a.ZipCode                                              
                                          })
                                          .ToListAsync();

            return View(addresses);
        }

        [HttpPost, ActionName("SelectDeliveryAddress")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SelectDeliveryAddressConfirm(int addressId)
        {
            var userId = _userManager.GetUserId(User);

            // Marcar o endereço selecionado para o usuário
            var userAddresses = await _context.Addresses
                                              .Where(a => a.ApplicationUserId == userId)
                                              .ToListAsync();

            foreach (var address in userAddresses)
            {
                address.IsSelected = (address.AddressId == addressId);
            }

            // Salvar mudanças
            await _context.SaveChangesAsync();

            // Armazenar o endereço selecionado no TempData
            TempData["SelectedAddressId"] = addressId;
            TempData.Keep("SelectedAddressId");

            // Redirecionar para a seleção do método de pagamento
            return RedirectToAction("SelectPaymentMethod", "ShoppingCarts");
        }
    }
}