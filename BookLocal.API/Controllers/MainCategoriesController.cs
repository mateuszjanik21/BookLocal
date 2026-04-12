using BookLocal.API.Interfaces;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class MainCategoriesController : ControllerBase
{
    private readonly IMainCategoriesService _mainCategoriesService;

    public MainCategoriesController(IMainCategoriesService mainCategoriesService)
    {
        _mainCategoriesService = mainCategoriesService;
    }

    [HttpGet]
    public async Task<IActionResult> GetMainCategories()
    {
        var categories = await _mainCategoriesService.GetMainCategoriesAsync();
        return Ok(categories);
    }
}