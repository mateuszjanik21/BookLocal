using BookLocal.API.DTOs;
using BookLocal.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookLocal.API.Controllers
{
    [ApiController]
    [Route("api/businesses/{businessId}/hr")]
    [Authorize(Roles = "owner")]
    public class EmployeeFinanceController : ControllerBase
    {
        private readonly AppDbContext _context;

        public EmployeeFinanceController(AppDbContext context)
        {
            _context = context;
        }


        [HttpGet("contracts")]
        public async Task<ActionResult<IEnumerable<EmploymentContractDto>>> GetContracts(int businessId)
        {
            var contracts = await _context.EmploymentContracts
                .Include(c => c.Employee)
                .Where(c => c.Employee.BusinessId == businessId)
                .Select(c => new EmploymentContractDto
                {
                    ContractId = c.ContractId,
                    EmployeeId = c.EmployeeId,
                    EmployeeName = $"{c.Employee.FirstName} {c.Employee.LastName}",
                    ContractType = c.ContractType,
                    BaseSalary = c.BaseSalary,
                    StartDate = c.StartDate,
                    EndDate = c.EndDate,
                    IsActive = c.IsActive
                })
                .ToListAsync();

            return Ok(contracts);
        }

        [HttpPost("contracts")]
        public async Task<ActionResult<EmploymentContractDto>> CreateContract(int businessId, EmploymentContractUpsertDto dto)
        {
            var exists = await _context.Employees.AnyAsync(e => e.EmployeeId == dto.EmployeeId && e.BusinessId == businessId);
            if (!exists) return BadRequest("Pracownik nie należy do tej firmy.");

            var activeContracts = await _context.EmploymentContracts
                .Where(c => c.EmployeeId == dto.EmployeeId && c.IsActive)
                .ToListAsync();

            foreach (var c in activeContracts) c.IsActive = false;

            var contract = new EmploymentContract
            {
                EmployeeId = dto.EmployeeId,
                ContractType = dto.ContractType,
                BaseSalary = dto.BaseSalary,
                TaxDeductibleExpenses = dto.TaxDeductibleExpenses,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                IsActive = true
            };

            _context.EmploymentContracts.Add(contract);
            await _context.SaveChangesAsync();

            var emp = await _context.Employees.FindAsync(dto.EmployeeId);

            return Ok(new EmploymentContractDto
            {
                ContractId = contract.ContractId,
                EmployeeId = contract.EmployeeId,
                EmployeeName = $"{emp.FirstName} {emp.LastName}",
                ContractType = contract.ContractType,
                BaseSalary = contract.BaseSalary,
                TaxDeductibleExpenses = contract.TaxDeductibleExpenses,
                StartDate = contract.StartDate,
                EndDate = contract.EndDate,
                IsActive = contract.IsActive
            });
        }


        [HttpGet("payrolls")]
        public async Task<ActionResult<IEnumerable<EmployeePayrollDto>>> GetPayrolls(int businessId, [FromQuery] int? month, [FromQuery] int? year)
        {
            var query = _context.EmployeePayrolls
                .Include(p => p.Employee)
                .Where(p => p.Employee.BusinessId == businessId);

            if (month.HasValue) query = query.Where(p => p.PeriodMonth == month.Value);
            if (year.HasValue) query = query.Where(p => p.PeriodYear == year.Value);

            var payrolls = await query.Select(p => new EmployeePayrollDto
            {
                PayrollId = p.PayrollId,
                EmployeeId = p.EmployeeId,
                EmployeeName = $"{p.Employee.FirstName} {p.Employee.LastName}",
                PeriodMonth = p.PeriodMonth,
                PeriodYear = p.PeriodYear,
                GrossAmount = p.GrossAmount,
                NetAmount = p.NetAmount,
                TotalEmployerCost = p.TotalEmployerCost,
                Status = p.Status,
                PaidAt = p.PaidAt
            }).ToListAsync();

            return Ok(payrolls);
        }

        [HttpPost("payrolls/generate")]
        public async Task<ActionResult<EmployeePayrollDto>> GeneratePayroll(int businessId, GeneratePayrollDto dto)
        {
            var requestedDate = new DateOnly(dto.Year, dto.Month, 1);
            var nextMonth = requestedDate.AddMonths(1);

            var contract = await _context.EmploymentContracts
               .Where(c => c.EmployeeId == dto.EmployeeId && c.IsActive)
               .OrderByDescending(c => c.StartDate)
               .FirstOrDefaultAsync();

            if (contract == null || contract.StartDate >= nextMonth)
            {
                return BadRequest("Brak aktywnej umowy w wybranym okresie.");
            }

            var employee = await _context.Employees
                .Include(e => e.FinanceSettings)
                .FirstOrDefaultAsync(e => e.EmployeeId == dto.EmployeeId);

            if (employee == null) return NotFound("Pracownik nie istnieje.");

            decimal gross = contract.BaseSalary;
            decimal net = 0;
            decimal employerCost = 0;

            decimal socialSecurity = 0;
            decimal healthInsurance = 0;
            decimal incomeTax = 0;
            decimal employerZus = 0;

            var age = 0;
            if (employee.DateOfBirth != DateOnly.MinValue)
            {
                age = DateTime.Now.Year - employee.DateOfBirth.Year;
                if (DateTime.Now.DayOfYear < employee.DateOfBirth.DayOfYear) age--;
            }
            bool isUnder26 = age < 26;
            bool isStudent = employee.FinanceSettings?.IsStudent ?? false;

            switch (contract.ContractType)
            {
                case ContractType.EmploymentContract:
                    decimal zusRate = 0.1371m;
                    decimal healthRate = 0.09m;
                    decimal taxRate = isUnder26 ? 0.0m : 0.17m;

                    socialSecurity = Math.Round(gross * zusRate, 2);
                    healthInsurance = Math.Round((gross - socialSecurity) * healthRate, 2);

                    if (!isUnder26)
                    {
                        decimal taxBase = Math.Round(gross - socialSecurity - contract.TaxDeductibleExpenses, 0);
                        incomeTax = Math.Round(taxBase * taxRate, 2) - 300.00m;
                        if (incomeTax < 0) incomeTax = 0;
                    }

                    net = gross - socialSecurity - incomeTax - healthInsurance;
                    employerZus = Math.Round(gross * 0.2048m, 2);
                    employerCost = gross + employerZus;
                    break;

                case ContractType.MandateContract:
                    if (isStudent && isUnder26)
                    {
                        net = gross;
                        employerCost = gross;
                    }
                    else
                    {
                        decimal zusRateUz = 0.1126m;
                        zusRateUz = 0.1371m;

                        socialSecurity = Math.Round(gross * zusRateUz, 2);
                        healthInsurance = Math.Round((gross - socialSecurity) * 0.09m, 2);

                        decimal taxRateUz = isUnder26 ? 0.0m : 0.12m;
                        decimal taxBaseUz = Math.Round(gross - socialSecurity - (gross * 0.20m), 0); 

                        if (!isUnder26)
                        {
                            incomeTax = Math.Round(taxBaseUz * 0.12m, 2);
                        }

                        net = gross - socialSecurity - healthInsurance - incomeTax;
                        employerZus = Math.Round(gross * 0.2048m, 2);
                        employerCost = gross + employerZus;
                    }
                    break;

                case ContractType.B2B:
                    net = gross;
                    employerCost = gross; 
                    break;

                default:
                    net = gross * 0.7m;
                    employerCost = gross * 1.2m;
                    break;
            }

            if (net < 0) net = 0;

            var payroll = new EmployeePayroll
            {
                EmployeeId = dto.EmployeeId,
                PeriodMonth = dto.Month,
                PeriodYear = dto.Year,
                BaseSalaryComponent = contract.BaseSalary,
                CommissionComponent = 0,
                BonusComponent = 0,
                GrossAmount = gross,
                SocialSecurityTax = socialSecurity,
                HealthInsuranceTax = healthInsurance,
                IncomeTaxAdvance = incomeTax,
                OtherDeductions = 0,
                NetAmount = net,
                EmployerSocialSecurityTax = employerZus,
                EmployerPPKContribution = 0,
                TotalEmployerCost = employerCost,
                Status = PayrollStatus.Draft,
                GeneratedAt = DateOnly.FromDateTime(DateTime.Now)
            };

            _context.EmployeePayrolls.Add(payroll);
            await _context.SaveChangesAsync();

            var emp = await _context.Employees.FindAsync(dto.EmployeeId);

            return Ok(new EmployeePayrollDto
            {
                PayrollId = payroll.PayrollId,
                EmployeeId = payroll.EmployeeId,
                EmployeeName = $"{emp.FirstName} {emp.LastName}",
                PeriodMonth = payroll.PeriodMonth,
                PeriodYear = payroll.PeriodYear,
                GrossAmount = payroll.GrossAmount,
                NetAmount = payroll.NetAmount,
                TotalEmployerCost = payroll.TotalEmployerCost,
                Status = payroll.Status
            });
        }
    }
}
