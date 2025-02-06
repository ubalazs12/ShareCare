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
        public required string Name { get; set; }

        [ForeignKey("CreatorUser")]
        [Display(Name = "Creator")]
        public string? CreatorUserId { get; set; }

        [Display(Name = "Creator")]
        public virtual ApplicationUser? CreatorUser { get; set; }

        public ICollection<ApplicationUser> Users { get; } = [];
    }
}
