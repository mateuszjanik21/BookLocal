using BookLocal.API.DTOs;
using System.Security.Claims;

namespace BookLocal.API.Interfaces
{
    public interface ISearchService
    {
        Task<(bool Success, PagedResultDto<ServiceSearchResultDto>? Data)> SearchServicesAsync(string? searchTerm, string? locationTerm, int? mainCategoryId, string? sortBy, int pageNumber, int pageSize);
        Task<(bool Success, PagedResultDto<BusinessSearchResultDto>? Data)> SearchBusinessesAsync(string? searchTerm, string? locationTerm, int? mainCategoryId, string? sortBy, int pageNumber, int pageSize);
        Task<(bool Success, PagedResultDto<ServiceCategorySearchResultDto>? Data)> SearchCategoryFeedAsync(string? searchTerm, string? locationTerm, int? mainCategoryId, string? sortBy, int pageNumber, int pageSize);
        Task<(bool Success, IEnumerable<RebookSuggestionDto>? Data, string? ErrorMessage)> GetRebookSuggestionsAsync(ClaimsPrincipal user);
    }
}
