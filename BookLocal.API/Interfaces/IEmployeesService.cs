using BookLocal.API.DTOs;
using System.Security.Claims;

namespace BookLocal.API.Interfaces
{
    public interface IEmployeesService
    {
        Task<(bool Success, IEnumerable<EmployeeDto>? Data, string? ErrorMessage)> GetEmployeesAsync(int businessId);
        Task<(bool Success, EmployeeDto? Data, string? ErrorMessage)> AddEmployeeAsync(int businessId, EmployeeUpsertDto dto, ClaimsPrincipal user);
        Task<(bool Success, string? Message, string? ErrorMessage)> AssignServicesToEmployeeAsync(int businessId, int employeeId, AssignServicesDto assignDto, ClaimsPrincipal user);
        Task<(bool Success, IEnumerable<EmployeeDto>? Data)> GetEmployeesForServiceAsync(int businessId, int serviceId);
        Task<(bool Success, IEnumerable<int>? Data)> GetAssignedServiceIdsForEmployeeAsync(int businessId, int employeeId, ClaimsPrincipal user);
        Task<(bool Success, string? ErrorMessage)> UpdateEmployeeAsync(int businessId, int employeeId, EmployeeUpsertDto dto, ClaimsPrincipal user);
        Task<(bool Success, string? Message, string? ErrorMessage)> ArchiveEmployeeAsync(int businessId, int employeeId, ClaimsPrincipal user);
        Task<(bool Success, EmployeeDetailDto? Data, string? ErrorMessage)> GetEmployeeDetailsAsync(int businessId, int employeeId, ClaimsPrincipal user);
        Task<(bool Success, string? ErrorMessage)> UpdateFinanceSettingsAsync(int businessId, int employeeId, FinanceSettingsDto dto, ClaimsPrincipal user);
        Task<(bool Success, EmployeeCertificateDto? Data, string? ErrorMessage)> AddCertificateAsync(int businessId, int employeeId, CreateCertificateDto dto, ClaimsPrincipal user);
        Task<(bool Success, string? Message, string? ErrorMessage)> DeleteCertificateAsync(int businessId, int employeeId, int certId, ClaimsPrincipal user);
        Task<(bool Success, object? Data, string? ErrorMessage)> AddAbsenceAsync(int businessId, int employeeId, CreateAbsenceDto dto, ClaimsPrincipal user);
        Task<(bool Success, object? Data, string? ErrorMessage)> ToggleAbsenceApprovalAsync(int businessId, int employeeId, int absenceId, ClaimsPrincipal user);
        Task<(bool Success, string? Message, string? ErrorMessage)> DeleteAbsenceAsync(int businessId, int employeeId, int absenceId, ClaimsPrincipal user);
    }
}
