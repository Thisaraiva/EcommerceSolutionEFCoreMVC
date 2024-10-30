using EcommerceSolutionEFCoreMVC.Models.Entities;
using Microsoft.AspNetCore.Identity;

namespace EcommerceSolutionEFCoreMVC.Areas.Admin.Models
{
    public class RoleEdit
    {
        public IdentityRole? Role { get; set; }
        public IEnumerable<ApplicationUser>? Members { get; set; }
        public IEnumerable<ApplicationUser>? NonMembers { get; set; }
    }
}
