
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
                ApplicationUser user = new ApplicationUser();
                user.FirstName = "User";
                user.LastName = "Common";
                user.UserName = "user@user.com"; // UserName definido diretamente
                user.Email = "user@user.com";
                user.NormalizedUserName = "USERCOMMON";
                user.NormalizedEmail = "USER@USER.COM";
                user.EmailConfirmed = true;
                user.LockoutEnabled = false;
                user.SecurityStamp = Guid.NewGuid().ToString();


                IdentityResult result = await _userManager.CreateAsync(user, "User@123");

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "Customer");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        Console.WriteLine($"Error creating user 'user@user.com': {error.Description}");
                    }
                }
            }

            if (await _userManager.FindByEmailAsync("admin@admin.com") == null)
            {
                ApplicationUser admin = new ApplicationUser
                {
                    FirstName = "Adm",
                    LastName = "Adm",
                    UserName = "admin@admin.com", // UserName definido diretamente
                    Email = "admin@admin.com",
                    NormalizedUserName = "ADM",
                    NormalizedEmail = "ADMIN@ADMIN.COM",
                    EmailConfirmed = true,
                    LockoutEnabled = false,
                    SecurityStamp = Guid.NewGuid().ToString()
                };

                IdentityResult result = await _userManager.CreateAsync(admin, "Admin@123");

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(admin, "Admin");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        Console.WriteLine($"Error creating admin user: {error.Description}");
                    }
                }
            }
            Console.WriteLine("Seeding completed: Users and roles should be created.");

        }

    }
}
