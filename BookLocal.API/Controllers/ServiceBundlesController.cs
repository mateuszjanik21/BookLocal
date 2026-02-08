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
            var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isOwner = false;

            if (ownerId != null)
            {
                isOwner = await _context.Businesses.AnyAsync(b => b.BusinessId == businessId && b.OwnerId == ownerId);
            }

            var query = _context.ServiceBundles
                .AsNoTracking()
                .Where(sb => sb.BusinessId == businessId);

            if (!isOwner)
            {
                query = query.Where(sb => sb.IsActive);
            }

            var dtos = await query
                .Select(b => new ServiceBundleDto
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
                })
                .ToListAsync();

            return Ok(dtos);
        }

        // GET: api/businesses/{businessId}/bundles/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ServiceBundleDto>> GetBundle(int businessId, int id)
        {
            var dto = await _context.ServiceBundles
                .AsNoTracking()
                .Where(sb => sb.ServiceBundleId == id && sb.BusinessId == businessId)
                .Select(b => new ServiceBundleDto
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
                })
                .FirstOrDefaultAsync();

            if (dto == null) return NotFound();

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
                IsActive = dto.IsActive,
                BundleItems = dto.Items.Select(i => new ServiceBundleItem
                {
                    ServiceVariantId = i.ServiceVariantId,
                    SequenceOrder = i.SequenceOrder
                }).ToList()
            };

            _context.ServiceBundles.Add(bundle);
            await _context.SaveChangesAsync();

            var resultDto = await _context.ServiceBundles
                .AsNoTracking()
                .Where(sb => sb.ServiceBundleId == bundle.ServiceBundleId)
                .Select(b => new ServiceBundleDto
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
                })
                .FirstOrDefaultAsync();

            return CreatedAtAction(nameof(GetBundle), new { businessId, id = bundle.ServiceBundleId }, resultDto);
        }

        // PUT: api/businesses/{businessId}/bundles/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "owner")]
        public async Task<ActionResult<ServiceBundleDto>> UpdateBundle(int businessId, int id, CreateServiceBundleDto dto)
        {
            var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var bundle = await _context.ServiceBundles
                .Include(sb => sb.Business)
                .Include(sb => sb.BundleItems)
                .FirstOrDefaultAsync(sb => sb.ServiceBundleId == id && sb.BusinessId == businessId);

            if (bundle == null) return NotFound();
            if (bundle.Business.OwnerId != ownerId) return Forbid();

            var variantIds = dto.Items.Select(i => i.ServiceVariantId).ToList();
            var variantsCount = await _context.ServiceVariants
                .Where(v => variantIds.Contains(v.ServiceVariantId) && v.Service.BusinessId == businessId)
                .CountAsync();

            if (variantsCount != variantIds.Distinct().Count())
            {
                return BadRequest("Jeden lub więcej wybranych wariantów nie istnieje lub nie należy do Twojego biznesu.");
            }

            bundle.Name = dto.Name;
            bundle.Description = dto.Description;
            bundle.TotalPrice = dto.TotalPrice;
            bundle.IsActive = dto.IsActive;

            _context.ServiceBundleItems.RemoveRange(bundle.BundleItems);
            bundle.BundleItems = dto.Items.Select(i => new ServiceBundleItem
            {
                ServiceBundleId = id,
                ServiceVariantId = i.ServiceVariantId,
                SequenceOrder = i.SequenceOrder
            }).ToList();

            await _context.SaveChangesAsync();

            return Ok(new ServiceBundleDto { ServiceBundleId = bundle.ServiceBundleId });
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

            bundle.IsActive = false;
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
