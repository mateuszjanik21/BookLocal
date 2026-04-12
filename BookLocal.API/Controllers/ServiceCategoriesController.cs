using BookLocal.API.DTOs;
using BookLocal.API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookLocal.API.Controllers
{
    [ApiController]
    [Route("api/businesses/{businessId}/categories")]
    [Authorize(Roles = "owner")]
    public class ServiceCategoriesController : ControllerBase
    {
        private readonly IServiceCategoriesService _serviceCategoriesService;

        public ServiceCategoriesController(IServiceCategoriesService serviceCategoriesService)
        {
            _serviceCategoriesService = serviceCategoriesService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ServiceCategoryDto>>> GetCategories(int businessId, [FromQuery] bool includeArchived = false)
        {
            var result = await _serviceCategoriesService.GetCategoriesAsync(businessId, includeArchived, User);

            if (!result.Success) return Forbid();

            return Ok(result.Data);
        }

        [HttpPost]
        public async Task<ActionResult<ServiceCategoryDto>> CreateCategory(int businessId, ServiceCategoryUpsertDto categoryDto)
        {
            var result = await _serviceCategoriesService.CreateCategoryAsync(businessId, categoryDto, User);

            if (!result.Success) return Forbid();

            return CreatedAtAction(nameof(GetCategories), new { businessId }, result.Data);
        }

        [HttpPut("{categoryId}")]
        public async Task<IActionResult> UpdateCategory(int businessId, int categoryId, ServiceCategoryUpsertDto categoryDto)
        {
            var result = await _serviceCategoriesService.UpdateCategoryAsync(businessId, categoryId, categoryDto, User);

            if (!result.Success) return NotFound();

            return NoContent();
        }

        [HttpDelete("{categoryId}")]
        public async Task<IActionResult> DeleteCategory(int businessId, int categoryId)
        {
            var result = await _serviceCategoriesService.DeleteCategoryAsync(businessId, categoryId, User);

            if (!result.Success) return NotFound();

            return NoContent();
        }

        [HttpPatch("{categoryId}/restore")]
        public async Task<IActionResult> RestoreCategory(int businessId, int categoryId)
        {
            var result = await _serviceCategoriesService.RestoreCategoryAsync(businessId, categoryId, User);

            if (!result.Success) return NotFound();

            return NoContent();
        }
    }
}