using BookLocal.API.DTOs;
using BookLocal.Data;
using BookLocal.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
public class BusinessesController : ControllerBase
{
    private readonly AppDbContext _context;

    public BusinessesController(AppDbContext context)
    {
        _context = context;
    }


    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<object>>> GetAllBusinesses([FromQuery] string? searchQuery)
    {
        var query = _context.Businesses.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchQuery))
        {
            var searchTerm = searchQuery.ToLower();
            query = query.Where(b =>
                b.Name.ToLower().Contains(searchTerm) ||
                (b.City != null && b.City.ToLower().Contains(searchTerm))
            );
        }

        var businesses = await query
            .Select(b => new
            {
                Id = b.BusinessId,
                b.Name,
                b.NIP,
                b.City,
                b.Description,
                b.PhotoUrl
            })
            .ToListAsync();

        return Ok(businesses);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<BusinessDetailDto>> GetBusiness(int id)
    {
        var business = await _context.Businesses
            .Include(b => b.Categories)
                .ThenInclude(c => c.Services)
            .Include(b => b.Employees)
            .FirstOrDefaultAsync(b => b.BusinessId == id);

        if (business == null) return NotFound();

        var businessDto = new BusinessDetailDto
        {
            Id = business.BusinessId,
            Name = business.Name,
            NIP = business.NIP,
            Address = business.Address,
            City = business.City,
            Description = business.Description,
            PhotoUrl = business.PhotoUrl,
            Categories = business.Categories.Select(c => new ServiceCategoryDto
            {
                ServiceCategoryId = c.ServiceCategoryId,
                Name = c.Name,
                PhotoUrl = c.PhotoUrl,
                Services = c.Services.Select(s => new ServiceDto
                {
                    Id = s.ServiceId,
                    Name = s.Name,
                    Price = s.Price,
                    DurationMinutes = s.DurationMinutes
                }).ToList()
            }).ToList(),
            Employees = business.Employees.Select(e => new EmployeeDto
            {
                Id = e.EmployeeId,
                FirstName = e.FirstName,
                LastName = e.LastName,
                Position = e.Position,
                PhotoUrl = e.PhotoUrl
            }).ToList()
        };

        return Ok(businessDto);
    }

    [HttpGet("my-business")]
    [Authorize(Roles = "owner")]
    public async Task<ActionResult<BusinessDetailDto>> GetMyBusiness()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var business = await _context.Businesses
            .Include(b => b.Categories)
                .ThenInclude(c => c.Services)
            .Include(b => b.Employees)
            .FirstOrDefaultAsync(b => b.OwnerId == userId);

        if (business == null)
        {
            return NotFound("Nie znaleziono firmy dla tego właściciela.");
        }

        var businessDto = new BusinessDetailDto
        {
            Id = business.BusinessId,
            Name = business.Name,
            NIP = business.NIP,
            Address = business.Address,
            City = business.City,
            Description = business.Description,
            PhotoUrl = business.PhotoUrl,
            Categories = business.Categories.Select(c => new ServiceCategoryDto
            {
                ServiceCategoryId = c.ServiceCategoryId,
                Name = c.Name,
                PhotoUrl = c.PhotoUrl,
                Services = c.Services.Select(s => new ServiceDto
                {
                    Id = s.ServiceId,
                    Name = s.Name,
                    Price = s.Price,
                    DurationMinutes = s.DurationMinutes
                }).ToList()
            }).ToList(),
            Employees = business.Employees.Select(e => new EmployeeDto
            {
                Id = e.EmployeeId,
                FirstName = e.FirstName,
                LastName = e.LastName,
                Position = e.Position,
                PhotoUrl = e.PhotoUrl
            }).ToList()
        };

        return Ok(businessDto);
    }


    [HttpPut("{id}")]
    [Authorize(Roles = "owner")]
    public async Task<IActionResult> UpdateBusiness(int id, BusinessDto businessDto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var business = await _context.Businesses.FindAsync(id);

        if (business == null) return NotFound();

        if (business.OwnerId != userId) return Forbid();

        business.Name = businessDto.Name;
        business.NIP = businessDto.NIP;
        business.Address = businessDto.Address;
        business.City = businessDto.City;
        business.Description = businessDto.Description;

        await _context.SaveChangesAsync();
        return NoContent();
    }


    [HttpDelete("{id}")]
    [Authorize(Roles = "owner")]
    public async Task<IActionResult> DeleteBusiness(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var business = await _context.Businesses.FindAsync(id);

        if (business == null) return NotFound();

        if (business.OwnerId != userId) return Forbid();

        _context.Businesses.Remove(business);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}