using BookLocal.Data.Models;

namespace BookLocal.API.DTOs
{
    public class AdminBusinessListDto
    {
        public int BusinessId { get; set; }
        public string Name { get; set; }
        public string OwnerEmail { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsVerified { get; set; }
        public string VerificationStatus { get; set; }
        public string SubscriptionPlanName { get; set; }
    }

    public class VerifyBusinessDto
    {
        public bool IsApproved { get; set; }
        public string? RejectionReason { get; set; }
    }
}
