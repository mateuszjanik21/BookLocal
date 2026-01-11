using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookLocal.Data.Models
{
    public enum VerificationStatus
    {
        Pending,
        Approved,
        Rejected,
        MoreInfoNeeded
    }

    public class BusinessVerification
    {
        [Key]
        public int VerificationId { get; set; }

        [Required]
        public int BusinessId { get; set; }
        [ForeignKey("BusinessId")]
        public virtual Business Business { get; set; }

        public VerificationStatus Status { get; set; } = VerificationStatus.Pending;

        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ReviewedAt { get; set; }

        public string? AdminNotes { get; set; }
        public string? RejectionReason { get; set; }
    }
}