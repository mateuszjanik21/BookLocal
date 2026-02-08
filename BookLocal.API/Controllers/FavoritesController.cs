using BookLocal.API.DTOs;
using BookLocal.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BookLocal.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FavoritesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public FavoritesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<FavoriteServiceDto>>> GetFavorites()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var favorites = await _context.UserFavoriteServices
                .Include(f => f.ServiceVariant)
                    .ThenInclude(v => v.Service)
                        .ThenInclude(s => s.Business)
                .Where(f => f.UserId == userId)
                .Select(f => new FavoriteServiceDto
                {
                    ServiceVariantId = f.ServiceVariantId,
                    ServiceName = f.ServiceVariant.Service.Name,
                    VariantName = f.ServiceVariant.Name,
                    Price = f.ServiceVariant.Price,
                    DurationMinutes = f.ServiceVariant.DurationMinutes,
                    BusinessId = f.ServiceVariant.Service.BusinessId,
                    BusinessName = f.ServiceVariant.Service.Business.Name,
                    BusinessCity = f.ServiceVariant.Service.Business.City,
                    BusinessPhotoUrl = f.ServiceVariant.Service.Business.PhotoUrl,
                    IsActive = f.ServiceVariant.IsActive,
                    IsServiceArchived = f.ServiceVariant.Service.IsArchived
                })
                .ToListAsync();

            return Ok(favorites);
        }

        [HttpPost("{serviceVariantId}")]
        public async Task<IActionResult> AddFavorite(int serviceVariantId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            // Check if variant exists
            var variantExists = await _context.ServiceVariants.AnyAsync(v => v.ServiceVariantId == serviceVariantId);
            if (!variantExists) return NotFound("Wariant usługi nie istnieje.");

            // Check if already favorite
            var existing = await _context.UserFavoriteServices
                .FirstOrDefaultAsync(f => f.UserId == userId && f.ServiceVariantId == serviceVariantId);

            if (existing != null) return Ok(); // Idempotent

            var favorite = new UserFavoriteService
            {
                UserId = userId,
                ServiceVariantId = serviceVariantId,
                CreatedAt = DateTime.UtcNow
            };

            _context.UserFavoriteServices.Add(favorite);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpDelete("{serviceVariantId}")]
        public async Task<IActionResult> RemoveFavorite(int serviceVariantId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var favorite = await _context.UserFavoriteServices
                .FirstOrDefaultAsync(f => f.UserId == userId && f.ServiceVariantId == serviceVariantId);

            if (favorite == null) return NotFound("Ulubiona usługa nie znaleziona.");

            _context.UserFavoriteServices.Remove(favorite);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("check/{serviceVariantId}")]
        public async Task<ActionResult<bool>> IsFavorite(int serviceVariantId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var isFavorite = await _context.UserFavoriteServices
                .AnyAsync(f => f.UserId == userId && f.ServiceVariantId == serviceVariantId);

            return Ok(isFavorite);
        }
    }
}
