using BookLocal.API.DTOs;
using BookLocal.API.Interfaces;
using BookLocal.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookLocal.API.Controllers
{
    [ApiController]
    [Route("api/businesses/{businessId}/finance")]
    [Authorize(Roles = "owner")]
    public class FinanceController : ControllerBase
    {
        private readonly IFinanceService _financeService;

        public FinanceController(IFinanceService financeService)
        {
            _financeService = financeService;
        }

        [HttpGet("reports-live")]
        public async Task<ActionResult<IEnumerable<FinanceReportSqlDto>>> GetLiveReports(
            int businessId,
            [FromQuery] DateOnly startDate,
            [FromQuery] DateOnly endDate)
        {
            var result = await _financeService.GetLiveReportsAsync(businessId, startDate, endDate, User);

            if (!result.Success) return Forbid();

            return Ok(result.Data);
        }

        [HttpPost("generate-range")]
        public async Task<ActionResult> GenerateReportRange(int businessId, [FromQuery] DateOnly startDate, [FromQuery] DateOnly endDate)
        {
            var result = await _financeService.GenerateReportRangeAsync(businessId, startDate, endDate, User);

            if (!result.Success)
            {
                if (result.ErrorMessage == "Brak uprawnień.") return Forbid();
                return BadRequest(result.ErrorMessage);
            }

            return Ok(new { message = result.Message });
        }

        [HttpDelete("report")]
        public async Task<ActionResult> DeleteReport(int businessId, [FromQuery] DateOnly date)
        {
            var result = await _financeService.DeleteReportAsync(businessId, date, User);

            if (!result.Success)
            {
                if (result.ErrorMessage == "Brak uprawnień.") return Forbid();
                return NotFound(result.ErrorMessage);
            }

            return Ok(new { message = result.Message });
        }

        [HttpPost("generate-daily-report")]
        public async Task<ActionResult<DailyFinancialReport>> GenerateDailyReport(int businessId, [FromQuery] DateOnly date)
        {
            var result = await _financeService.GenerateDailyReportAsync(businessId, date, User);

            if (!result.Success) return Forbid();

            return Ok(result.Data);
        }

        [HttpGet("reports")]
        public async Task<ActionResult<IEnumerable<DailyFinancialReport>>> GetReports(int businessId, [FromQuery] int month, [FromQuery] int year)
        {
            var result = await _financeService.GetReportsAsync(businessId, month, year, User);

            if (!result.Success) return Forbid();

            return Ok(result.Data);
        }

        [HttpGet("employee-performance")]
        public async Task<ActionResult<IEnumerable<DailyEmployeePerformanceDto>>> GetEmployeePerformance(
            int businessId,
            [FromQuery] DateOnly? date,
            [FromQuery] DateOnly? startDate,
            [FromQuery] DateOnly? endDate)
        {
            var result = await _financeService.GetEmployeePerformanceAsync(businessId, date, startDate, endDate, User);

            if (!result.Success)
            {
                if (result.ErrorMessage == "Brak uprawnień.") return Forbid();
                return BadRequest(result.ErrorMessage);
            }

            return Ok(result.Data);
        }
    }
}
