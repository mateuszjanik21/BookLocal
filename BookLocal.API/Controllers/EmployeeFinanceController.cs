using BookLocal.API.DTOs;
using BookLocal.API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookLocal.API.Controllers
{
    [ApiController]
    [Route("api/businesses/{businessId}/hr")]
    [Authorize(Roles = "owner")]
    public class EmployeeFinanceController : ControllerBase
    {
        private readonly IEmployeeFinanceService _employeeFinanceService;

        public EmployeeFinanceController(IEmployeeFinanceService employeeFinanceService)
        {
            _employeeFinanceService = employeeFinanceService;
        }

        [HttpGet("employees")]
        public async Task<ActionResult<IEnumerable<EmployeeDto>>> GetEmployeesForHr(int businessId)
        {
            var result = await _employeeFinanceService.GetEmployeesForHrAsync(businessId, User);

            if (!result.Success) return Forbid();

            return Ok(result.Data);
        }

        [HttpGet("contracts")]
        public async Task<ActionResult<IEnumerable<EmploymentContractDto>>> GetContracts(int businessId)
        {
            var result = await _employeeFinanceService.GetContractsAsync(businessId, User);

            if (!result.Success) return Forbid();

            return Ok(result.Data);
        }

        [HttpPost("contracts")]
        public async Task<ActionResult<EmploymentContractDto>> CreateContract(int businessId, EmploymentContractUpsertDto dto)
        {
            var result = await _employeeFinanceService.CreateContractAsync(businessId, dto, User);

            if (!result.Success)
            {
                if (result.ErrorMessage == "Brak dostępu.") return Forbid();
                return BadRequest(result.ErrorMessage);
            }

            return Ok(result.Data);
        }

        [HttpPatch("contracts/{contractId}/archive")]
        public async Task<IActionResult> ArchiveContract(int businessId, int contractId)
        {
            var result = await _employeeFinanceService.ArchiveContractAsync(businessId, contractId, User);

            if (!result.Success)
            {
                if (result.ErrorMessage == "Brak dostępu.") return Forbid();
                if (result.ErrorMessage == "Umowa nie istnieje.") return NotFound(result.ErrorMessage);
                return BadRequest(result.ErrorMessage);
            }

            return NoContent();
        }

        [HttpPut("contracts/{contractId}")]
        public async Task<ActionResult<EmploymentContractDto>> UpdateContract(int businessId, int contractId, EmploymentContractUpsertDto dto)
        {
            var result = await _employeeFinanceService.UpdateContractAsync(businessId, contractId, dto, User);

            if (!result.Success)
            {
                if (result.ErrorMessage == "Brak dostępu.") return Forbid();
                if (result.ErrorMessage == "Umowa nie istnieje.") return NotFound(result.ErrorMessage);
                return BadRequest(result.ErrorMessage);
            }

            return Ok(result.Data);
        }

        [HttpGet("payrolls")]
        public async Task<ActionResult<IEnumerable<EmployeePayrollDto>>> GetPayrolls(int businessId, [FromQuery] int? month, [FromQuery] int? year)
        {
            var result = await _employeeFinanceService.GetPayrollsAsync(businessId, month, year, User);

            if (!result.Success) return Forbid();

            return Ok(result.Data);
        }

        [HttpPost("payrolls/generate")]
        public async Task<ActionResult<EmployeePayrollDto>> GeneratePayroll(int businessId, GeneratePayrollDto dto)
        {
            var result = await _employeeFinanceService.GeneratePayrollAsync(businessId, dto, User);

            if (!result.Success)
            {
                if (result.ErrorMessage == "Brak dostępu.") return Forbid();
                if (result.ErrorMessage == "Pracownik nie istnieje.") return NotFound(result.ErrorMessage);
                return BadRequest(result.ErrorMessage);
            }

            if (result.Data == null && result.ErrorMessage == "Payroll already generated")
            {
                return NoContent();
            }

            return Ok(result.Data);
        }

        [HttpDelete("payrolls/{payrollId}")]
        public async Task<IActionResult> DeletePayroll(int businessId, int payrollId)
        {
            var result = await _employeeFinanceService.DeletePayrollAsync(businessId, payrollId, User);

            if (!result.Success)
            {
                if (result.ErrorMessage == "Brak dostępu.") return Forbid();
                return NotFound(result.ErrorMessage);
            }

            return NoContent();
        }

        [HttpGet("monthly-summary")]
        public async Task<ActionResult<IEnumerable<HrMonthlySummaryDto>>> GetMonthlySummary(
            int businessId,
            [FromQuery] int endMonth,
            [FromQuery] int endYear,
            [FromQuery] int count = 6)
        {
            var result = await _employeeFinanceService.GetMonthlySummaryAsync(businessId, endMonth, endYear, count, User);

            if (!result.Success) return Forbid();

            return Ok(result.Data);
        }
    }
}
