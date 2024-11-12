using System.ComponentModel.DataAnnotations;

namespace EcommerceSolutionEFCoreMVC.Models.Entities
{
    public class Address
    {
        public int AddressId { get; set; }
        [Display(Name  = "User")]
        public string ApplicationUserId { get; set; }
        public ApplicationUser? ApplicationUser { get; set; }        

        public string? Street { get; set; } = string.Empty;


        public string? Number { get; set; } = string.Empty;

        public string? Complement { get; set; }

        public string? Neighborhood { get; set; } = string.Empty;


        public string? City { get; set; } = string.Empty;


        public string? State { get; set; } = string.Empty;


        public string? Country { get; set; } = string.Empty;

        [DataType(DataType.PostalCode)]
        public string ZipCode { get; set; } = string.Empty;
        public bool IsSelected { get; set; }

        // Propriedade auxiliar para retornar o endereço completo.
        public string FullAddress => $"{Street}, {Number} - {Neighborhood}, {City} - {State}, {ZipCode}, {Country}";

    }
}
