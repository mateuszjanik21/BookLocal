using BookLocal.API.DTOs;
using BookLocal.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BookLocal.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BusinessesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public BusinessesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<BusinessSearchResultDto>>> GetAllBusinesses([FromQuery] string? searchQuery)
        {
            var query = _context.Businesses
                .Include(b => b.Reviews)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                var searchTerm = searchQuery.ToLower();
                query = query.Where(b =>
                    b.Name.ToLower().Contains(searchTerm) ||
                    (b.City != null && b.City.ToLower().Contains(searchTerm))
                );
            }

            var verifiedIds = await _context.BusinessVerifications
                .Where(v => v.Status == VerificationStatus.Approved)
                .Select(v => v.BusinessId)
                .ToListAsync();

            var businesses = await query
                .Select(b => new BusinessSearchResultDto
                {
                    BusinessId = b.BusinessId,
                    Name = b.Name,
                    City = b.City,
                    PhotoUrl = b.PhotoUrl,
                    AverageRating = b.Reviews.Any() ? b.Reviews.Average(r => r.Rating) : 0,
                    ReviewCount = b.Reviews.Count,
                    IsVerified = false
                })
                .ToListAsync();

            foreach (var b in businesses)
            {
                b.IsVerified = verifiedIds.Contains(b.BusinessId);
            }

            return Ok(businesses);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<BusinessDetailDto>> GetBusiness(int id)
        {
            var business = await _context.Businesses
                .Include(b => b.Reviews)
                .Include(b => b.Categories)
                    .ThenInclude(c => c.Services)
                        .ThenInclude(s => s.Variants)
                .Include(b => b.Employees)
                    .ThenInclude(e => e.FinanceSettings)
                .Include(b => b.Owner)
                .FirstOrDefaultAsync(b => b.BusinessId == id);

            if (business == null) return NotFound();

            var isVerified = await _context.BusinessVerifications
                .AnyAsync(v => v.BusinessId == id && v.Status == VerificationStatus.Approved);

            var businessDto = new BusinessDetailDto
            {
                Id = business.BusinessId,
                Name = business.Name,
                NIP = business.NIP,
                Address = business.Address,
                City = business.City,
                Description = business.Description,
                PhotoUrl = business.PhotoUrl,
                IsVerified = isVerified,
                AverageRating = business.Reviews.Any() ? business.Reviews.Average(r => r.Rating) : 0,
                ReviewCount = business.Reviews.Count,
                Owner = new OwnerDto { FirstName = business.Owner?.FirstName ?? "Właściciel" },

                Categories = business.Categories.Select(c => new ServiceCategoryDto
                {
                    ServiceCategoryId = c.ServiceCategoryId,
                    Name = c.Name,
                    PhotoUrl = c.PhotoUrl,
                    Services = c.Services
                        .Where(s => !s.IsArchived)
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
                                CleanupTimeMinutes = v.CleanupTimeMinutes,
                                IsDefault = v.IsDefault
                            }).ToList()
                        }).ToList()
                }).ToList(),

                Employees = business.Employees.Select(e => new EmployeeDto
                {
                    Id = e.EmployeeId,
                    FirstName = e.FirstName,
                    LastName = e.LastName,
                    Position = e.Position,
                    PhotoUrl = e.PhotoUrl,
                    DateOfBirth = e.DateOfBirth,
                    Specialization = e.EmployeeDetails != null ? e.EmployeeDetails.Specialization : null,
                    InstagramProfileUrl = e.EmployeeDetails != null ? e.EmployeeDetails.InstagramProfileUrl : null,
                    PortfolioUrl = e.EmployeeDetails != null ? e.EmployeeDetails.PortfolioUrl : null,
                    IsStudent = e.FinanceSettings != null ? e.FinanceSettings.IsStudent : false
                }).ToList()
            };

            return Ok(businessDto);
        }

        [HttpGet("my-business")]
        [Authorize(Roles = "owner")]
        public async Task<ActionResult<BusinessDetailDto>> GetMyBusiness()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var business = await _context.Businesses
                .Include(b => b.Reviews)
                .Include(b => b.Categories)
                    .ThenInclude(c => c.Services)
                        .ThenInclude(s => s.Variants)
                .Include(b => b.Employees)
                    .ThenInclude(e => e.FinanceSettings)
                .Include(b => b.Owner)
                .FirstOrDefaultAsync(b => b.OwnerId == userId);

            if (business == null)
            {
                return NotFound("Nie znaleziono firmy dla tego właściciela.");
            }

            var isVerified = await _context.BusinessVerifications
                .AnyAsync(v => v.BusinessId == business.BusinessId && v.Status == VerificationStatus.Approved);

            var businessDto = new BusinessDetailDto
            {
                Id = business.BusinessId,
                Name = business.Name,
                NIP = business.NIP,
                Address = business.Address,
                City = business.City,
                Description = business.Description,
                PhotoUrl = business.PhotoUrl,
                IsVerified = isVerified,
                AverageRating = business.Reviews.Any() ? business.Reviews.Average(r => r.Rating) : 0,
                ReviewCount = business.Reviews.Count,
                Owner = new OwnerDto { FirstName = business.Owner?.FirstName },

                Categories = business.Categories.Select(c => new ServiceCategoryDto
                {
                    ServiceCategoryId = c.ServiceCategoryId,
                    Name = c.Name,
                    PhotoUrl = c.PhotoUrl,
                    Services = c.Services
                        .Where(s => !s.IsArchived)
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
                }).ToList(),

                Employees = business.Employees.Select(e => new EmployeeDto
                {
                    Id = e.EmployeeId,
                    FirstName = e.FirstName,
                    LastName = e.LastName,
                    Position = e.Position,
                    PhotoUrl = e.PhotoUrl,
                    DateOfBirth = e.DateOfBirth,
                    Specialization = e.EmployeeDetails != null ? e.EmployeeDetails.Specialization : null,
                    InstagramProfileUrl = e.EmployeeDetails != null ? e.EmployeeDetails.InstagramProfileUrl : null,
                    PortfolioUrl = e.EmployeeDetails != null ? e.EmployeeDetails.PortfolioUrl : null,
                    IsStudent = e.FinanceSettings != null ? e.FinanceSettings.IsStudent : false
                }).ToList()
            };

            return Ok(businessDto);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "owner")]
        public async Task<IActionResult> UpdateBusiness(int id, BusinessDto businessDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var business = await _context.Businesses.FindAsync(id);

            if (business == null) return NotFound();
            if (business.OwnerId != userId) return Forbid();

            business.Name = businessDto.Name;
            business.NIP = businessDto.NIP;
            business.Address = businessDto.Address;
            business.City = businessDto.City;
            business.Description = businessDto.Description;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "owner")]
        public async Task<IActionResult> DeleteBusiness(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var business = await _context.Businesses.FindAsync(id);

            if (business == null) return NotFound();
            if (business.OwnerId != userId) return Forbid();

            _context.Businesses.Remove(business);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}