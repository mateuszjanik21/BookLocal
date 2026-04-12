using BookLocal.API.DTOs;
using BookLocal.API.Interfaces;
using BookLocal.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BookLocal.API.Services
{
    public class ServiceCategoriesService : IServiceCategoriesService
    {
        private readonly AppDbContext _context;

        public ServiceCategoriesService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<(bool Success, IEnumerable<ServiceCategoryDto>? Data, string? ErrorMessage)> GetCategoriesAsync(int businessId, bool includeArchived, ClaimsPrincipal user)
        {
            var ownerId = user.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!await _context.Businesses.AnyAsync(b => b.BusinessId == businessId && b.OwnerId == ownerId))
            {
                return (false, null, "Brak uprawnień.");
            }

            IQueryable<ServiceCategory> query = _context.ServiceCategories
                .AsNoTracking()
                .Where(sc => sc.BusinessId == businessId);

            if (!includeArchived)
            {
                query = query.Where(sc => !sc.IsArchived);
            }

            var categoryDtos = await query
                .Select(sc => new ServiceCategoryDto
                {
                    ServiceCategoryId = sc.ServiceCategoryId,
                    Name = sc.Name,
                    PhotoUrl = sc.PhotoUrl,
                    IsArchived = sc.IsArchived,
                    Services = sc.Services
                        .Where(s => includeArchived || !s.IsArchived)
                        .Select(s => new ServiceDto
                        {
                            Id = s.ServiceId,
                            Name = s.Name,
                            Description = s.Description,
                            ServiceCategoryId = s.ServiceCategoryId,
                            IsArchived = s.IsArchived,
                            Variants = s.Variants
                                .Where(v => includeArchived || v.IsActive)
                                .Select(v => new ServiceVariantDto
                                {
                                    ServiceVariantId = v.ServiceVariantId,
                                    Name = v.Name,
                                    Price = v.Price,
                                    DurationMinutes = v.DurationMinutes,
                                    CleanupTimeMinutes = v.CleanupTimeMinutes,
                                    IsDefault = v.IsDefault,
                                    IsActive = v.IsActive,
                                    FavoritesCount = _context.UserFavoriteServices.Count(ufs => ufs.ServiceVariantId == v.ServiceVariantId)
                                }).ToList()
                        }).ToList()
                })
                .ToListAsync();

            return (true, categoryDtos, null);
        }

        public async Task<(bool Success, ServiceCategoryDto? Data, string? ErrorMessage)> CreateCategoryAsync(int businessId, ServiceCategoryUpsertDto categoryDto, ClaimsPrincipal user)
        {
            var ownerId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            var business = await _context.Businesses.FirstOrDefaultAsync(b => b.BusinessId == businessId && b.OwnerId == ownerId);

            if (business == null)
            {
                return (false, null, "Brak uprawnień.");
            }

            var newCategory = new ServiceCategory
            {
                Name = categoryDto.Name,
                BusinessId = businessId,
                MainCategoryId = categoryDto.MainCategoryId,
                IsArchived = false
            };

            _context.ServiceCategories.Add(newCategory);
            await _context.SaveChangesAsync();

            var categoryToReturn = new ServiceCategoryDto
            {
                ServiceCategoryId = newCategory.ServiceCategoryId,
                Name = newCategory.Name,
                PhotoUrl = newCategory.PhotoUrl,
                Services = new List<ServiceDto>()
            };

            return (true, categoryToReturn, null);
        }

        public async Task<(bool Success, string? Message, string? ErrorMessage)> UpdateCategoryAsync(int businessId, int categoryId, ServiceCategoryUpsertDto categoryDto, ClaimsPrincipal user)
        {
            var ownerId = user.FindFirstValue(ClaimTypes.NameIdentifier);

            var category = await _context.ServiceCategories
                .Include(sc => sc.Business)
                .FirstOrDefaultAsync(sc => sc.ServiceCategoryId == categoryId && sc.BusinessId == businessId && sc.Business != null && sc.Business.OwnerId == ownerId);

            if (category == null)
            {
                return (false, null, "Nie znaleziono kategorii.");
            }

            category.Name = categoryDto.Name;
            category.MainCategoryId = categoryDto.MainCategoryId;

            await _context.SaveChangesAsync();

            return (true, "Kategoria została zaktualizowana.", null);
        }

        public async Task<(bool Success, string? Message, string? ErrorMessage)> DeleteCategoryAsync(int businessId, int categoryId, ClaimsPrincipal user)
        {
            var ownerId = user.FindFirstValue(ClaimTypes.NameIdentifier);

            var category = await _context.ServiceCategories
                .Include(sc => sc.Business)
                .Include(sc => sc.Services)
                .ThenInclude(s => s.Variants)
                .FirstOrDefaultAsync(sc => sc.ServiceCategoryId == categoryId && sc.BusinessId == businessId && sc.Business != null && sc.Business.OwnerId == ownerId);

            if (category == null)
            {
                return (false, null, "Nie znaleziono kategorii.");
            }

            category.IsArchived = true;

            foreach (var service in category.Services)
            {
                if (!service.IsArchived)
                {
                    var variantIds = service.Variants.Select(v => v.ServiceVariantId).ToList();
                    var futureReservations = await _context.Reservations
                        .Where(r => variantIds.Contains(r.ServiceVariantId) && r.StartTime > DateTime.UtcNow && r.Status == ReservationStatus.Confirmed)
                        .ToListAsync();

                    foreach (var reservation in futureReservations)
                    {
                        reservation.Status = ReservationStatus.Cancelled;
                    }

                    foreach (var variant in service.Variants)
                    {
                        variant.IsActive = false;
                    }

                    service.IsArchived = true;
                }
            }

            await _context.SaveChangesAsync();

            return (true, "Kategoria została zarchiwizowana.", null);
        }

        public async Task<(bool Success, string? Message, string? ErrorMessage)> RestoreCategoryAsync(int businessId, int categoryId, ClaimsPrincipal user)
        {
            var ownerId = user.FindFirstValue(ClaimTypes.NameIdentifier);

            var category = await _context.ServiceCategories
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(sc => sc.ServiceCategoryId == categoryId && sc.BusinessId == businessId && sc.Business != null && sc.Business.OwnerId == ownerId);

            if (category == null)
            {
                return (false, null, "Nie znaleziono kategorii.");
            }

            category.IsArchived = false;
            await _context.SaveChangesAsync();

            return (true, "Kategoria została przywrócona.", null);
        }
    }
}
