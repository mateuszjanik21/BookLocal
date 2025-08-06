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
            .Where(s => s.BusinessId == businessId)
            .ToListAsync();

        return Ok(services);
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
            Price = serviceDto.Price,
            DurationMinutes = serviceDto.DurationMinutes,
            ServiceCategoryId = serviceDto.ServiceCategoryId,
            BusinessId = businessId
        };

        _context.Services.Add(service);
        await _context.SaveChangesAsync();

        var serviceToReturn = new ServiceDto
        {
            Id = service.ServiceId,
            Name = service.Name,
            Price = service.Price,
            DurationMinutes = service.DurationMinutes
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
        service.Price = serviceDto.Price;
        service.DurationMinutes = serviceDto.DurationMinutes;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{serviceId}")]
    [Authorize(Roles = "owner")]
    public async Task<IActionResult> DeleteService(int businessId, int serviceId)
    {
        var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var service = await _context.Services
            .FirstOrDefaultAsync(s => s.ServiceId == serviceId && s.BusinessId == businessId && s.Business.OwnerId == ownerId);

        if (service == null)
        {
            return NotFound("Usługa nie została znaleziona lub nie masz do niej dostępu.");
        }

        try
        {
            _context.Services.Remove(service);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return Conflict(new { title = "Nie można usunąć usługi, ponieważ jest ona w użyciu (np. w rezerwacji lub jest przypisana do pracownika)." });
        }

        return NoContent();
    }
}