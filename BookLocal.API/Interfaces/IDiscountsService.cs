using BookLocal.API.DTOs;
using System.Security.Claims;

namespace BookLocal.API.Interfaces
{
    public interface IDiscountsService
    {
        Task<(bool Success, IEnumerable<DiscountDto>? Data)> GetDiscountsAsync(int businessId, ClaimsPrincipal user);
        Task<(bool Success, DiscountDto? Data, string? ErrorMessage)> CreateDiscountAsync(int businessId, CreateDiscountDto dto, ClaimsPrincipal user);
        Task<(bool Success, bool? IsActive)> ToggleDiscountAsync(int businessId, int discountId, ClaimsPrincipal user);
        Task<(bool Success, VerifyDiscountResult? Data)> VerifyDiscountAsync(int businessId, VerifyDiscountRequest request);
    }
}
