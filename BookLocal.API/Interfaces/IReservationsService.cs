using BookLocal.API.DTOs;
using System.Security.Claims;

namespace BookLocal.API.Interfaces
{
    public interface IReservationsService
    {
        Task<(bool Success, string? Message, string? ErrorMessage)> CreateReservationAsync(ReservationCreateDto dto, ClaimsPrincipal user);
        Task<(bool Success, CustomerStatsDto? Data, string? ErrorMessage)> GetMyStatsAsync(ClaimsPrincipal user);
        Task<(bool Success, PagedResultDto<ReservationDto>? Data)> GetMyReservationsAsync(string scope, int pageNumber, int pageSize, ClaimsPrincipal user);
        Task<(bool Success, IEnumerable<ReservationDto>? Data)> GetCalendarEventsAsync(DateTime? start, DateTime? end, int? employeeId, ClaimsPrincipal user);
        Task<(bool Success, ReservationDto? Data, string? ErrorMessage)> GetReservationByIdAsync(int id, ClaimsPrincipal user);
        Task<(bool Success, string? Message, string? ErrorMessage)> UpdateReservationStatusAsync(int id, UpdateReservationStatusDto dto, ClaimsPrincipal user);
        Task<(bool Success, string? Message, string? ErrorMessage)> CancelReservationAsync(int id, ClaimsPrincipal user);
        Task<(bool Success, string? Message, string? ErrorMessage)> CreateBundleReservationAsync(BundleReservationCreateDto dto, ClaimsPrincipal user);
        Task<(bool Success, string? Message, string? ErrorMessage)> CreateReservationAsOwnerAsync(OwnerCreateReservationDto dto, ClaimsPrincipal user);
        Task<(bool Success, string? Message, string? ErrorMessage)> CreateBundleReservationAsOwnerAsync(OwnerCreateBundleReservationDto dto, ClaimsPrincipal user);
        Task<(bool Success, object? Data, string? ErrorMessage)> GetAdjacentReservationsAsync(int id, ClaimsPrincipal user);
    }
}
