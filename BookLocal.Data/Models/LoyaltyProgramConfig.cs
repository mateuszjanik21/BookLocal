using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookLocal.Data.Models
{
    public class LoyaltyProgramConfig
    {
        [Key]
        public int ConfigId { get; set; }

        [Required]
        public int BusinessId { get; set; }
        [ForeignKey("BusinessId")]
        public virtual Business Business { get; set; }

        public bool IsActive { get; set; } = false;

        [Column(TypeName = "decimal(18,2)")]
        public decimal SpendAmountForOnePoint { get; set; } = 10.00m;

    }
}
