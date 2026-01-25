using BookLocal.Data.Models;

namespace BookLocal.API.DTOs
{
    public class EmploymentContractDto
    {
        public int ContractId { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public ContractType ContractType { get; set; }
        public decimal BaseSalary { get; set; }
        public decimal TaxDeductibleExpenses { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public bool IsActive { get; set; }
    }

    public class EmploymentContractUpsertDto
    {
        public int EmployeeId { get; set; }
        public ContractType ContractType { get; set; }
        public decimal BaseSalary { get; set; }
        public decimal TaxDeductibleExpenses { get; set; } = 250.00m;
        public DateOnly StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
    }

    public class EmployeePayrollDto
    {
        public int PayrollId { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public int PeriodMonth { get; set; }
        public int PeriodYear { get; set; }
        public decimal GrossAmount { get; set; }
        public decimal NetAmount { get; set; }
        public decimal TotalEmployerCost { get; set; }
        public PayrollStatus Status { get; set; }
        public DateOnly? PaidAt { get; set; }
    }

    public class GeneratePayrollDto
    {
        public int EmployeeId { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
    }
}
