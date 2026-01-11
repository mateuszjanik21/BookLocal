using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookLocal.Data.Models
{
    public enum DocumentType
    {
        IDCard,
        BusinessLicense,
        PremisesLeaseAgreement,
        SanepidApproval
    }

    public class VerificationDocument
    {
        [Key]
        public int DocumentId { get; set; }

        [Required]
        public int VerificationId { get; set; }
        [ForeignKey("VerificationId")]
        public virtual BusinessVerification Verification { get; set; }

        public DocumentType Type { get; set; }
        public string FileUrl { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }
}