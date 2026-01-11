using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookLocal.Data.Models
{
    public enum ContractType
    {
        EmploymentContract,
        B2B,
        MandateContract,
        Apprenticeship
    }

    public class EmploymentContract
    {
        [Key]
        public int ContractId { get; set; }

        [Required]
        public int EmployeeId { get; set; }
        [ForeignKey("EmployeeId")]
        public virtual Employee Employee { get; set; }

        [Required]
        public ContractType ContractType { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal BaseSalary { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal TaxDeductibleExpenses { get; set; } = 250.00m;
        public DateOnly StartDate { get; set; }
        public DateOnly? EndDate { get; set; }

        public bool IsActive { get; set; } = true;
    }
}