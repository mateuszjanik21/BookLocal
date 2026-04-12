using BookLocal.API.DTOs;
using BookLocal.API.Interfaces;
using BookLocal.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace BookLocal.API.Services
{
    public class CategoriesService : ICategoriesService
    {
        private readonly AppDbContext _context;

        public CategoriesService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ServiceCategoryFeedDto>> GetCategoryFeedAsync()
        {
            var assignedServiceIds = await _context.EmployeeServices
                .Select(es => es.ServiceId)
                .Distinct()
                .ToListAsync();

            return await _context.ServiceCategories
                .AsNoTracking()
                .Where(sc => sc.Services.Any(s =>
                    !s.IsArchived &&
                    s.Variants.Any(v => v.IsActive) &&
                    assignedServiceIds.Contains(s.ServiceId)))
                .Select(sc => new ServiceCategoryFeedDto
                {
                    ServiceCategoryId = sc.ServiceCategoryId,
                    Name = sc.Name ?? string.Empty,
                    PhotoUrl = sc.PhotoUrl,
                    BusinessId = sc.BusinessId,
                    BusinessName = sc.Business != null ? (sc.Business.Name ?? string.Empty) : string.Empty,
                    BusinessCity = sc.Business != null ? sc.Business.City : string.Empty,
                    Services = sc.Services
                        .Where(s => !s.IsArchived && assignedServiceIds.Contains(s.ServiceId))
                        .Select(s => new ServiceDto
                        {
                            Id = s.ServiceId,
                            Name = s.Name,
                            Description = s.Description,
                            ServiceCategoryId = s.ServiceCategoryId,
                            IsArchived = s.IsArchived,
                            Variants = s.Variants
                                .Where(v => v.IsActive)
                                .Select(v => new ServiceVariantDto
                                {
                                    ServiceVariantId = v.ServiceVariantId,
                                    Name = v.Name,
                                    Price = v.Price,
                                    DurationMinutes = v.DurationMinutes,
                                    IsDefault = v.IsDefault
                                }).ToList()
                        }).ToList()
                })
                .ToListAsync();
        }
    }
}
