using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookLocal.Data.Models
{
    public enum DiscountType
    {
        Percentage,
        FixedAmount
    }

    public class Discount
    {
        [Key]
        public int DiscountId { get; set; }

        [Required]
        public int BusinessId { get; set; }
        [ForeignKey("BusinessId")]
        public virtual Business Business { get; set; }

        [Required]
        [MaxLength(20)]
        public string Code { get; set; } 

        public DiscountType Type { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal Value { get; set; }

        public int? MaxUses { get; set; }
        public int UsedCount { get; set; } = 0;

        public DateOnly? ValidFrom { get; set; }
        public DateOnly? ValidTo { get; set; }

        public bool IsActive { get; set; } = true;

        public int? ServiceId { get; set; }
    }
}