using ShareCare.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShareCare.Models
{
    public class Group
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Id { get; set; }

        [Required]
        public string Name { get; set; }

        [ForeignKey("CreatorUser")]
        [Display(Name = "Creator")]
        public string CreatorUserId { get; set; }

        [Display(Name = "Creator")]
        public virtual ApplicationUser? CreatorUser { get; set; }

        public ICollection<ApplicationUser> Users { get; } = [];

        public Group(string name)
        {
            Id = GenerateRandomId();
            Name = name;
        }

        public Group()
        {
            Id = GenerateRandomId();
        }

        private string GenerateRandomId()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            Random random = new Random();
            return new string(Enumerable.Repeat(chars, 5)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
