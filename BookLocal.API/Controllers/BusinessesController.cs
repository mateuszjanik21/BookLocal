using BookLocal.API.DTOs;
using BookLocal.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
public class BusinessesController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly UserManager<User> _userManager;

    public BusinessesController(AppDbContext context, UserManager<User> userManager)
    {
        _context = context;
        _userManager = userManager;
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
                b.Description
            })
            .ToListAsync();

        return Ok(businesses);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<BusinessDetailDto>> GetBusiness(int id)
    {
        var business = await _context.Businesses
            .Include(b => b.Services)
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
            Services = business.Services.Select(s => new ServiceDto
            {
                Id = s.ServiceId,
                Name = s.Name,
                Price = s.Price,
                DurationMinutes = s.DurationMinutes
            }).ToList(),
            Employees = business.Employees.Select(e => new EmployeeDto
            {
                Id = e.EmployeeId,
                FirstName = e.FirstName,
                LastName = e.LastName,
                Position = e.Position
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
            .Include(b => b.Services)
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
            Services = business.Services.Select(s => new ServiceDto
            {
                Id = s.ServiceId,
                Name = s.Name,
                Price = s.Price,
                DurationMinutes = s.DurationMinutes
            }).ToList(),
            Employees = business.Employees.Select(e => new EmployeeDto
            {
                Id = e.EmployeeId,
                FirstName = e.FirstName,
                LastName = e.LastName,
                Position = e.Position
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