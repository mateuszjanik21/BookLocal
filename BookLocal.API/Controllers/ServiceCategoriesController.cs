using BookLocal.API.DTOs;
using BookLocal.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BookLocal.API.Controllers
{
    [ApiController]
    [Route("api/businesses/{businessId}/categories")]
    [Authorize(Roles = "owner")]
    public class ServiceCategoriesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ServiceCategoriesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ServiceCategoryDto>>> GetCategories(int businessId, [FromQuery] bool includeArchived = false)
        {
            var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!await _context.Businesses.AnyAsync(b => b.BusinessId == businessId && b.OwnerId == ownerId))
            {
                return Forbid();
            }

            var categories = await _context.ServiceCategories
                .Where(sc => sc.BusinessId == businessId)
                .Include(sc => sc.Services)
                    .ThenInclude(s => s.Variants)
                .ToListAsync();

            var categoryDtos = categories.Select(sc => new ServiceCategoryDto
            {
                ServiceCategoryId = sc.ServiceCategoryId,
                Name = sc.Name,
                PhotoUrl = sc.PhotoUrl,
                Services = sc.Services
                    .Where(s => includeArchived || !s.IsArchived)
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
            }).ToList();

            return Ok(categoryDtos);
        }

        [HttpPost]
        public async Task<ActionResult<ServiceCategoryDto>> CreateCategory(int businessId, ServiceCategoryUpsertDto categoryDto)
        {
            var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var business = await _context.Businesses.FirstOrDefaultAsync(b => b.BusinessId == businessId && b.OwnerId == ownerId);

            if (business == null)
            {
                return Forbid();
            }

            var newCategory = new ServiceCategory
            {
                Name = categoryDto.Name,
                BusinessId = businessId,
                MainCategoryId = categoryDto.MainCategoryId
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

            return CreatedAtAction(nameof(GetCategories), new { businessId = businessId }, categoryToReturn);
        }

        [HttpPut("{categoryId}")]
        public async Task<IActionResult> UpdateCategory(int businessId, int categoryId, ServiceCategoryUpsertDto categoryDto)
        {
            var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var category = await _context.ServiceCategories
                .Include(sc => sc.Business)
                .FirstOrDefaultAsync(sc => sc.ServiceCategoryId == categoryId && sc.BusinessId == businessId && sc.Business.OwnerId == ownerId);

            if (category == null)
            {
                return NotFound();
            }

            category.Name = categoryDto.Name;
            category.MainCategoryId = categoryDto.MainCategoryId;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{categoryId}")]
        public async Task<IActionResult> DeleteCategory(int businessId, int categoryId)
        {
            var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var category = await _context.ServiceCategories
                .Include(sc => sc.Business)
                .Include(sc => sc.Services)
                .FirstOrDefaultAsync(sc => sc.ServiceCategoryId == categoryId && sc.BusinessId == businessId && sc.Business.OwnerId == ownerId);

            if (category == null)
            {
                return NotFound();
            }

            if (category.Services.Any())
            {
                return BadRequest("Nie można usunąć kategorii, która zawiera usługi. Najpierw usuń lub przenieś usługi.");
            }

            _context.ServiceCategories.Remove(category);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}