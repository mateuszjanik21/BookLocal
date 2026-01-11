using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookLocal.Data.Models
{
    public enum PayrollStatus
    {
        Draft,
        Calculated,
        Approved,
        Paid
    }

    public class EmployeePayroll
    {
        [Key]
        public int PayrollId { get; set; }

        [Required]
        public int EmployeeId { get; set; }
        [ForeignKey("EmployeeId")]
        public virtual Employee Employee { get; set; }

        [Required]
        public int PeriodMonth { get; set; }
        [Required]
        public int PeriodYear { get; set; }

        // --- 1. PRZYCHÓD (Brutto) ---
        // Suma: Podstawa + Prowizje + Premie
        [Column(TypeName = "decimal(10, 2)")]
        public decimal GrossAmount { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal BaseSalaryComponent { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal CommissionComponent { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal BonusComponent { get; set; }

        // --- 2. POTRĄCENIA PRACOWNIKA ---
        // ZUS
        [Column(TypeName = "decimal(10, 2)")]
        public decimal SocialSecurityTax { get; set; }

        // NFZ
        [Column(TypeName = "decimal(10, 2)")]
        public decimal HealthInsuranceTax { get; set; }

        // PIT
        [Column(TypeName = "decimal(10, 2)")]
        public decimal IncomeTaxAdvance { get; set; }

        // Inne potrącenia
        [Column(TypeName = "decimal(10, 2)")]
        public decimal OtherDeductions { get; set; }

        // --- 3. Netto ---
        [Column(TypeName = "decimal(10, 2)")]
        public decimal NetAmount { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal EmployerSocialSecurityTax { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal EmployerPPKContribution { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal TotalEmployerCost { get; set; }

        public PayrollStatus Status { get; set; } = PayrollStatus.Draft;
        public DateOnly? GeneratedAt { get; set; }
        public DateOnly? PaidAt { get; set; }
    }
}