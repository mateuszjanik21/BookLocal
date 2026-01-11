using BookLocal.API.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookLocal.API.Controllers
{
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
                .AsNoTracking()
                .Include(sc => sc.Business)
                .Include(sc => sc.Services)
                    .ThenInclude(s => s.Variants)
                .Where(sc => sc.Services.Any(s => !s.IsArchived && s.Variants.Any(v => v.IsActive)))
                .Select(sc => new ServiceCategoryFeedDto
                {
                    ServiceCategoryId = sc.ServiceCategoryId,
                    Name = sc.Name,
                    PhotoUrl = sc.PhotoUrl,
                    BusinessId = sc.BusinessId,
                    BusinessName = sc.Business.Name,
                    BusinessCity = sc.Business.City,
                    Services = sc.Services
                        .Where(s => !s.IsArchived)
                        .Select(s => new ServiceDto
                        {
                            Id = s.ServiceId,
                            Name = s.Name,
                            Description = s.Description,
                            ServiceCategoryId = s.ServiceCategoryId,
                            IsArchived = s.IsArchived,
                            Variants = s.Variants.Select(v => new ServiceVariantDto
                            {
                                ServiceVariantId = v.ServiceVariantId,
                                Name = v.Name,
                                Price = v.Price,
                                DurationMinutes = v.DurationMinutes,
                                IsDefault = v.IsDefault
                            }).ToList()
                        }).ToList()
                })
                .ToListAsync();

            return Ok(feed);
        }
    }
}