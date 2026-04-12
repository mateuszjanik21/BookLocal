using BookLocal.API.DTOs;
using System.Security.Claims;

namespace BookLocal.API.Interfaces
{
    public interface IServicesService
    {
        Task<(bool Success, IEnumerable<ServiceDto>? Data, string? ErrorMessage)> GetServicesForBusinessAsync(int businessId);
        Task<(bool Success, ServiceDto? Data, string? ErrorMessage)> AddServiceAsync(int businessId, ServiceUpsertDto serviceDto, ClaimsPrincipal user);
        Task<(bool Success, string? Message, string? ErrorMessage)> UpdateServiceAsync(int businessId, int serviceId, ServiceUpsertDto serviceDto, ClaimsPrincipal user);
        Task<(bool Success, string? Message, string? ErrorMessage)> DeleteServiceAsync(int businessId, int serviceId, ClaimsPrincipal user);
        Task<(bool Success, string? Message, string? ErrorMessage)> DeleteServiceVariantAsync(int businessId, int serviceId, int variantId, ClaimsPrincipal user);
        Task<(bool Success, string? Message, string? ErrorMessage)> RestoreServiceAsync(int businessId, int serviceId, ClaimsPrincipal user);
        Task<(bool Success, string? Message, string? ErrorMessage)> RestoreServiceVariantAsync(int businessId, int serviceId, int variantId, ClaimsPrincipal user);
    }
}
