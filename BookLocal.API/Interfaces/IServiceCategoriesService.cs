using BookLocal.API.DTOs;
using System.Security.Claims;

namespace BookLocal.API.Interfaces
{
    public interface IServiceCategoriesService
    {
        Task<(bool Success, IEnumerable<ServiceCategoryDto>? Data, string? ErrorMessage)> GetCategoriesAsync(int businessId, bool includeArchived, ClaimsPrincipal user);
        Task<(bool Success, ServiceCategoryDto? Data, string? ErrorMessage)> CreateCategoryAsync(int businessId, ServiceCategoryUpsertDto categoryDto, ClaimsPrincipal user);
        Task<(bool Success, string? Message, string? ErrorMessage)> UpdateCategoryAsync(int businessId, int categoryId, ServiceCategoryUpsertDto categoryDto, ClaimsPrincipal user);
        Task<(bool Success, string? Message, string? ErrorMessage)> DeleteCategoryAsync(int businessId, int categoryId, ClaimsPrincipal user);
        Task<(bool Success, string? Message, string? ErrorMessage)> RestoreCategoryAsync(int businessId, int categoryId, ClaimsPrincipal user);
    }
}
