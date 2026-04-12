using BookLocal.API.DTOs;
using System.Security.Claims;

namespace BookLocal.API.Interfaces
{
    public interface IReviewsService
    {
        Task<(bool Success, PagedResultDto<ReviewDto>? Data, string? ErrorMessage, int StatusCode)> GetReviewsAsync(int businessId, int pageNumber, int pageSize, int? rating, string? search, string? sortBy, ClaimsPrincipal user);
        Task<(bool Success, string? ErrorMessage, int StatusCode)> UpdateReviewAsync(int businessId, int reviewId, UpdateReviewDto dto, ClaimsPrincipal user);
        Task<(bool Success, string? ErrorMessage, int StatusCode)> DeleteReviewAsync(int businessId, int reviewId, ClaimsPrincipal user);
        Task<(bool Success, ReviewDto? Data, int BusinessId, string? ErrorMessage, int StatusCode)> PostReviewForReservationAsync(int reservationId, CreateReviewDto dto, ClaimsPrincipal user);
        Task<(bool Success, object? Data, string? ErrorMessage)> CanUserReviewAsync(int businessId, ClaimsPrincipal user);
    }
}
