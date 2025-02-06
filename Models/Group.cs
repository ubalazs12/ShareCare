using ShareCare.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShareCare.Models
{
    public class Group
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [ForeignKey("CreatorUser")]
        [Display(Name = "Creator")]
        public string CreatorUserId { get; set; }

        [Display(Name = "Creator")]
        public virtual ApplicationUser? CreatorUser { get; set; }

        public ICollection<ApplicationUser> Users { get; } = [];
    }
}
