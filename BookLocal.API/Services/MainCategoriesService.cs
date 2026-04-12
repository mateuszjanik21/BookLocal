using BookLocal.API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BookLocal.API.Services
{
    public class MainCategoriesService : IMainCategoriesService
    {
        private readonly AppDbContext _context;

        public MainCategoriesService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<object>> GetMainCategoriesAsync()
        {
            var categories = await _context.MainCategories
                .AsNoTracking()
                .Select(c => new { c.MainCategoryId, c.Name })
                .OrderBy(c => c.Name)
                .ToListAsync();

            return categories;
        }
    }
}
