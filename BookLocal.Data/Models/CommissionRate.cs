using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookLocal.Data.Models
{
    public class CommissionRate
    {
        [Key]
        public int CommissionRateId { get; set; }

        [Required]
        public int EmployeeId { get; set; }
        [ForeignKey("EmployeeId")]
        public virtual Employee Employee { get; set; }

        public int? ServiceCategoryId { get; set; }
        [ForeignKey("ServiceCategoryId")]
        public virtual ServiceCategory? ServiceCategory { get; set; }

        [Required]
        [Range(0, 100)]
        public double Percentage { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal ThresholdAmount { get; set; } = 0;
    }
}