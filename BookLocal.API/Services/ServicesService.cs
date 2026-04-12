using BookLocal.API.DTOs;
using BookLocal.API.Interfaces;
using BookLocal.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BookLocal.API.Services
{
    public class ServicesService : IServicesService
    {
        private readonly AppDbContext _context;

        public ServicesService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<(bool Success, IEnumerable<ServiceDto>? Data, string? ErrorMessage)> GetServicesForBusinessAsync(int businessId)
        {
            if (!await _context.Businesses.AnyAsync(b => b.BusinessId == businessId))
            {
                return (false, null, "Firma o podanym ID nie istnieje.");
            }

            var services = await _context.Services
                .Include(s => s.Variants)
                .AsNoTracking()
                .Where(s => s.BusinessId == businessId)
                .ToListAsync();

            var serviceDtos = services.Select(s => new ServiceDto
            {
                Id = s.ServiceId,
                Name = s.Name,
                ServiceCategoryId = s.ServiceCategoryId,
                IsArchived = s.IsArchived,
                Variants = s.Variants
                    .Where(v => v.IsActive)
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
            });

            return (true, serviceDtos, null);
        }

        public async Task<(bool Success, ServiceDto? Data, string? ErrorMessage)> AddServiceAsync(int businessId, ServiceUpsertDto serviceDto, ClaimsPrincipal user)
        {
            var ownerId = user.FindFirstValue(ClaimTypes.NameIdentifier);

            var categoryExists = await _context.ServiceCategories
                .Include(sc => sc.Business)
                .AnyAsync(sc => sc.ServiceCategoryId == serviceDto.ServiceCategoryId && sc.BusinessId == businessId && sc.Business != null && sc.Business.OwnerId == ownerId);

            if (!categoryExists)
            {
                return (false, null, "Brak uprawnień do dodania usługi w tej kategorii.");
            }

            var service = new Service
            {
                Name = serviceDto.Name,
                Description = serviceDto.Description,
                ServiceCategoryId = serviceDto.ServiceCategoryId,
                BusinessId = businessId,
                Variants = serviceDto.Variants.Select(v => new ServiceVariant
                {
                    Name = v.Name,
                    Price = v.Price,
                    DurationMinutes = v.DurationMinutes,
                    CleanupTimeMinutes = v.CleanupTimeMinutes,
                    IsDefault = v.IsDefault,
                    IsActive = true
                }).ToList()
            };

            _context.Services.Add(service);
            await _context.SaveChangesAsync();

            var serviceToReturn = new ServiceDto
            {
                Id = service.ServiceId,
                Name = service.Name,
                ServiceCategoryId = service.ServiceCategoryId,
                Variants = service.Variants.Select(v => new ServiceVariantDto
                {
                    ServiceVariantId = v.ServiceVariantId,
                    Name = v.Name,
                    Price = v.Price,
                    DurationMinutes = v.DurationMinutes,
                    IsDefault = v.IsDefault
                }).ToList()
            };

            return (true, serviceToReturn, null);
        }

        public async Task<(bool Success, string? Message, string? ErrorMessage)> UpdateServiceAsync(int businessId, int serviceId, ServiceUpsertDto serviceDto, ClaimsPrincipal user)
        {
            var ownerId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            var service = await _context.Services
                .Include(s => s.Variants)
                .Include(s => s.Business)
                .FirstOrDefaultAsync(s => s.ServiceId == serviceId && s.BusinessId == businessId && s.Business != null && s.Business.OwnerId == ownerId);

            if (service == null)
            {
                return (false, null, "Usługa nie została znaleziona lub nie masz do niej dostępu.");
            }

            service.Name = serviceDto.Name;
            service.Description = serviceDto.Description;
            service.ServiceCategoryId = serviceDto.ServiceCategoryId;

            var existingVariants = service.Variants.ToList();
            var incomingVariantIds = serviceDto.Variants
                .Where(v => v.ServiceVariantId.HasValue)
                .Select(v => v.ServiceVariantId!.Value)
                .ToList();

            foreach (var incomingVariant in serviceDto.Variants.Where(v => v.ServiceVariantId.HasValue))
            {
                var existingVariant = existingVariants.FirstOrDefault(v => v.ServiceVariantId == incomingVariant.ServiceVariantId!.Value);
                if (existingVariant != null)
                {
                    existingVariant.Name = incomingVariant.Name;
                    existingVariant.Price = incomingVariant.Price;
                    existingVariant.DurationMinutes = incomingVariant.DurationMinutes;
                    existingVariant.CleanupTimeMinutes = incomingVariant.CleanupTimeMinutes;
                    existingVariant.IsDefault = incomingVariant.IsDefault;
                }
            }

            var newVariants = serviceDto.Variants.Where(v => !v.ServiceVariantId.HasValue).ToList();
            foreach (var newVariantDto in newVariants)
            {
                service.Variants.Add(new ServiceVariant
                {
                    Name = newVariantDto.Name,
                    Price = newVariantDto.Price,
                    DurationMinutes = newVariantDto.DurationMinutes,
                    CleanupTimeMinutes = newVariantDto.CleanupTimeMinutes,
                    IsDefault = newVariantDto.IsDefault,
                    IsActive = true,
                    ServiceId = serviceId
                });
            }

            var variantsToDelete = existingVariants
                .Where(v => !incomingVariantIds.Contains(v.ServiceVariantId) && v.IsActive)
                .ToList();

            foreach (var variantToDelete in variantsToDelete)
            {
                var futureReservations = await _context.Reservations
                    .Where(r => r.ServiceVariantId == variantToDelete.ServiceVariantId && r.StartTime > DateTime.UtcNow && r.Status == ReservationStatus.Confirmed)
                    .ToListAsync();

                foreach (var reservation in futureReservations)
                {
                    reservation.Status = ReservationStatus.Cancelled;
                }

                variantToDelete.IsActive = false;
            }

            await _context.SaveChangesAsync();
            return (true, "Usługa zaktualizowana.", null);
        }

        public async Task<(bool Success, string? Message, string? ErrorMessage)> DeleteServiceAsync(int businessId, int serviceId, ClaimsPrincipal user)
        {
            var ownerId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            var service = await _context.Services
                .Include(s => s.Business)
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(s => s.ServiceId == serviceId && s.BusinessId == businessId && s.Business != null && s.Business.OwnerId == ownerId);

            if (service == null)
            {
                return (false, null, "Usługa nie została znaleziona lub nie masz do niej dostępu.");
            }

            var variants = await _context.ServiceVariants
                .Where(v => v.ServiceId == serviceId)
                .ToListAsync();

            var variantIds = variants.Select(v => v.ServiceVariantId).ToList();

            var futureReservations = await _context.Reservations
                .Where(r => variantIds.Contains(r.ServiceVariantId) && r.StartTime > DateTime.UtcNow && r.Status == ReservationStatus.Confirmed)
                .ToListAsync();

            foreach (var reservation in futureReservations)
            {
                reservation.Status = ReservationStatus.Cancelled;
            }

            foreach (var variant in variants)
            {
                variant.IsActive = false;
            }

            service.IsArchived = true;
            await _context.SaveChangesAsync();

            return (true, "Usługa usunięta.", null);
        }

        public async Task<(bool Success, string? Message, string? ErrorMessage)> DeleteServiceVariantAsync(int businessId, int serviceId, int variantId, ClaimsPrincipal user)
        {
            var ownerId = user.FindFirstValue(ClaimTypes.NameIdentifier);

            var variant = await _context.ServiceVariants
                .Include(v => v.Service)
                .ThenInclude(s => s.Business)
                .FirstOrDefaultAsync(v => v.ServiceVariantId == variantId && v.ServiceId == serviceId && v.Service != null && v.Service.BusinessId == businessId && v.Service.Business != null && v.Service.Business.OwnerId == ownerId);

            if (variant == null)
            {
                return (false, null, "Wariant nie został znaleziony lub nie masz do niego dostępu.");
            }

            var futureReservations = await _context.Reservations
                .Where(r => r.ServiceVariantId == variantId && r.StartTime > DateTime.UtcNow && r.Status == ReservationStatus.Confirmed)
                .ToListAsync();

            foreach (var reservation in futureReservations)
            {
                reservation.Status = ReservationStatus.Cancelled;
            }

            variant.IsActive = false;

            await _context.SaveChangesAsync();

            return (true, "Wariant usunięty.", null);
        }

        public async Task<(bool Success, string? Message, string? ErrorMessage)> RestoreServiceAsync(int businessId, int serviceId, ClaimsPrincipal user)
        {
            var ownerId = user.FindFirstValue(ClaimTypes.NameIdentifier);

            var service = await _context.Services
                .Include(s => s.ServiceCategory)
                .Include(s => s.Business)
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(s => s.ServiceId == serviceId && s.BusinessId == businessId && s.Business != null && s.Business.OwnerId == ownerId);

            if (service == null)
            {
                return (false, null, "Usługa nie została znaleziona lub nie masz do niej dostępu.");
            }

            service.IsArchived = false;

            if (service.ServiceCategory != null && service.ServiceCategory.IsArchived)
            {
                service.ServiceCategory.IsArchived = false;
            }

            await _context.SaveChangesAsync();

            return (true, "Usługa przywrócona.", null);
        }

        public async Task<(bool Success, string? Message, string? ErrorMessage)> RestoreServiceVariantAsync(int businessId, int serviceId, int variantId, ClaimsPrincipal user)
        {
            var ownerId = user.FindFirstValue(ClaimTypes.NameIdentifier);

            var variant = await _context.ServiceVariants
                .Include(v => v.Service)
                .ThenInclude(s => s.Business)
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(v => v.ServiceVariantId == variantId && v.ServiceId == serviceId && v.Service != null && v.Service.BusinessId == businessId && v.Service.Business != null && v.Service.Business.OwnerId == ownerId);

            if (variant == null)
            {
                return (false, null, "Wariant nie został znaleziony lub nie masz do niego dostępu.");
            }

            variant.IsActive = true;

            if (variant.Service != null && variant.Service.IsArchived)
            {
                variant.Service.IsArchived = false;
            }

            await _context.SaveChangesAsync();

            return (true, "Wariant przywrócony.", null);
        }
    }
}
