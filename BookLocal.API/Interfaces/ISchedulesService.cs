using BookLocal.API.DTOs;
using System.Security.Claims;

namespace BookLocal.API.Interfaces
{
    public interface ISchedulesService
    {
        Task<(bool Success, IEnumerable<WorkScheduleDto>? Data, string? ErrorMessage)> GetScheduleAsync(int employeeId, ClaimsPrincipal user);
        Task<(bool Success, string? Message, string? ErrorMessage)> UpdateScheduleAsync(int employeeId, List<WorkScheduleDto> schedulePayload, ClaimsPrincipal user);
    }
}
