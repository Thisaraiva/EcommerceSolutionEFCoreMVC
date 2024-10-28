using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace EcommerceSolutionEFCoreMVC.Models.Entities
{
    public class ApplicationUser : IdentityUser
    {
        
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

        [PersonalData]
        public ICollection<Address> Addresses { get; set; } = new HashSet<Address>();
        [PersonalData]
        [DataType(DataType.Date)]
        public DateTime BirthDate { get; set; }
        [PersonalData]
        [DataType(DataType.DateTime)]
        public DateTime RegisterDate { get; private set; } = DateTime.UtcNow;
        [PersonalData]
        public int ShoppingCartId { get; set; }
        public ShoppingCart ShoppingCart { get; set; }
        [PersonalData]
        public ICollection<Order> Orders { get; set; } = new List<Order>();
        [PersonalData]
        public int PositionId { get; set; }
        [PersonalData]
        public Position Position { get; set; }
        [PersonalData]
        public DateTime HireDate { get; set; } = DateTime.UtcNow;

        private static string? ToUpper(string? value) => value?.ToUpper();

        public void AddAddress(Address address)
        {
            Addresses.Add(address);
        }
    }
}
