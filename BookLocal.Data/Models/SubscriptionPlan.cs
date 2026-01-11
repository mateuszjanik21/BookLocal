using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookLocal.Data.Models
{
    public class SubscriptionPlan
    {
        [Key]
        public int PlanId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal PriceMonthly { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal PriceYearly { get; set; }

        // Ograniczenia w pakiecie
        public int MaxEmployees { get; set; }
        public int MaxServices { get; set; }

        public bool HasAdvancedReports { get; set; } = false;
        public bool HasMarketingTools { get; set; } = false;

        public bool IsActive { get; set; } = true;
    }
}