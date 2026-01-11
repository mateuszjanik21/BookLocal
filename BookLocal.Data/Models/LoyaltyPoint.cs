using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookLocal.Data.Models
{
    public class LoyaltyPoint
    {
        [Key]
        public int LoyaltyId { get; set; }

        [Required]
        public int BusinessId { get; set; }
        [ForeignKey("BusinessId")]
        public virtual Business Business { get; set; }

        [Required]
        public string CustomerId { get; set; }
        [ForeignKey("CustomerId")]
        public virtual User Customer { get; set; }

        public int PointsBalance { get; set; } = 0;
        public int TotalPointsEarned { get; set; } = 0;

        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }
}