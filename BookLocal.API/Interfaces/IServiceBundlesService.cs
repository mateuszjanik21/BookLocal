using BookLocal.API.DTOs;
using System.Security.Claims;

namespace BookLocal.API.Interfaces
{
    public interface IServiceBundlesService
    {
        Task<(bool Success, IEnumerable<ServiceBundleDto>? Data, string? ErrorMessage)> GetBundlesAsync(int businessId, ClaimsPrincipal user);
        Task<(bool Success, ServiceBundleDto? Data, string? ErrorMessage)> GetBundleAsync(int businessId, int id);
        Task<(bool Success, ServiceBundleDto? Data, string? ErrorMessage)> CreateBundleAsync(int businessId, CreateServiceBundleDto dto, ClaimsPrincipal user);
        Task<(bool Success, ServiceBundleDto? Data, string? ErrorMessage)> UpdateBundleAsync(int businessId, int id, CreateServiceBundleDto dto, ClaimsPrincipal user);
        Task<(bool Success, string? Message, string? ErrorMessage)> DeleteBundleAsync(int businessId, int id, ClaimsPrincipal user);
    }
}
