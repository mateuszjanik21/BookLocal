using BookLocal.API.DTOs;
using BookLocal.API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookLocal.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FavoritesController : ControllerBase
    {
        private readonly IFavoritesService _favoritesService;

        public FavoritesController(IFavoritesService favoritesService)
        {
            _favoritesService = favoritesService;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResultDto<FavoriteServiceDto>>> GetFavorites([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 50)
        {
            var result = await _favoritesService.GetFavoritesAsync(pageNumber, pageSize, User);

            if (!result.Success) return Unauthorized();

            return Ok(result.Data);
        }

        [HttpPost("{serviceVariantId}")]
        public async Task<IActionResult> AddFavorite(int serviceVariantId)
        {
            var result = await _favoritesService.AddFavoriteAsync(serviceVariantId, User);

            if (!result.Success)
            {
                if (result.ErrorMessage == "Brak weryfikacji") return Unauthorized();
                return NotFound(result.ErrorMessage);
            }

            return Ok();
        }

        [HttpDelete("{serviceVariantId}")]
        public async Task<IActionResult> RemoveFavorite(int serviceVariantId)
        {
            var result = await _favoritesService.RemoveFavoriteAsync(serviceVariantId, User);

            if (!result.Success)
            {
                if (result.ErrorMessage == "Brak autoryzacji") return Unauthorized();
                return NotFound(result.ErrorMessage);
            }

            return NoContent();
        }

        [HttpGet("check/{serviceVariantId}")]
        public async Task<ActionResult<bool>> IsFavorite(int serviceVariantId)
        {
            var result = await _favoritesService.IsFavoriteAsync(serviceVariantId, User);

            if (!result.Success) return Unauthorized();

            return Ok(result.IsFavorite);
        }
    }
}
