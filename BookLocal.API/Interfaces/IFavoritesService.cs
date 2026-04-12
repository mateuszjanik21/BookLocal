using BookLocal.API.DTOs;
using System.Security.Claims;

namespace BookLocal.API.Interfaces
{
    public interface IFavoritesService
    {
        Task<(bool Success, PagedResultDto<FavoriteServiceDto>? Data)> GetFavoritesAsync(int pageNumber, int pageSize, ClaimsPrincipal user);
        Task<(bool Success, string? ErrorMessage)> AddFavoriteAsync(int serviceVariantId, ClaimsPrincipal user);
        Task<(bool Success, string? ErrorMessage)> RemoveFavoriteAsync(int serviceVariantId, ClaimsPrincipal user);
        Task<(bool Success, bool IsFavorite)> IsFavoriteAsync(int serviceVariantId, ClaimsPrincipal user);
    }
}
