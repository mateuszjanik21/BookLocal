using BookLocal.API.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class SearchController : ControllerBase
{
    private readonly AppDbContext _context;

    public SearchController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("services")]
    public async Task<IActionResult> SearchServices(
        [FromQuery] string? searchTerm,
        [FromQuery] int? mainCategoryId,
        [FromQuery] string? sortBy,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 12)
    {
        var query = _context.Services
            .Include(s => s.Business).ThenInclude(b => b.Reviews)
            .Include(s => s.ServiceCategory).ThenInclude(sc => sc.MainCategory)
            .AsQueryable();

        if (mainCategoryId.HasValue)
        {
            query = query.Where(s => s.ServiceCategory.MainCategoryId == mainCategoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.ToLower();
            query = query.Where(s =>
                s.Name.ToLower().Contains(term) ||
                s.Business.Name.ToLower().Contains(term) ||
                s.ServiceCategory.Name.ToLower().Contains(term)
            );
        }

        var projectedQuery = query.Select(s => new ServiceSearchResultDto
        {
            ServiceId = s.ServiceId,
            ServiceName = s.Name,
            Price = s.Price,
            DurationMinutes = s.DurationMinutes,
            BusinessId = s.BusinessId,
            BusinessName = s.Business.Name,
            BusinessCity = s.Business.City,
            MainCategoryName = s.ServiceCategory.MainCategory.Name,
            AverageRating = s.Business.Reviews.Any() ? s.Business.Reviews.Average(r => r.Rating) : 0,
            ReviewCount = s.Business.Reviews.Count()
        });

        projectedQuery = sortBy switch
        {
            "price_asc" => projectedQuery.OrderBy(r => r.Price),
            "price_desc" => projectedQuery.OrderByDescending(r => r.Price),
            "rating_desc" => projectedQuery.OrderByDescending(r => r.AverageRating),
            _ => projectedQuery.OrderByDescending(r => r.AverageRating)
        };

        var totalCount = await projectedQuery.CountAsync();

        var items = await projectedQuery
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var pagedResult = new PagedResultDto<ServiceSearchResultDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        return Ok(pagedResult);
    }

    [HttpGet("businesses")]
    public async Task<IActionResult> SearchBusinesses(
    [FromQuery] string? searchTerm,
    [FromQuery] int? mainCategoryId,
    [FromQuery] string? sortBy,
    [FromQuery] int pageNumber = 1,
    [FromQuery] int pageSize = 9) 
    {
        var query = _context.Businesses.AsQueryable();

        if (mainCategoryId.HasValue)
        {
            query = query.Where(b => b.Categories.Any(sc => sc.MainCategoryId == mainCategoryId.Value));
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.ToLower();
            query = query.Where(b =>
                b.Name.ToLower().Contains(term) ||
                (b.City != null && b.City.ToLower().Contains(term))
            );
        }

        var projectedQuery = query.Select(b => new BusinessSearchResultDto
        {
            BusinessId = b.BusinessId,
            Name = b.Name,
            City = b.City,
            PhotoUrl = b.PhotoUrl,
            AverageRating = b.Reviews.Any() ? b.Reviews.Average(r => r.Rating) : 0,
            ReviewCount = b.Reviews.Count(),
            MainCategories = b.Categories.Select(sc => sc.MainCategory.Name).Distinct().ToList()
        });

        projectedQuery = sortBy switch
        {
            "rating_desc" => projectedQuery.OrderByDescending(b => b.AverageRating).ThenByDescending(b => b.ReviewCount),
            _ => projectedQuery.OrderByDescending(b => b.AverageRating).ThenByDescending(b => b.ReviewCount)
        };

        var totalCount = await projectedQuery.CountAsync();

        var items = await projectedQuery
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var pagedResult = new PagedResultDto<BusinessSearchResultDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        return Ok(pagedResult);
    }


    [HttpGet("category-feed")]
    public async Task<IActionResult> SearchCategoryFeed(
    [FromQuery] string? searchTerm,
    [FromQuery] int? mainCategoryId,
    [FromQuery] string? sortBy,
    [FromQuery] int pageNumber = 1,
    [FromQuery] int pageSize = 10)
    {
        var query = _context.ServiceCategories
            .Include(sc => sc.Business).ThenInclude(b => b.Reviews)
            .Include(sc => sc.Services)
            .AsQueryable();

        if (mainCategoryId.HasValue)
        {
            query = query.Where(sc => sc.MainCategoryId == mainCategoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.ToLower();
            query = query.Where(sc =>
                sc.Name.ToLower().Contains(term) ||
                sc.Business.Name.ToLower().Contains(term)
            );
        }

        var projectedQuery = query.Select(sc => new
        {
            ServiceCategory = sc,
            Business = sc.Business,
            AverageRating = sc.Business.Reviews.Any() ? sc.Business.Reviews.Average(r => r.Rating) : 0,
            ReviewCount = sc.Business.Reviews.Count()
        });

        projectedQuery = sortBy switch
        {
            "rating_desc" => projectedQuery.OrderByDescending(x => x.AverageRating).ThenByDescending(x => x.ReviewCount),
            "reviews_desc" => projectedQuery.OrderByDescending(x => x.ReviewCount),
            "newest_desc" => projectedQuery.OrderByDescending(x => x.Business.CreatedAt), 
            "name_asc" => projectedQuery.OrderBy(x => x.Business.Name),
            _ => projectedQuery.OrderByDescending(x => x.AverageRating).ThenByDescending(x => x.ReviewCount)
        };

        var totalCount = await projectedQuery.CountAsync();

        var pagedItems = await projectedQuery
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new ServiceCategorySearchResultDto
            {
                ServiceCategoryId = x.ServiceCategory.ServiceCategoryId,
                Name = x.ServiceCategory.Name,
                PhotoUrl = x.ServiceCategory.PhotoUrl,
                BusinessId = x.ServiceCategory.BusinessId,
                BusinessName = x.ServiceCategory.Business.Name,
                BusinessCity = x.ServiceCategory.Business.City,
                AverageRating = x.AverageRating,
                BusinessCreatedAt = x.Business.CreatedAt,
                ReviewCount = x.ReviewCount,
                Services = x.ServiceCategory.Services.Select(s => new ServiceDto
                {
                    Id = s.ServiceId,
                    Name = s.Name,
                    Price = s.Price,
                    DurationMinutes = s.DurationMinutes,
                    IsArchived = s.IsArchived
                }).ToList()
            })
            .ToListAsync();

        var pagedResult = new PagedResultDto<ServiceCategorySearchResultDto>
        {
            Items = pagedItems,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        return Ok(pagedResult);
    }
}