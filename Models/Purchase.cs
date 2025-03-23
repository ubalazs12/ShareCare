using ShareCare.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShareCare.Models
{
    public class Purchase
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Group")]
        [Display(Name = "Csoport")]
        public string? GroupId { get; set; }

        [Display(Name = "Csoport")]
        public virtual Group? Group { get; set; }

        [Display(Name = "Feltöltő felhasználó")]
        [ForeignKey("UploaderUser")]
        public string? UploaderUserId { get; set; }

        [Display(Name = "Feltöltő felhasználó")]
        public virtual ApplicationUser? UploaderUser { get; set; }

        [Required(ErrorMessage = "Az összeg mező kitöltése kötelező!")]
        [Display(Name = "Vásárlás összege")]
        public double TotalAmount { get; set; }

        [Required(ErrorMessage = "A megjegyzés mező kitöltése kötelező!")]
        [Display(Name = "Megjegyzés")]
        public string Note { get; set; }

        [Display(Name = "Kép adat")]
        public byte[]? ImageData { get; set; }

        [Display(Name = "Kép típus")]
        public string? ImageMimeType { get; set; }

        [Display(Name = "Tartozások")]
        public ICollection<Debt> Debts { get; set; } = [];
    }
}
