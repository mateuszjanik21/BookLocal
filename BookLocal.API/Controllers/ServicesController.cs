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
            Variants = s.Variants.Select(v => new ServiceVariantDto
            {
                ServiceVariantId = v.ServiceVariantId,
                Name = v.Name,
                Price = v.Price,
                DurationMinutes = v.DurationMinutes,
                IsDefault = v.IsDefault,
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
            .FirstOrDefaultAsync(s => s.ServiceId == serviceId && s.BusinessId == businessId && s.Business.OwnerId == ownerId);

        if (service == null)
        {
            return NotFound("Usługa nie została znaleziona lub nie masz do niej dostępu.");
        }

        service.Name = serviceDto.Name;
        service.Description = serviceDto.Description;
        service.ServiceCategoryId = serviceDto.ServiceCategoryId;

        _context.ServiceVariants.RemoveRange(service.Variants);

        service.Variants = serviceDto.Variants.Select(v => new ServiceVariant
        {
            Name = v.Name,
            Price = v.Price,
            DurationMinutes = v.DurationMinutes,
            CleanupTimeMinutes = v.CleanupTimeMinutes,
            IsDefault = v.IsDefault,
            IsActive = true,
            ServiceId = service.ServiceId
        }).ToList();

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

        var variantIds = await _context.ServiceVariants
            .Where(v => v.ServiceId == serviceId)
            .Select(v => v.ServiceVariantId)
            .ToListAsync();

        var futureReservations = await _context.Reservations
            .Where(r => variantIds.Contains(r.ServiceVariantId) && r.StartTime > DateTime.UtcNow && r.Status == ReservationStatus.Confirmed)
            .ToListAsync();

        foreach (var reservation in futureReservations)
        {
            reservation.Status = ReservationStatus.Cancelled;
        }

        service.IsArchived = true;
        await _context.SaveChangesAsync();
        return NoContent();
    }
}