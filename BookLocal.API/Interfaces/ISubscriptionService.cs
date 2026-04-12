using BookLocal.API.DTOs;
using System.Security.Claims;

namespace BookLocal.API.Interfaces
{
    public interface ISubscriptionService
    {
        Task<(bool Success, IEnumerable<SubscriptionPlanDto>? Data, string? ErrorMessage)> GetPublicPlansAsync();
        Task<(bool Success, string? Message, string? ErrorMessage)> SubscribeAsync(int planId, ClaimsPrincipal user);
        Task<(bool Success, object? Data, string? ErrorMessage)> GetCurrentSubscriptionAsync(ClaimsPrincipal user);
    }
}
