using System.ComponentModel.DataAnnotations;

namespace BookLocal.API.DTOs
{
    public class SubscriptionPlanDto
    {
        public int PlanId { get; set; }
        public string Name { get; set; }
        public decimal PriceMonthly { get; set; }
        public decimal PriceYearly { get; set; }
        public int MaxEmployees { get; set; }
        public int MaxServices { get; set; }
        public bool HasAdvancedReports { get; set; }
        public bool HasMarketingTools { get; set; }
        public decimal CommissionPercentage { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateSubscriptionPlanDto
    {
        [Required]
        public string Name { get; set; }

        [Range(0, double.MaxValue)]
        public decimal PriceMonthly { get; set; }

        [Range(0, double.MaxValue)]
        public decimal PriceYearly { get; set; }

        public int MaxEmployees { get; set; }
        public int MaxServices { get; set; }
        public bool HasAdvancedReports { get; set; }
        public bool HasMarketingTools { get; set; }

        [Range(0, 100)]
        public decimal CommissionPercentage { get; set; }
    }
}
