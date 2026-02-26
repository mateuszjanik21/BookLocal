using BookLocal.API.DTOs;
using BookLocal.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BookLocal.API.Controllers
{
    [ApiController]
    [Route("api/businesses/{businessId}/hr")]
    [Authorize(Roles = "owner")]
    public class EmployeeFinanceController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;

        public EmployeeFinanceController(AppDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet("employees")]
        public async Task<ActionResult<IEnumerable<EmployeeDto>>> GetEmployeesForHr(int businessId)
        {
            var business = await _context.Businesses.FindAsync(businessId);
            if (business == null) return NotFound("Firma nie istnieje.");

            var owner = await _userManager.FindByIdAsync(business.OwnerId);
            string ownerFirst = owner?.FirstName ?? string.Empty;
            string ownerLast = owner?.LastName ?? string.Empty;

            var employees = await _context.Employees
                .Include(e => e.EmployeeDetails)
                .Include(e => e.FinanceSettings)
                .Where(e => e.BusinessId == businessId
                         && !e.IsArchived
                         && !(e.FirstName == ownerFirst && e.LastName == ownerLast))
                .Select(e => new EmployeeDto
                {
                    Id = e.EmployeeId,
                    FirstName = e.FirstName,
                    LastName = e.LastName,
                    Position = e.Position,
                    PhotoUrl = e.PhotoUrl,
                    DateOfBirth = e.DateOfBirth,
                    Specialization = e.EmployeeDetails != null ? e.EmployeeDetails.Specialization : null,
                    InstagramProfileUrl = e.EmployeeDetails != null ? e.EmployeeDetails.InstagramProfileUrl : null,
                    PortfolioUrl = e.EmployeeDetails != null ? e.EmployeeDetails.PortfolioUrl : null,
                    IsStudent = e.FinanceSettings != null ? e.FinanceSettings.IsStudent : false,
                    IsArchived = e.IsArchived
                })
                .ToListAsync();

            return Ok(employees);
        }

        [HttpGet("contracts")]
        public async Task<ActionResult<IEnumerable<EmploymentContractDto>>> GetContracts(int businessId)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);

            var expired = await _context.EmploymentContracts
                .Include(c => c.Employee)
                .Where(c => c.Employee.BusinessId == businessId
                         && c.IsActive
                         && c.EndDate.HasValue
                         && c.EndDate.Value < today)
                .ToListAsync();

            if (expired.Any())
            {
                foreach (var ec in expired) ec.IsActive = false;
                await _context.SaveChangesAsync();
            }

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
            var emp = await _context.Employees
                .Include(e => e.FinanceSettings)
                .FirstOrDefaultAsync(e => e.EmployeeId == dto.EmployeeId && e.BusinessId == businessId);
            if (emp == null) return BadRequest("Pracownik nie należy do tej firmy.");

            if (dto.ContractType != ContractType.Apprenticeship && dto.BaseSalary <= 0)
                return BadRequest("Wynagrodzenie brutto musi być większe niż 0 dla tego rodzaju umowy.");

            if (dto.EndDate.HasValue && dto.EndDate.Value <= dto.StartDate)
                return BadRequest("Data zakończenia musi być późniejsza niż data rozpoczęcia.");

            var activeContracts = await _context.EmploymentContracts
                .Where(c => c.EmployeeId == dto.EmployeeId && c.IsActive)
                .ToListAsync();

            var newStartDate = dto.StartDate;
            foreach (var c in activeContracts)
            {
                c.IsActive = false;
                if (!c.EndDate.HasValue || c.EndDate.Value >= newStartDate)
                {
                    c.EndDate = newStartDate.AddDays(-1);
                }
            }

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

            if (dto.IsStudent.HasValue)
            {
                if (emp.FinanceSettings == null)
                {
                    emp.FinanceSettings = new EmployeeFinanceSettings { EmployeeId = dto.EmployeeId };
                    _context.EmployeeFinanceSettings.Add(emp.FinanceSettings);
                }
                emp.FinanceSettings.IsStudent = dto.IsStudent.Value;
            }

            await _context.SaveChangesAsync();

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

        [HttpPatch("contracts/{contractId}/archive")]
        public async Task<IActionResult> ArchiveContract(int businessId, int contractId)
        {
            var contract = await _context.EmploymentContracts
                .Include(c => c.Employee)
                .FirstOrDefaultAsync(c => c.ContractId == contractId && c.Employee.BusinessId == businessId);

            if (contract == null) return NotFound("Umowa nie istnieje.");
            if (!contract.IsActive) return BadRequest("Umowa jest już zarchiwizowana.");

            contract.IsActive = false;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPut("contracts/{contractId}")]
        public async Task<ActionResult<EmploymentContractDto>> UpdateContract(int businessId, int contractId, EmploymentContractUpsertDto dto)
        {
            var contract = await _context.EmploymentContracts
                .Include(c => c.Employee)
                .FirstOrDefaultAsync(c => c.ContractId == contractId && c.Employee.BusinessId == businessId);

            if (contract == null) return NotFound("Umowa nie istnieje.");

            if (dto.ContractType != ContractType.Apprenticeship && dto.BaseSalary <= 0)
                return BadRequest("Wynagrodzenie brutto musi być większe niż 0 dla tego rodzaju umowy.");

            if (dto.EndDate.HasValue && dto.EndDate.Value <= dto.StartDate)
                return BadRequest("Data zakończenia musi być późniejsza niż data rozpoczęcia.");

            contract.ContractType = dto.ContractType;
            contract.BaseSalary = dto.BaseSalary;
            contract.TaxDeductibleExpenses = dto.TaxDeductibleExpenses;
            contract.StartDate = dto.StartDate;
            contract.EndDate = dto.EndDate;

            if (dto.IsStudent.HasValue)
            {
                var financeSettings = await _context.EmployeeFinanceSettings.FirstOrDefaultAsync(fs => fs.EmployeeId == contract.EmployeeId);
                if (financeSettings == null)
                {
                    financeSettings = new EmployeeFinanceSettings { EmployeeId = contract.EmployeeId };
                    _context.EmployeeFinanceSettings.Add(financeSettings);
                }
                financeSettings.IsStudent = dto.IsStudent.Value;
            }

            await _context.SaveChangesAsync();

            return Ok(new EmploymentContractDto
            {
                ContractId = contract.ContractId,
                EmployeeId = contract.EmployeeId,
                EmployeeName = $"{contract.Employee.FirstName} {contract.Employee.LastName}",
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
            var existingPayroll = await _context.EmployeePayrolls
                .FirstOrDefaultAsync(p => p.EmployeeId == dto.EmployeeId && p.PeriodMonth == dto.Month && p.PeriodYear == dto.Year);

            if (existingPayroll != null)
            {
                return BadRequest("Płaca za ten miesiąc została już wygenerowana.");
            }

            var serverNow = DateTime.Now;
            if (dto.Year > serverNow.Year || (dto.Year == serverNow.Year && dto.Month > serverNow.Month))
            {
                return BadRequest("Nie można generować list płac za przyszłe miesiące.");
            }

            if (dto.Day.HasValue && (dto.Day.Value < 1 || dto.Day.Value > DateTime.DaysInMonth(dto.Year, dto.Month)))
            {
                return BadRequest("Nieprawidłowy dzień miesiąca.");
            }

            var startOfPeriod = new DateOnly(dto.Year, dto.Month, 1);
            var endOfPeriod = new DateOnly(dto.Year, dto.Month, dto.Day ?? DateTime.DaysInMonth(dto.Year, dto.Month));

            var contracts = await _context.EmploymentContracts
               .Where(c => c.EmployeeId == dto.EmployeeId && c.StartDate <= endOfPeriod && (c.EndDate == null || c.EndDate >= startOfPeriod))
               .OrderByDescending(c => c.StartDate)
               .ToListAsync();

            if (!contracts.Any())
            {
                return BadRequest("Brak umowy obejmującej wybrany okres.");
            }

            var employee = await _context.Employees
                .Include(e => e.FinanceSettings)
                .FirstOrDefaultAsync(e => e.EmployeeId == dto.EmployeeId);

            if (employee == null) return NotFound("Pracownik nie istnieje.");

            var totalDaysInMonth = DateTime.DaysInMonth(dto.Year, dto.Month);

            decimal totalGross = 0;
            decimal totalNet = 0;
            decimal totalEmployerCost = 0;
            decimal totalSocialSecurity = 0;
            decimal totalHealthInsurance = 0;
            decimal totalIncomeTax = 0;
            decimal totalEmployerZus = 0;

            var age = 0;
            if (employee.DateOfBirth != DateOnly.MinValue)
            {
                age = DateTime.Now.Year - employee.DateOfBirth.Year;
                if (DateTime.Now.DayOfYear < employee.DateOfBirth.DayOfYear) age--;
            }
            bool isUnder26 = age < 26;
            bool isStudent = employee.FinanceSettings?.IsStudent ?? false;

            foreach (var c in contracts)
            {
                var activeStart = c.StartDate > startOfPeriod ? c.StartDate : startOfPeriod;
                var activeEnd = c.EndDate.HasValue && c.EndDate.Value < endOfPeriod ? c.EndDate.Value : endOfPeriod;
                var activeDays = (activeEnd.DayNumber - activeStart.DayNumber) + 1;

                if (activeDays <= 0) continue;

                decimal contractGross = Math.Round(c.BaseSalary * (activeDays / (decimal)totalDaysInMonth), 2);
                totalGross += contractGross;

                decimal contractNet = 0;
                decimal contractEmployerCost = 0;
                decimal contractSocialSecurity = 0;
                decimal contractHealthInsurance = 0;
                decimal contractIncomeTax = 0;
                decimal contractEmployerZus = 0;

                switch (c.ContractType)
                {
                    case ContractType.EmploymentContract:
                        decimal zusRate = 0.1371m;
                        decimal healthRate = 0.09m;
                        decimal taxRate = isUnder26 ? 0.0m : 0.17m;

                        contractSocialSecurity = Math.Round(contractGross * zusRate, 2);
                        contractHealthInsurance = Math.Round((contractGross - contractSocialSecurity) * healthRate, 2);

                        if (!isUnder26)
                        {
                            decimal taxBase = Math.Round(contractGross - contractSocialSecurity - c.TaxDeductibleExpenses, 0);
                            contractIncomeTax = Math.Round(taxBase * taxRate, 2) - 300.00m;
                            if (contractIncomeTax < 0) contractIncomeTax = 0;
                        }

                        contractNet = contractGross - contractSocialSecurity - contractIncomeTax - contractHealthInsurance;
                        contractEmployerZus = Math.Round(contractGross * 0.2048m, 2);
                        contractEmployerCost = contractGross + contractEmployerZus;
                        break;

                    case ContractType.MandateContract:
                        if (isStudent && isUnder26)
                        {
                            contractNet = contractGross;
                            contractEmployerCost = contractGross;
                        }
                        else
                        {
                            decimal zusRateUz = 0.1371m;

                            contractSocialSecurity = Math.Round(contractGross * zusRateUz, 2);
                            contractHealthInsurance = Math.Round((contractGross - contractSocialSecurity) * 0.09m, 2);

                            decimal taxRateUz = isUnder26 ? 0.0m : 0.12m;
                            decimal taxBaseUz = Math.Round(contractGross - contractSocialSecurity - (contractGross * 0.20m), 0);

                            if (!isUnder26)
                            {
                                contractIncomeTax = Math.Round(taxBaseUz * 0.12m, 2);
                            }

                            contractNet = contractGross - contractSocialSecurity - contractHealthInsurance - contractIncomeTax;
                            contractEmployerZus = Math.Round(contractGross * 0.2048m, 2);
                            contractEmployerCost = contractGross + contractEmployerZus;
                        }
                        break;

                    case ContractType.B2B:
                        contractNet = contractGross;
                        contractEmployerCost = contractGross;
                        break;

                    default:
                        contractNet = contractGross * 0.7m;
                        contractEmployerCost = contractGross * 1.2m;
                        break;
                }

                if (contractNet < 0) contractNet = 0;

                totalNet += contractNet;
                totalEmployerCost += contractEmployerCost;
                totalSocialSecurity += contractSocialSecurity;
                totalHealthInsurance += contractHealthInsurance;
                totalIncomeTax += contractIncomeTax;
                totalEmployerZus += contractEmployerZus;
            }

            var latestContract = contracts.First();

            var payroll = new EmployeePayroll
            {
                EmployeeId = dto.EmployeeId,
                PeriodMonth = dto.Month,
                PeriodYear = dto.Year,
                BaseSalaryComponent = latestContract.BaseSalary,
                CommissionComponent = 0,
                BonusComponent = 0,
                GrossAmount = totalGross,
                SocialSecurityTax = totalSocialSecurity,
                HealthInsuranceTax = totalHealthInsurance,
                IncomeTaxAdvance = totalIncomeTax,
                OtherDeductions = 0,
                NetAmount = totalNet,
                EmployerSocialSecurityTax = totalEmployerZus,
                EmployerPPKContribution = 0,
                TotalEmployerCost = totalEmployerCost,
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

        [HttpDelete("payrolls/{payrollId}")]
        public async Task<IActionResult> DeletePayroll(int businessId, int payrollId)
        {
            var payroll = await _context.EmployeePayrolls
                .Include(p => p.Employee)
                .FirstOrDefaultAsync(p => p.PayrollId == payrollId && p.Employee.BusinessId == businessId);

            if (payroll == null) return NotFound("Płaca nie istnieje.");

            _context.EmployeePayrolls.Remove(payroll);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
