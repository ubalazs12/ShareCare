using Microsoft.AspNetCore.Identity;
using ShareCare.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShareCare.Data
{
    public class ApplicationUser : IdentityUser
    {
        [Required(ErrorMessage = "Az email cím mező kitöltése kötelező!")]
        public override string Email { get; set; }

        [Required(ErrorMessage = "A felhasználónév mező kitöltése kötelező!")]
        public override string UserName { get; set; }


        [Required(ErrorMessage = "A vezetéknév mező kitöltése kötelező!")]
        [Display(Name = "Vezetéknév")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "A keresztnév mező kitöltése kötelező!")]
        [Display(Name = "Keresztnév")]
        public string LastName { get; set; }

        [NotMapped]
        [Display(Name = "Teljes név")]
        public string FullName => $"{FirstName} {LastName}";

        [Display(Name = "Csoportok")]
        public ICollection<Group> Groups { get; } = [];

        [Display(Name = "Vásárlások")]
        public ICollection<Purchase> Purchases { get; set; } = [];

        [Display(Name = "Tartozások másnak")]
        public ICollection<Debt> Debts { get; set; } = [];

        [Display(Name = "Tartozások nekem")]
        public ICollection<Debt> Credits { get; set; } = [];
    }
}
