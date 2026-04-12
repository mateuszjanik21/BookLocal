using BookLocal.API.DTOs;
using System.Security.Claims;

namespace BookLocal.API.Interfaces
{
    public interface IEmployeeFinanceService
    {
        Task<(bool Success, IEnumerable<EmployeeDto>? Data, string? ErrorMessage)> GetEmployeesForHrAsync(int businessId, ClaimsPrincipal user);
        Task<(bool Success, IEnumerable<EmploymentContractDto>? Data, string? ErrorMessage)> GetContractsAsync(int businessId, ClaimsPrincipal user);
        Task<(bool Success, EmploymentContractDto? Data, string? ErrorMessage)> CreateContractAsync(int businessId, EmploymentContractUpsertDto dto, ClaimsPrincipal user);
        Task<(bool Success, string? ErrorMessage)> ArchiveContractAsync(int businessId, int contractId, ClaimsPrincipal user);
        Task<(bool Success, EmploymentContractDto? Data, string? ErrorMessage)> UpdateContractAsync(int businessId, int contractId, EmploymentContractUpsertDto dto, ClaimsPrincipal user);
        Task<(bool Success, IEnumerable<EmployeePayrollDto>? Data, string? ErrorMessage)> GetPayrollsAsync(int businessId, int? month, int? year, ClaimsPrincipal user);
        Task<(bool Success, EmployeePayrollDto? Data, string? ErrorMessage)> GeneratePayrollAsync(int businessId, GeneratePayrollDto dto, ClaimsPrincipal user);
        Task<(bool Success, string? ErrorMessage)> DeletePayrollAsync(int businessId, int payrollId, ClaimsPrincipal user);
        Task<(bool Success, IEnumerable<HrMonthlySummaryDto>? Data, string? ErrorMessage)> GetMonthlySummaryAsync(int businessId, int endMonth, int endYear, int count, ClaimsPrincipal user);
    }
}
