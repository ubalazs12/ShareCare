using ShareCare.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShareCare.Models
{
    public class Debt
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Group")]
        public string? GroupId { get; set; }

        public virtual Group? Group { get; set; }

        [ForeignKey("Purchase")]
        public int? PurchaseId { get; set; }

        public virtual Purchase? Purchase { get; set; }

        [ForeignKey("UploaderUser")]
        public string? UploaderUserId { get; set; }

        public virtual ApplicationUser? UploaderUser { get; set; }

        [ForeignKey("OwerUser")]
        public string? OwerUserId { get; set; }

        public virtual ApplicationUser? OwerUser { get; set; }


        [Required]
        public double Amount { get; set; }

        public eApprovalState ApprovalState { get; set; }

        public ePaymentState PaymentState { get; set; }
    }
}
