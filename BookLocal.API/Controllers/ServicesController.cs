using BookLocal.API.DTOs;
using BookLocal.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

[ApiController]
[Route("api/businesses/{businessId}/[controller]")]
public class ServicesController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly UserManager<User> _userManager;

    public ServicesController(AppDbContext context, UserManager<User> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<Service>>> GetServicesForBusiness(int businessId)
    {
        if (!await _context.Businesses.AnyAsync(b => b.BusinessId == businessId))
        {
            return NotFound("Firma o podanym ID nie istnieje.");
        }

        var services = await _context.Services
            .Include(s => s.Variants)
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

        return Ok(serviceDtos);
    }

    [HttpPost]
    [Authorize(Roles = "owner")]
    public async Task<ActionResult<ServiceDto>> AddService(int businessId, ServiceUpsertDto serviceDto)
    {
        var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var categoryExists = await _context.ServiceCategories
            .AnyAsync(sc => sc.ServiceCategoryId == serviceDto.ServiceCategoryId && sc.BusinessId == businessId && sc.Business.OwnerId == ownerId);

        if (!categoryExists)
        {
            return Forbid();
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

        return CreatedAtAction(nameof(GetServicesForBusiness), new { businessId = businessId, id = service.ServiceId }, serviceToReturn);

    }

    [HttpPut("{serviceId}")]
    [Authorize(Roles = "owner")]
    public async Task<IActionResult> UpdateService(int businessId, int serviceId, ServiceUpsertDto serviceDto)
    {
        var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var service = await _context.Services
            .Include(s => s.Variants)
            .FirstOrDefaultAsync(s => s.ServiceId == serviceId && s.BusinessId == businessId && s.Business.OwnerId == ownerId);

        if (service == null)
        {
            return NotFound("Usługa nie została znaleziona lub nie masz do niej dostępu.");
        }

        service.Name = serviceDto.Name;
        service.Description = serviceDto.Description;
        service.ServiceCategoryId = serviceDto.ServiceCategoryId;

        var existingVariants = service.Variants.ToList();
        var incomingVariantIds = serviceDto.Variants
            .Where(v => v.ServiceVariantId.HasValue)
            .Select(v => v.ServiceVariantId.Value)
            .ToList();

        foreach (var incomingVariant in serviceDto.Variants.Where(v => v.ServiceVariantId.HasValue))
        {
            var existingVariant = existingVariants.FirstOrDefault(v => v.ServiceVariantId == incomingVariant.ServiceVariantId.Value);
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
        return NoContent();
    }

    [HttpDelete("{serviceId}")]
    [Authorize(Roles = "owner")]
    public async Task<IActionResult> DeleteService(int businessId, int serviceId)
    {
        var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var service = await _context.Services
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.ServiceId == serviceId && s.BusinessId == businessId && s.Business.OwnerId == ownerId);

        if (service == null)
        {
            return NotFound("Usługa nie została znaleziona lub nie masz do niej dostępu.");
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
        return NoContent();
    }

    [HttpDelete("{serviceId}/variants/{variantId}")]
    [Authorize(Roles = "owner")]
    public async Task<IActionResult> DeleteServiceVariant(int businessId, int serviceId, int variantId)
    {
        var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var variant = await _context.ServiceVariants
            .Include(v => v.Service)
            .ThenInclude(s => s.Business)
            .FirstOrDefaultAsync(v => v.ServiceVariantId == variantId && v.ServiceId == serviceId && v.Service.BusinessId == businessId && v.Service.Business.OwnerId == ownerId);

        if (variant == null)
        {
            return NotFound("Wariant nie został znaleziony lub nie masz do niego dostępu.");
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

        return NoContent();
    }

    [HttpPatch("{serviceId}/restore")]
    [Authorize(Roles = "owner")]
    public async Task<IActionResult> RestoreService(int businessId, int serviceId)
    {
        var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var service = await _context.Services
            .Include(s => s.ServiceCategory)
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.ServiceId == serviceId && s.BusinessId == businessId && s.Business.OwnerId == ownerId);

        if (service == null)
        {
            return NotFound("Usługa nie została znaleziona lub nie masz do niej dostępu.");
        }

        service.IsArchived = false;

        if (service.ServiceCategory != null && service.ServiceCategory.IsArchived)
        {
            service.ServiceCategory.IsArchived = false;
        }

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPatch("{serviceId}/variants/{variantId}/restore")]
    [Authorize(Roles = "owner")]
    public async Task<IActionResult> RestoreServiceVariant(int businessId, int serviceId, int variantId)
    {
        var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var variant = await _context.ServiceVariants
            .Include(v => v.Service)
            .ThenInclude(s => s.Business)
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(v => v.ServiceVariantId == variantId && v.ServiceId == serviceId && v.Service.BusinessId == businessId && v.Service.Business.OwnerId == ownerId);

        if (variant == null)
        {
            return NotFound("Wariant nie został znaleziony lub nie masz do niego dostępu.");
        }

        variant.IsActive = true;

        if (variant.Service.IsArchived)
        {
            variant.Service.IsArchived = false;
        }

        await _context.SaveChangesAsync();

        return NoContent();
    }
}