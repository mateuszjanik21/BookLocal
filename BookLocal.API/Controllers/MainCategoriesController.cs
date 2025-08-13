using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class MainCategoriesController : ControllerBase
{
    private readonly AppDbContext _context;

    public MainCategoriesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetMainCategories()
    {
        var categories = await _context.MainCategories
            .AsNoTracking()
            .Select(c => new { c.MainCategoryId, c.Name })
            .OrderBy(c => c.Name)
            .ToListAsync();

        return Ok(categories);
    }
}