using BookLocal.API.DTOs;
using BookLocal.API.Interfaces;
using BookLocal.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BookLocal.API.Services
{
    public class FavoritesService : IFavoritesService
    {
        private readonly AppDbContext _context;

        public FavoritesService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<(bool Success, PagedResultDto<FavoriteServiceDto>? Data)> GetFavoritesAsync(int pageNumber, int pageSize, ClaimsPrincipal user)
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return (false, null);

            var query = _context.UserFavoriteServices
                .AsNoTracking()
                .Include(f => f.ServiceVariant)
                    .ThenInclude(v => v.Service)
                        .ThenInclude(s => s.Business)
                .Where(f => f.UserId == userId);

            var totalCount = await query.CountAsync();

            var favorites = await query
                .OrderByDescending(f => f.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(f => new FavoriteServiceDto
                {
                    ServiceVariantId = f.ServiceVariantId,
                    ServiceName = f.ServiceVariant.Service.Name,
                    VariantName = f.ServiceVariant.Name,
                    Price = f.ServiceVariant.Price,
                    DurationMinutes = f.ServiceVariant.DurationMinutes,
                    BusinessId = f.ServiceVariant.Service.BusinessId,
                    BusinessName = f.ServiceVariant.Service.Business.Name,
                    BusinessCity = f.ServiceVariant.Service.Business.City,
                    BusinessPhotoUrl = f.ServiceVariant.Service.Business.PhotoUrl,
                    IsActive = f.ServiceVariant.IsActive,
                    IsServiceArchived = f.ServiceVariant.Service.IsArchived
                })
                .ToListAsync();

            var result = new PagedResultDto<FavoriteServiceDto>
            {
                Items = favorites,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            return (true, result);
        }

        public async Task<(bool Success, string? ErrorMessage)> AddFavoriteAsync(int serviceVariantId, ClaimsPrincipal user)
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return (false, "Brak weryfikacji");

            var variantExists = await _context.ServiceVariants.AnyAsync(v => v.ServiceVariantId == serviceVariantId);
            if (!variantExists) return (false, "Wariant usługi nie istnieje.");

            var existing = await _context.UserFavoriteServices
                .FirstOrDefaultAsync(f => f.UserId == userId && f.ServiceVariantId == serviceVariantId);

            if (existing != null) return (true, null);

            var favorite = new UserFavoriteService
            {
                UserId = userId,
                ServiceVariantId = serviceVariantId,
                CreatedAt = DateTime.UtcNow
            };

            _context.UserFavoriteServices.Add(favorite);
            await _context.SaveChangesAsync();

            return (true, null);
        }

        public async Task<(bool Success, string? ErrorMessage)> RemoveFavoriteAsync(int serviceVariantId, ClaimsPrincipal user)
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return (false, "Brak autoryzacji");

            var favorite = await _context.UserFavoriteServices
                .FirstOrDefaultAsync(f => f.UserId == userId && f.ServiceVariantId == serviceVariantId);

            if (favorite == null) return (false, "Ulubiona usługa nie znaleziona.");

            _context.UserFavoriteServices.Remove(favorite);
            await _context.SaveChangesAsync();

            return (true, null);
        }

        public async Task<(bool Success, bool IsFavorite)> IsFavoriteAsync(int serviceVariantId, ClaimsPrincipal user)
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return (false, false);

            var isFavorite = await _context.UserFavoriteServices
                .AnyAsync(f => f.UserId == userId && f.ServiceVariantId == serviceVariantId);

            return (true, isFavorite);
        }
    }
}
