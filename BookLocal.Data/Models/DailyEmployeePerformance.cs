using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookLocal.Data.Models
{
    public class DailyEmployeePerformance
    {
        [Key]
        public int PerformanceId { get; set; }

        [Required]
        public int EmployeeId { get; set; }
        [ForeignKey("EmployeeId")]
        public virtual Employee Employee { get; set; }

        [Required]
        public DateOnly ReportDate { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal TotalRevenueGenerated { get; set; }

        public int ClientsServed { get; set; }

        public int ServicesPerformed { get; set; }

        public int MinutesWorked { get; set; }
        public int MinutesIdle { get; set; }

        public double AverageRating { get; set; }
    }
}