using EcommerceSolutionEFCoreMVC.Models.Entities;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace EcommerceSolutionEFCoreMVC.Models.ViewModels
{
    public class RegisterViewModel
    {
        
        public string? Email { get; set; }
        private string? _firstName;
        private string? _lastName;
        [PersonalData]
        [Required, StringLength(50, MinimumLength = 3, ErrorMessage = "O {0} deve conter entre {2} e {1} caracteres.")]
        public string? FirstName
        {
            get => _firstName;
            set => _firstName = ToUpper(value);
        }
        [PersonalData]
        [Required, StringLength(50, MinimumLength = 3, ErrorMessage = "O {0} deve conter entre {2} e {1} caracteres.")]
        public string? LastName
        {
            get => _lastName;
            set => _lastName = ToUpper(value);
        }

        public string FullName => $"{FirstName} {LastName}";
        private static string? ToUpper(string? value) => value?.ToUpper();
        [Required, DataType(DataType.Date)]
        public DateTime? BirthDate { get; set; }
        [DataType(DataType.Date)]
        public DateTime? HireDate { get; set; }
        public string? PositionId { get; set; }
        public Position? Position { get; set; }
        public string? RoleId { get; set; }
        public IdentityRole? Role { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public ICollection<Address>? Addresses { get; set; }
    }
}
