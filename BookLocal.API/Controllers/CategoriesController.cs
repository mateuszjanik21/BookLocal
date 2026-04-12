using BookLocal.API.DTOs;
using BookLocal.API.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BookLocal.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoriesService _categoriesService;

        public CategoriesController(ICategoriesService categoriesService)
        {
            _categoriesService = categoriesService;
        }

        [HttpGet("feed")]
        public async Task<ActionResult<IEnumerable<ServiceCategoryFeedDto>>> GetCategoryFeed()
        {
            var feed = await _categoriesService.GetCategoryFeedAsync();
            return Ok(feed);
        }
    }
}