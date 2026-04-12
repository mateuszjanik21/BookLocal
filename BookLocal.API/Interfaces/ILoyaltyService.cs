using BookLocal.API.DTOs;
using System.Security.Claims;

namespace BookLocal.API.Interfaces
{
    public interface ILoyaltyService
    {
        Task<(bool Success, LoyaltyConfigDto? Data)> GetConfigAsync(int businessId);
        Task<(bool Success, object? Data)> UpdateConfigAsync(int businessId, LoyaltyConfigDto dto);
        Task<(bool Success, LoyaltyStatsDto? Data)> GetStatsAsync(int businessId);
        Task<(bool Success, object? Data)> GetCustomerLoyaltyAsync(int businessId, string customerId);
        Task<(bool Success, string? Message, string? ErrorMessage)> RecalculatePointsAsync(int businessId);
    }
}
