using BookLocal.API.DTOs;
using BookLocal.Data.Models;
using System.Security.Claims;

namespace BookLocal.API.Interfaces
{
    public interface IFinanceService
    {
        Task<(bool Success, IEnumerable<FinanceReportSqlDto>? Data, string? ErrorMessage)> GetLiveReportsAsync(int businessId, DateOnly startDate, DateOnly endDate, ClaimsPrincipal user);
        Task<(bool Success, string? Message, string? ErrorMessage)> GenerateReportRangeAsync(int businessId, DateOnly startDate, DateOnly endDate, ClaimsPrincipal user);
        Task<(bool Success, string? Message, string? ErrorMessage)> DeleteReportAsync(int businessId, DateOnly date, ClaimsPrincipal user);
        Task<(bool Success, DailyFinancialReport? Data, string? ErrorMessage)> GenerateDailyReportAsync(int businessId, DateOnly date, ClaimsPrincipal user);
        Task<(bool Success, IEnumerable<DailyFinancialReport>? Data, string? ErrorMessage)> GetReportsAsync(int businessId, int month, int year, ClaimsPrincipal user);
        Task<(bool Success, IEnumerable<DailyEmployeePerformanceDto>? Data, string? ErrorMessage)> GetEmployeePerformanceAsync(int businessId, DateOnly? date, DateOnly? startDate, DateOnly? endDate, ClaimsPrincipal user);
    }
}
