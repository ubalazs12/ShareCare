using ShareCare.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShareCare.Models
{
    public class Group
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string? Id { get; set; } = "INITIAL_ID";

        [Required]
        [Display(Name = "Csoportnév")]
        public required string Name { get; set; }

        [ForeignKey("CreatorUser")]
        [Display(Name = "Csoport tulajdonosa")]
        public string? CreatorUserId { get; set; }

        [Display(Name = "Csoport tulajdonosa")]
        public virtual ApplicationUser? CreatorUser { get; set; }

        [Display(Name = "Tagok")]
        public ICollection<ApplicationUser> Users { get; } = [];

        [Display(Name = "Vásárlások")]
        public ICollection<Purchase> Purchases { get; set; } = [];

        [Display(Name = "Tartozások")]
        public ICollection<Debt> Debts { get; set; } = [];
    }
}
