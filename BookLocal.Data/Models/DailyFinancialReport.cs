using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookLocal.Data.Models
{
    public class DailyFinancialReport
    {
        [Key]
        public int ReportId { get; set; }

        [Required]
        public int BusinessId { get; set; }
        [ForeignKey("BusinessId")]
        public virtual Business Business { get; set; }

        [Required]
        public DateOnly ReportDate { get; set; }

        // --- FINANSE ---
        [Column(TypeName = "decimal(12, 2)")]
        public decimal TotalRevenue { get; set; }

        [Column(TypeName = "decimal(12, 2)")]
        public decimal TipsAmount { get; set; }

        [Column(TypeName = "decimal(12, 2)")]
        public decimal AverageTicketValue { get; set; }

        // --- METODY PŁATNOŚCI ---
        [Column(TypeName = "decimal(12, 2)")]
        public decimal CashRevenue { get; set; }

        [Column(TypeName = "decimal(12, 2)")]
        public decimal CardRevenue { get; set; }

        [Column(TypeName = "decimal(12, 2)")]
        public decimal OnlineRevenue { get; set; }

        // --- RUCH I KLINECI ---
        public int TotalAppointments { get; set; }
        public int CompletedAppointments { get; set; }
        public int CancelledAppointments { get; set; } 
        public int NoShowCount { get; set; }

        public int NewCustomersCount { get; set; }
        public int ReturningCustomersCount { get; set; }

        // --- STATYSTYKI ---
        public double OccupancyRate { get; set; }

        [MaxLength(200)]
        public string? TopSellingServiceName { get; set; }
    }
}