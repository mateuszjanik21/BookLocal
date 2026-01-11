using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookLocal.Data.Models
{
    public enum LoyaltyTransactionType
    {
        Earned,
        Redeemed,
        ManualAdjustment
    }

    public class LoyaltyTransaction
    {
        [Key]
        public int TransactionId { get; set; }

        public int LoyaltyPointId { get; set; }
        [ForeignKey("LoyaltyPointId")]
        public virtual LoyaltyPoint LoyaltyPoint { get; set; }

        public int PointsAmount { get; set; } 
        public LoyaltyTransactionType Type { get; set; }

        public string Description { get; set; }
        public int? ReservationId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
