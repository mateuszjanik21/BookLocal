using BookLocal.API.DTOs;
using BookLocal.API.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BookLocal.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SearchController : ControllerBase
    {
        private readonly ISearchService _searchService;

        public SearchController(ISearchService searchService)
        {
            _searchService = searchService;
        }

        [HttpGet("services")]
        public async Task<IActionResult> SearchServices(
            [FromQuery] string? searchTerm,
            [FromQuery] string? locationTerm,
            [FromQuery] int? mainCategoryId,
            [FromQuery] string? sortBy,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 12)
        {
            var result = await _searchService.SearchServicesAsync(searchTerm, locationTerm, mainCategoryId, sortBy, pageNumber, pageSize);
            return Ok(result.Data);
        }

        [HttpGet("businesses")]
        public async Task<IActionResult> SearchBusinesses(
            [FromQuery] string? searchTerm,
            [FromQuery] string? locationTerm,
            [FromQuery] int? mainCategoryId,
            [FromQuery] string? sortBy,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 12)
        {
            var result = await _searchService.SearchBusinessesAsync(searchTerm, locationTerm, mainCategoryId, sortBy, pageNumber, pageSize);
            return Ok(result.Data);
        }

        [HttpGet("category-feed")]
        public async Task<IActionResult> SearchCategoryFeed(
            [FromQuery] string? searchTerm,
            [FromQuery] string? locationTerm,
            [FromQuery] int? mainCategoryId,
            [FromQuery] string? sortBy,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = await _searchService.SearchCategoryFeedAsync(searchTerm, locationTerm, mainCategoryId, sortBy, pageNumber, pageSize);
            return Ok(result.Data);
        }

        [HttpGet("rebook-suggestions")]
        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "customer")]
        public async Task<IActionResult> GetRebookSuggestions()
        {
            var result = await _searchService.GetRebookSuggestionsAsync(User);

            if (!result.Success) return Unauthorized(result.ErrorMessage);

            return Ok(result.Data);
        }
    }
}