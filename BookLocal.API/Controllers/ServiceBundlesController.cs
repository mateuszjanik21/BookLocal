using BookLocal.API.DTOs;
using BookLocal.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BookLocal.API.Controllers
{
    [ApiController]
    [Route("api/businesses/{businessId}/bundles")]
    public class ServiceBundlesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ServiceBundlesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/businesses/{businessId}/bundles
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ServiceBundleDto>>> GetBundles(int businessId)
        {
            // Allow anyone to see active bundles for a business
            var bundles = await _context.ServiceBundles
                .Where(sb => sb.BusinessId == businessId && sb.IsActive)
                .Include(sb => sb.BundleItems)
                    .ThenInclude(sbi => sbi.ServiceVariant)
                        .ThenInclude(sv => sv.Service)
                .ToListAsync();

            var dtos = bundles.Select(b => new ServiceBundleDto
            {
                ServiceBundleId = b.ServiceBundleId,
                BusinessId = b.BusinessId,
                Name = b.Name,
                Description = b.Description,
                TotalPrice = b.TotalPrice,
                PhotoUrl = b.PhotoUrl,
                IsActive = b.IsActive,
                Items = b.BundleItems.OrderBy(i => i.SequenceOrder).Select(i => new ServiceBundleItemDto
                {
                    ServiceBundleItemId = i.ServiceBundleItemId,
                    ServiceVariantId = i.ServiceVariantId,
                    VariantName = i.ServiceVariant.Name,
                    ServiceName = i.ServiceVariant.Service.Name,
                    DurationMinutes = i.ServiceVariant.DurationMinutes,
                    SequenceOrder = i.SequenceOrder,
                    OriginalPrice = i.ServiceVariant.Price
                }).ToList()
            }).ToList();

            return Ok(dtos);
        }

        // GET: api/businesses/{businessId}/bundles/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ServiceBundleDto>> GetBundle(int businessId, int id)
        {
            var bundle = await _context.ServiceBundles
                .Include(sb => sb.Business)
                .Include(sb => sb.BundleItems)
                    .ThenInclude(sbi => sbi.ServiceVariant)
                        .ThenInclude(sv => sv.Service)
                .FirstOrDefaultAsync(sb => sb.ServiceBundleId == id && sb.BusinessId == businessId);

            if (bundle == null) return NotFound();

            // Only show active bundles to non-owners (implied logic, or just show all if ID is known?)
            // For now, let's keep it simple: if it exists, show it. 
            // Ideally we might want to check if it's active if the user is not the owner, but let's stick to simple retrieval first.

            var dto = new ServiceBundleDto
            {
                ServiceBundleId = bundle.ServiceBundleId,
                BusinessId = bundle.BusinessId,
                Name = bundle.Name,
                Description = bundle.Description,
                TotalPrice = bundle.TotalPrice,
                PhotoUrl = bundle.PhotoUrl,
                IsActive = bundle.IsActive,
                Items = bundle.BundleItems.OrderBy(i => i.SequenceOrder).Select(i => new ServiceBundleItemDto
                {
                    ServiceBundleItemId = i.ServiceBundleItemId,
                    ServiceVariantId = i.ServiceVariantId,
                    VariantName = i.ServiceVariant.Name,
                    ServiceName = i.ServiceVariant.Service.Name,
                    DurationMinutes = i.ServiceVariant.DurationMinutes,
                    SequenceOrder = i.SequenceOrder,
                    OriginalPrice = i.ServiceVariant.Price
                }).ToList()
            };

            return Ok(dto);
        }

        // POST: api/businesses/{businessId}/bundles
        [HttpPost]
        [Authorize(Roles = "owner")]
        public async Task<ActionResult<ServiceBundleDto>> CreateBundle(int businessId, CreateServiceBundleDto dto)
        {
            var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var business = await _context.Businesses.FirstOrDefaultAsync(b => b.BusinessId == businessId && b.OwnerId == ownerId);

            if (business == null) return Forbid();

            // Validate variants
            var variantIds = dto.Items.Select(i => i.ServiceVariantId).ToList();
            var variantsCount = await _context.ServiceVariants
                .Where(v => variantIds.Contains(v.ServiceVariantId) && v.Service.BusinessId == businessId)
                .CountAsync();

            if (variantsCount != variantIds.Distinct().Count())
            {
                return BadRequest("Jeden lub więcej wybranych wariantów nie istnieje lub nie należy do Twojego biznesu.");
            }

            var bundle = new ServiceBundle
            {
                BusinessId = businessId,
                Name = dto.Name,
                Description = dto.Description,
                TotalPrice = dto.TotalPrice,
                IsActive = true,
                BundleItems = dto.Items.Select(i => new ServiceBundleItem
                {
                    ServiceVariantId = i.ServiceVariantId,
                    SequenceOrder = i.SequenceOrder
                }).ToList()
            };

            _context.ServiceBundles.Add(bundle);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetBundle), new { businessId, id = bundle.ServiceBundleId }, null);
        }

        // DELETE: api/businesses/{businessId}/bundles/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "owner")]
        public async Task<IActionResult> DeleteBundle(int businessId, int id)
        {
            var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var bundle = await _context.ServiceBundles
                .Include(sb => sb.Business)
                .FirstOrDefaultAsync(sb => sb.ServiceBundleId == id && sb.BusinessId == businessId);

            if (bundle == null) return NotFound();
            if (bundle.Business.OwnerId != ownerId) return Forbid();

            // Soft delete
            bundle.IsActive = false;
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
