using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookLocal.Data.Models
{
    public class BusinessSubscription
    {
        [Key]
        public int SubscriptionId { get; set; }

        [Required]
        public int BusinessId { get; set; }
        [ForeignKey("BusinessId")]
        public virtual Business Business { get; set; }

        [Required]
        public int PlanId { get; set; }
        [ForeignKey("PlanId")]
        public virtual SubscriptionPlan Plan { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public bool IsActive { get; set; } = true;
        public bool IsAutoRenew { get; set; } = true;

        public string? ExternalSubscriptionId { get; set; }
    }
}