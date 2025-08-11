using BookLocal.API.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly AppDbContext _context;

    public CategoriesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("feed")]
    public async Task<ActionResult<IEnumerable<ServiceCategoryFeedDto>>> GetCategoryFeed()
    {
        var feed = await _context.ServiceCategories
            .Include(sc => sc.Business)
            .Include(sc => sc.Services) 
            .Where(sc => sc.Services.Any()) 
            .Select(sc => new ServiceCategoryFeedDto
            {
                ServiceCategoryId = sc.ServiceCategoryId,
                Name = sc.Name,
                PhotoUrl = sc.PhotoUrl,
                BusinessId = sc.BusinessId,
                BusinessName = sc.Business.Name,
                BusinessCity = sc.Business.City,
                Services = sc.Services.Select(s => new ServiceDto 
                {
                    Id = s.ServiceId,
                    Name = s.Name,
                    Price = s.Price,
                    DurationMinutes = s.DurationMinutes,
                    ServiceCategoryId = s.ServiceCategoryId
                })
            })
            .ToListAsync();

        return Ok(feed);
    }
}