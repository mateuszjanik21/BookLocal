using BookLocal.API.DTOs;
using BookLocal.API.Interfaces;
using BookLocal.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BookLocal.API.Services
{
    public class BusinessService : IBusinessService
    {
        private readonly AppDbContext _context;

        public BusinessService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<BusinessSearchResultDto>> GetAllBusinessesAsync(string? searchQuery)
        {
            var query = _context.Businesses
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                var searchTerm = searchQuery.ToLower();
                query = query.Where(b =>
                    (b.Name != null && b.Name.ToLower().Contains(searchTerm)) ||
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
                    Name = b.Name ?? string.Empty,
                    City = b.City ?? string.Empty,
                    PhotoUrl = b.PhotoUrl,
                    AverageRating = b.Reviews.Any() ? b.Reviews.Average(r => r.Rating) : 0,
                    ReviewCount = b.Reviews.Count,
                    PhoneNumber = b.Owner != null ? b.Owner.PhoneNumber : null,
                    IsVerified = false,
                    CreatedAt = b.CreatedAt
                })
                .ToListAsync();

            foreach (var b in businesses)
            {
                b.IsVerified = verifiedIds.Contains(b.BusinessId);
            }

            return businesses;
        }

        public async Task<BusinessDetailDto?> GetBusinessAsync(int id)
        {
            var business = await _context.Businesses
                .AsNoTracking()
                .Include(b => b.Reviews)
                .Include(b => b.Categories)
                    .ThenInclude(c => c.Services)
                        .ThenInclude(s => s.Variants)
                .Include(b => b.Employees)
                    .ThenInclude(e => e.FinanceSettings)
                .Include(b => b.Employees)
                    .ThenInclude(e => e.EmployeeDetails)
                .Include(b => b.Employees)
                    .ThenInclude(e => e.EmployeeServices)
                .Include(b => b.Owner)
                .FirstOrDefaultAsync(b => b.BusinessId == id);

            if (business == null) return null;

            var isVerified = await _context.BusinessVerifications
                .AnyAsync(v => v.BusinessId == id && v.Status == VerificationStatus.Approved);

            var serviceIdsWithEmployees = await _context.EmployeeServices
                .Include(es => es.Employee)
                .Where(es => es.Service.BusinessId == id && !es.Employee.IsArchived)
                .Select(es => es.ServiceId)
                .Distinct()
                .ToListAsync();

            var businessDto = new BusinessDetailDto
            {
                Id = business.BusinessId,
                Name = business.Name ?? string.Empty,
                NIP = business.NIP ?? string.Empty,
                Address = business.Address ?? string.Empty,
                City = business.City ?? string.Empty,
                Description = business.Description ?? string.Empty,
                PhotoUrl = business.PhotoUrl,
                PhoneNumber = business.Owner?.PhoneNumber,
                IsVerified = isVerified,
                AverageRating = business.Reviews.Any() ? business.Reviews.Average(r => r.Rating) : 0,
                ReviewCount = business.Reviews.Count,
                Owner = new OwnerDto
                {
                    FirstName = business.Owner?.FirstName ?? "Właściciel",
                    LastName = business.Owner?.LastName
                },

                Categories = business.Categories.Select(c => new ServiceCategoryDto
                {
                    ServiceCategoryId = c.ServiceCategoryId,
                    Name = c.Name,
                    PhotoUrl = c.PhotoUrl,
                    Services = c.Services
                        .Where(s => !s.IsArchived && serviceIdsWithEmployees.Contains(s.ServiceId) && s.Variants.Any(v => v.IsActive))
                        .Select(s => new ServiceDto
                        {
                            Id = s.ServiceId,
                            Name = s.Name,
                            Description = s.Description,
                            ServiceCategoryId = s.ServiceCategoryId,
                            IsArchived = s.IsArchived,
                            Variants = s.Variants.Where(v => v.IsActive).Select(v => new ServiceVariantDto
                            {
                                ServiceVariantId = v.ServiceVariantId,
                                Name = v.Name,
                                Price = v.Price,
                                DurationMinutes = v.DurationMinutes,
                                CleanupTimeMinutes = v.CleanupTimeMinutes,
                                IsDefault = v.IsDefault,
                                FavoritesCount = v.UserFavoriteServices != null ? v.UserFavoriteServices.Count : 0
                            }).ToList()
                        }).ToList()
                }).Where(c => c.Services.Any()).ToList(),

                Employees = business.Employees.Where(e => !e.IsArchived).Select(e => new EmployeeDto
                {
                    Id = e.EmployeeId,
                    FirstName = e.FirstName,
                    LastName = e.LastName,
                    Position = e.Position,
                    PhotoUrl = e.PhotoUrl,
                    DateOfBirth = e.DateOfBirth,
                    Specialization = e.EmployeeDetails != null ? e.EmployeeDetails.Specialization : null,
                    Bio = e.EmployeeDetails != null ? e.EmployeeDetails.Bio : null,
                    InstagramProfileUrl = e.EmployeeDetails != null ? e.EmployeeDetails.InstagramProfileUrl : null,
                    PortfolioUrl = e.EmployeeDetails != null ? e.EmployeeDetails.PortfolioUrl : null,
                    IsStudent = e.FinanceSettings != null ? e.FinanceSettings.IsStudent : false,
                    AssignedServiceIds = e.EmployeeServices.Select(es => es.ServiceId).ToList()
                }).ToList()
            };

            var employeeIds = business.Employees.Select(e => e.EmployeeId).ToList();
            var certificates = await _context.Set<BookLocal.Data.Models.EmployeeCertificate>()
                .AsNoTracking()
                .Where(c => employeeIds.Contains(c.EmployeeId) && c.IsVisibleToClient)
                .ToListAsync();

            foreach (var empDto in businessDto.Employees)
            {
                empDto.Certificates = certificates
                    .Where(c => c.EmployeeId == empDto.Id)
                    .Select(c => new EmployeeCertificateDto
                    {
                        CertificateId = c.CertificateId,
                        Name = c.Name,
                        Institution = c.Institution,
                        DateObtained = c.DateObtained,
                        ImageUrl = c.ImageUrl,
                        IsVisibleToClient = c.IsVisibleToClient
                    }).ToList();
            }

            return businessDto;
        }

        public async Task<(bool Success, BusinessDetailDto? Data, string? ErrorMessage)> GetMyBusinessAsync(ClaimsPrincipal user)
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);

            var business = await _context.Businesses
                .AsNoTracking()
                .Include(b => b.Reviews)
                .Include(b => b.Categories)
                    .ThenInclude(c => c.Services)
                        .ThenInclude(s => s.Variants)
                            .ThenInclude(v => v.UserFavoriteServices)
                .Include(b => b.Employees)
                    .ThenInclude(e => e.FinanceSettings)
                .Include(b => b.Employees)
                    .ThenInclude(e => e.EmployeeServices)
                .Include(b => b.Employees)
                    .ThenInclude(e => e.Contracts)
                .Include(b => b.Employees)
                    .ThenInclude(e => e.Reservations)
                .Include(b => b.Employees)
                    .ThenInclude(e => e.EmployeeDetails)
                .Include(b => b.Owner)
                .FirstOrDefaultAsync(b => b.OwnerId == userId);

            if (business == null)
            {
                return (false, null, "Nie znaleziono firmy dla tego właściciela.");
            }

            var isVerified = await _context.BusinessVerifications
                .AnyAsync(v => v.BusinessId == business.BusinessId && v.Status == VerificationStatus.Approved);

            var now = DateTime.Now;
            var currentMonthStart = new DateTime(now.Year, now.Month, 1);
            var currentMonthEnd = currentMonthStart.AddMonths(1);

            var businessDto = new BusinessDetailDto
            {
                Id = business.BusinessId,
                Name = business.Name ?? string.Empty,
                NIP = business.NIP ?? string.Empty,
                Address = business.Address ?? string.Empty,
                City = business.City ?? string.Empty,
                Description = business.Description ?? string.Empty,
                PhotoUrl = business.PhotoUrl,
                PhoneNumber = business.Owner?.PhoneNumber,
                IsVerified = isVerified,
                AverageRating = business.Reviews.Any() ? business.Reviews.Average(r => r.Rating) : 0,
                ReviewCount = business.Reviews.Count,
                Owner = new OwnerDto
                {
                    FirstName = business.Owner?.FirstName ?? string.Empty,
                    LastName = business.Owner?.LastName ?? string.Empty
                },

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
                                IsDefault = v.IsDefault,
                                FavoritesCount = v.UserFavoriteServices != null ? v.UserFavoriteServices.Count : 0
                            }).ToList()
                        }).ToList()
                }).ToList(),

                Employees = business.Employees.Select(e => {
                    var activeContract = e.Contracts
                        .Where(c => c.IsActive)
                        .OrderByDescending(c => c.StartDate)
                        .FirstOrDefault();

                    var completedThisMonth = e.Reservations
                        .Where(r => r.Status == ReservationStatus.Completed
                            && r.StartTime >= currentMonthStart
                            && r.StartTime < currentMonthEnd)
                        .ToList();

                    return new EmployeeDto
                    {
                        Id = e.EmployeeId,
                        FirstName = e.FirstName,
                        LastName = e.LastName,
                        Position = e.Position,
                        PhotoUrl = e.PhotoUrl,
                        DateOfBirth = e.DateOfBirth,
                        Specialization = e.EmployeeDetails != null ? e.EmployeeDetails.Specialization : null,
                        Bio = e.EmployeeDetails != null ? e.EmployeeDetails.Bio : null,
                        InstagramProfileUrl = e.EmployeeDetails != null ? e.EmployeeDetails.InstagramProfileUrl : null,
                        PortfolioUrl = e.EmployeeDetails != null ? e.EmployeeDetails.PortfolioUrl : null,
                        IsStudent = e.FinanceSettings != null ? e.FinanceSettings.IsStudent : false,
                        IsArchived = e.IsArchived,
                        AssignedServicesCount = e.EmployeeServices.Count,
                        CompletedReservationsCount = completedThisMonth.Count,
                        ActiveContractType = activeContract?.ContractType.ToString(),
                        EstimatedMonthlyRevenue = completedThisMonth.Sum(r => r.AgreedPrice),
                        AssignedServiceIds = e.EmployeeServices.Select(es => es.ServiceId).ToList()
                    };
                }).ToList()
            };

            return (true, businessDto, null);
        }

        public async Task<bool> UpdateBusinessAsync(int id, BusinessDto businessDto, ClaimsPrincipal user)
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            var business = await _context.Businesses.FindAsync(id);

            if (business == null || business.OwnerId != userId) return false;

            business.Name = businessDto.Name;
            business.NIP = businessDto.NIP;
            business.Address = businessDto.Address;
            business.City = businessDto.City;
            business.Description = businessDto.Description;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteBusinessAsync(int id, ClaimsPrincipal user)
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            var business = await _context.Businesses.FindAsync(id);

            if (business == null || business.OwnerId != userId) return false;

            _context.Businesses.Remove(business);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<DashboardDataDto> GetDashboardDataAsync(ClaimsPrincipal user)
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            var ownerParam = new Microsoft.Data.SqlClient.SqlParameter("@OwnerId", userId);

            var statsResult = await _context.Database
                .SqlQueryRaw<DashboardStatsSqlDto>("EXEC GetDashboardStats @OwnerId", ownerParam)
                .ToListAsync();

            var stats = statsResult.FirstOrDefault() ?? new DashboardStatsSqlDto();

            ownerParam = new Microsoft.Data.SqlClient.SqlParameter("@OwnerId", userId);
            var todaysReservations = await _context.Database
                .SqlQueryRaw<ReservationSqlDto>("EXEC GetTodaysReservations @OwnerId", ownerParam)
                .ToListAsync();

            ownerParam = new Microsoft.Data.SqlClient.SqlParameter("@OwnerId", userId);
            var latestReviewsSql = await _context.Database
                .SqlQueryRaw<ReviewSqlDto>("EXEC GetLatestReviews @OwnerId", ownerParam)
                .ToListAsync();

            return new DashboardDataDto
            {
                Stats = new DashboardStatsDto
                {
                    UpcomingReservationsCount = stats.UpcomingReservationsCount,
                    ClientCount = stats.ClientCount,
                    EmployeeCount = stats.EmployeeCount,
                    ServiceCount = stats.ServiceCount,
                    HasVariants = stats.HasVariants
                },
                TodaysReservations = todaysReservations.Select(r => new ReservationDto
                {
                    ReservationId = r.ReservationId,
                    StartTime = r.StartTime,
                    EndTime = r.EndTime,
                    Status = r.Status,
                    ServiceVariantId = r.ServiceVariantId,
                    ServiceName = r.ServiceName,
                    VariantName = r.VariantName,
                    AgreedPrice = r.AgreedPrice,
                    BusinessName = r.BusinessName,
                    BusinessId = r.BusinessId,
                    EmployeeId = r.EmployeeId,
                    EmployeeFullName = r.EmployeeFullName ?? string.Empty,
                    CustomerId = r.CustomerId ?? string.Empty,
                    CustomerFullName = r.CustomerFullName ?? string.Empty,
                    GuestName = r.GuestName ?? string.Empty,
                    HasReview = r.HasReview,
                    PaymentMethod = r.PaymentMethod
                }).ToList(),
                LatestReviews = latestReviewsSql.Select(r => new ReviewDto
                {
                    ReviewId = r.ReviewId,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    ReviewerName = r.ReviewerName,
                    CreatedAt = r.CreatedAt
                }).ToList()
            };
        }
    }
}
