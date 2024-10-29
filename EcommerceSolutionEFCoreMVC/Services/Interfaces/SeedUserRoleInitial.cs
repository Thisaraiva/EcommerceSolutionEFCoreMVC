
using EcommerceSolutionEFCoreMVC.Models.Entities;
using Microsoft.AspNetCore.Identity;

namespace EcommerceSolutionEFCoreMVC.Services.Interfaces
{
    public class SeedUserRoleInitial : ISeedUserRoleInitial
    {

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public SeedUserRoleInitial(UserManager<ApplicationUser> userManager, 
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task SeedRolesAsync()
        {
            if (!await _roleManager.RoleExistsAsync("Customer"))
            {
                IdentityRole role = new IdentityRole();
                role.Name = "Customer";
                role.NormalizedName = "CUSTOMER";
                role.ConcurrencyStamp = Guid.NewGuid().ToString();

                IdentityResult roleResult = await _roleManager.CreateAsync(role);
            }

            if (!await _roleManager.RoleExistsAsync("Manager"))
            {
                IdentityRole role = new IdentityRole();
                role.Name = "Manager";
                role.NormalizedName = "MANAGER";
                role.ConcurrencyStamp = Guid.NewGuid().ToString();

                IdentityResult roleResult = await _roleManager.CreateAsync(role);
            }

            if (!await _roleManager.RoleExistsAsync("Admin"))
            {
                IdentityRole role = new IdentityRole();
                role.Name = "Admin";
                role.NormalizedName = "ADMIN";
                role.ConcurrencyStamp = Guid.NewGuid().ToString();

                IdentityResult roleResult = await _roleManager.CreateAsync(role);
            }


        }

        public async Task SeedUsersAsync()
        {
            if (await _userManager.FindByEmailAsync("user@user.com") == null)
            {
                ApplicationUser admin = new ApplicationUser();

                admin.FirstName = "User";
                admin.LastName = "Common";
                admin.UserName = admin.FullName;
                admin.Email = "user@user.com";
                admin.NormalizedUserName = admin.FullName.ToUpper();
                admin.NormalizedEmail = "USER@USER.COM";
                admin.EmailConfirmed = true;
                admin.LockoutEnabled = false;
                admin.SecurityStamp = Guid.NewGuid().ToString();

                IdentityResult result = await _userManager.CreateAsync(admin, "User@123");

                if (!result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(admin, "User");
                }
            }

            if (await _userManager.FindByEmailAsync("admin@admin.com") == null)
            {
                ApplicationUser admin = new ApplicationUser();

                admin.FirstName = "Admin";
                admin.LastName = "User";
                admin.UserName = admin.FullName;
                admin.Email = "admin@admin.com";
                admin.NormalizedUserName = admin.FullName.ToUpper();
                admin.NormalizedEmail = "ADMIN@ADMIN.COM";
                admin.EmailConfirmed = true;
                admin.LockoutEnabled = false;
                admin.SecurityStamp = Guid.NewGuid().ToString();

                IdentityResult result = await _userManager.CreateAsync(admin, "Admin@123");

                if (!result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(admin, "Admin");
                }
            }
        }
    }
}
