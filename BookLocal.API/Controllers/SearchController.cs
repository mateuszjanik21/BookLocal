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
        [FromQuery] string? locationTerm,
        [FromQuery] int? mainCategoryId,
        [FromQuery] string? sortBy,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 12)
    {
        var query = _context.Services
            .Include(s => s.Variants)
            .Include(s => s.Business)
                .ThenInclude(b => b.Reviews)
            .Include(s => s.ServiceCategory)
                .ThenInclude(sc => sc.MainCategory)
            .Where(s => !s.IsArchived && s.Variants.Any(v => v.IsActive) && _context.EmployeeServices.Any(es => es.ServiceId == s.ServiceId))
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

        if (!string.IsNullOrWhiteSpace(locationTerm))
        {
            var lTerm = locationTerm.ToLower();
            query = query.Where(s => (s.Business.City != null && s.Business.City.ToLower().Contains(lTerm)) ||
                                     (s.Business.Address != null && s.Business.Address.ToLower().Contains(lTerm)));
        }

        var projectedQuery = query.Select(s => new ServiceSearchResultDto
        {
            ServiceId = s.ServiceId,
            DefaultServiceVariantId = s.Variants.OrderBy(v => v.Price).First().ServiceVariantId,
            ServiceName = s.Name,
            Price = s.Variants.Min(v => v.Price),
            DurationMinutes = s.Variants.OrderBy(v => v.Price).First().DurationMinutes,
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
    [FromQuery] string? locationTerm,
    [FromQuery] int? mainCategoryId,
    [FromQuery] string? sortBy,
    [FromQuery] int pageNumber = 1,
    [FromQuery] int pageSize = 12)
    {
        var query = _context.Businesses.AsQueryable();

        if (mainCategoryId.HasValue)
        {
            query = query.Where(b => b.Categories.Any(sc => sc.MainCategoryId == mainCategoryId.Value));
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.ToLower();
            query = query.Where(b => b.Name.ToLower().Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(locationTerm))
        {
            var lTerm = locationTerm.ToLower();
            query = query.Where(b => (b.City != null && b.City.ToLower().Contains(lTerm)) ||
                                     (b.Address != null && b.Address.ToLower().Contains(lTerm)));
        }

        var projectedQuery = query.Select(b => new BusinessSearchResultDto
        {
            BusinessId = b.BusinessId,
            Name = b.Name,
            City = b.City,
            PhotoUrl = b.PhotoUrl,
            AverageRating = b.Reviews.Any() ? b.Reviews.Average(r => r.Rating) : 0,
            ReviewCount = b.Reviews.Count(),
            IsVerified = b.IsVerified,
            SubscriptionPlanName = _context.BusinessSubscriptions.Where(s => s.BusinessId == b.BusinessId && s.IsActive && s.EndDate > DateTime.UtcNow).Select(s => s.Plan.Name).FirstOrDefault(),
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
    [FromQuery] string? locationTerm,
    [FromQuery] int? mainCategoryId,
    [FromQuery] string? sortBy,
    [FromQuery] int pageNumber = 1,
    [FromQuery] int pageSize = 10)
    {
        var query = _context.ServiceCategories
            .AsNoTracking()
            .Where(sc => sc.Services.Any(s => !s.IsArchived && _context.EmployeeServices.Any(es => es.ServiceId == s.ServiceId)))
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
                sc.Business.Name.ToLower().Contains(term) ||
                sc.Services.Any(s => s.Name.ToLower().Contains(term) && !s.IsArchived)
            );
        }

        if (!string.IsNullOrWhiteSpace(locationTerm))
        {
            var lTerm = locationTerm.ToLower();
            query = query.Where(sc => (sc.Business.City != null && sc.Business.City.ToLower().Contains(lTerm)) ||
                                      (sc.Business.Address != null && sc.Business.Address.ToLower().Contains(lTerm)));
        }

        var projectedQuery = query.Select(sc => new ServiceCategorySearchResultDto
        {
            ServiceCategoryId = sc.ServiceCategoryId,
            Name = sc.Name,
            PhotoUrl = sc.PhotoUrl,
            BusinessId = sc.BusinessId,
            BusinessName = sc.Business.Name,
            BusinessCity = sc.Business.City,
            MainCategoryName = sc.MainCategory != null ? sc.MainCategory.Name : null,
            AverageRating = sc.Business.Reviews.Any() ? sc.Business.Reviews.Average(r => r.Rating) : 0,
            ReviewCount = sc.Business.Reviews.Count(),
            BusinessCreatedAt = sc.Business.CreatedAt,
            Services = sc.Services
                .Where(s => !s.IsArchived && _context.EmployeeServices.Any(es => es.ServiceId == s.ServiceId))
                .Select(s => new ServiceDto
                {
                    Id = s.ServiceId,
                    Name = s.Name,
                    Description = s.Description,
                    ServiceCategoryId = s.ServiceCategoryId,
                    IsArchived = s.IsArchived,
                    Variants = s.Variants.Select(v => new ServiceVariantDto
                    {
                        ServiceVariantId = v.ServiceVariantId,
                        Name = v.Name,
                        Price = v.Price,
                        DurationMinutes = v.DurationMinutes,
                        IsDefault = v.IsDefault
                    }).ToList()
                }).ToList()
        });

        IOrderedQueryable<ServiceCategorySearchResultDto> sortedQuery = sortBy switch
        {
            "rating_desc" => projectedQuery.OrderByDescending(x => x.AverageRating).ThenByDescending(x => x.ReviewCount),
            "reviews_desc" => projectedQuery.OrderByDescending(x => x.ReviewCount),
            "newest_desc" => projectedQuery.OrderByDescending(x => x.BusinessCreatedAt),
            "name_asc" => projectedQuery.OrderBy(x => x.BusinessName),
            _ => projectedQuery.OrderByDescending(x => x.AverageRating).ThenByDescending(x => x.ReviewCount)
        };

        var totalCount = await projectedQuery.CountAsync();

        var resultItems = await sortedQuery
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var pagedResult = new PagedResultDto<ServiceCategorySearchResultDto>
        {
            Items = resultItems,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        return Ok(pagedResult);
    }

    [HttpGet("rebook-suggestions")]
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "customer")]
    public async Task<IActionResult> GetRebookSuggestions()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var suggestions = await _context.Reservations
            .AsNoTracking()
            .Where(r => r.CustomerId == userId && r.Status == ReservationStatus.Completed)
            .GroupBy(r => new
            {
                r.ServiceVariant.Service.ServiceCategoryId,
                CategoryName = r.ServiceVariant.Service.ServiceCategory.Name,
                CategoryPhotoUrl = r.ServiceVariant.Service.ServiceCategory.PhotoUrl,
                r.BusinessId,
                BusinessName = r.Business.Name,
                BusinessCity = r.Business.City
            })
            .Select(g => new RebookSuggestionDto
            {
                ServiceCategoryId = g.Key.ServiceCategoryId,
                CategoryName = g.Key.CategoryName,
                CategoryPhotoUrl = g.Key.CategoryPhotoUrl,
                BusinessId = g.Key.BusinessId,
                BusinessName = g.Key.BusinessName,
                BusinessCity = g.Key.BusinessCity,
                EmployeeName = g.OrderByDescending(r => r.StartTime).First().Employee.FirstName + " " + g.OrderByDescending(r => r.StartTime).First().Employee.LastName,
                EmployeePhotoUrl = g.OrderByDescending(r => r.StartTime).First().Employee.PhotoUrl,
                LastVisitDate = g.Max(r => r.StartTime),
                VisitCount = g.Count()
            })
            .OrderByDescending(s => s.LastVisitDate)
            .Take(8)
            .ToListAsync();

        return Ok(suggestions);
    }
}