using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookLocal.Data.Models
{
    public enum WaitlistStatus
    {
        Waiting,
        Notified,
        Converted,
        Expired
    }

    public class Waitlist
    {
        [Key]
        public int WaitlistId { get; set; }

        [Required]
        public int BusinessId { get; set; }
        [ForeignKey("BusinessId")]
        public virtual Business Business { get; set; }

        [Required]
        public string CustomerId { get; set; }
        [ForeignKey("CustomerId")]
        public virtual User Customer { get; set; }

        public int? ServiceVariantId { get; set; }
        public int? PreferredEmployeeId { get; set; }

        public DateOnly DesiredDate { get; set; }
        public TimeSpan? DesiredTimeFrom { get; set; }
        public TimeSpan? DesiredTimeTo { get; set; }

        public WaitlistStatus Status { get; set; } = WaitlistStatus.Waiting;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}