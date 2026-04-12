using BookLocal.API.DTOs;
using BookLocal.API.Interfaces;
using BookLocal.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BookLocal.API.Services
{
    public class CustomersService : ICustomersService
    {
        private readonly AppDbContext _context;

        public CustomersService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<(bool Success, PagedResultDto<CustomerListItemDto>? Data)> GetCustomersAsync(
            int businessId, ClaimsPrincipal user, string? search, CustomerStatusFilter status, 
            CustomerHistoryFilter history, CustomerSpentFilter spent, int page, int pageSize)
        {
            var ownerId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            var businessExists = await _context.Businesses.AnyAsync(b => b.BusinessId == businessId && b.OwnerId == ownerId);
            if (!businessExists) return (false, null);

            var query = from p in _context.CustomerBusinessProfiles.AsNoTracking()
                        where p.BusinessId == businessId
                        let loyalty = _context.LoyaltyPoints.FirstOrDefault(lp => lp.BusinessId == businessId && lp.CustomerId == p.CustomerId)
                        let nextVisit = _context.Reservations
                                        .Where(r => r.BusinessId == businessId && r.CustomerId == p.CustomerId && r.StartTime > DateTime.UtcNow && r.Status == ReservationStatus.Confirmed)
                                        .OrderBy(r => r.StartTime)
                                        .Select(r => (DateTime?)r.StartTime)
                                        .FirstOrDefault()
                        select new CustomerListItemDto
                        {
                            ProfileId = p.ProfileId,
                            UserId = p.CustomerId,
                            FullName = p.Customer.FirstName + " " + p.Customer.LastName,
                            PhoneNumber = p.Customer.PhoneNumber,
                            Email = p.Customer.Email,
                            PhotoUrl = p.Customer.PhotoUrl,
                            LastVisitDate = p.LastVisitDate,
                            NextVisitDate = nextVisit,
                            CancelledCount = p.CancelledCount,
                            TotalSpent = p.TotalSpent,
                            IsVIP = p.IsVIP,
                            IsBanned = p.IsBanned,
                            PointsBalance = loyalty != null ? loyalty.PointsBalance : 0
                        };

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.ToLower();
                query = query.Where(c =>
                    c.FullName.ToLower().Contains(term) ||
                    (c.Email != null && c.Email.ToLower().Contains(term)) ||
                    (c.PhoneNumber != null && c.PhoneNumber.Contains(term))
                );
            }

            if (status != CustomerStatusFilter.All)
            {
                if (status == CustomerStatusFilter.VIP) query = query.Where(c => c.IsVIP);
                else if (status == CustomerStatusFilter.Banned) query = query.Where(c => c.IsBanned);
                else if (status == CustomerStatusFilter.Standard) query = query.Where(c => !c.IsVIP && !c.IsBanned);
            }

            if (history != CustomerHistoryFilter.All)
            {
                var defaultDate = new DateTime(1, 1, 1, 0, 0, 0);
                if (history == CustomerHistoryFilter.WithHistory)
                    query = query.Where(c => c.LastVisitDate != defaultDate);
                else if (history == CustomerHistoryFilter.WithoutHistory)
                    query = query.Where(c => c.LastVisitDate == defaultDate);
            }

            if (spent != CustomerSpentFilter.All)
            {
                if (spent == CustomerSpentFilter.Any) query = query.Where(c => c.TotalSpent > 0);
                else if (spent == CustomerSpentFilter.Over100) query = query.Where(c => c.TotalSpent >= 100);
                else if (spent == CustomerSpentFilter.Over500) query = query.Where(c => c.TotalSpent >= 500);
                else if (spent == CustomerSpentFilter.Over1000) query = query.Where(c => c.TotalSpent >= 1000);
            }

            var totalCount = await query.CountAsync();

            var customers = await query
                .OrderByDescending(c => c.LastVisitDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var result = new PagedResultDto<CustomerListItemDto>
            {
                Items = customers,
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = pageSize
            };

            return (true, result);
        }

        public async Task<(bool Success, CustomerDetailDto? Data, string? ErrorMessage)> GetCustomerDetailsAsync(
            int businessId, string customerId, ClaimsPrincipal user)
        {
            var ownerId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            var businessExists = await _context.Businesses.AnyAsync(b => b.BusinessId == businessId && b.OwnerId == ownerId);
            if (!businessExists) return (false, null, "Brak dostępu lub firma nie istnieje.");

            var profile = await _context.CustomerBusinessProfiles
                .AsNoTracking()
                .Include(p => p.Customer)
                .FirstOrDefaultAsync(p => p.BusinessId == businessId && p.CustomerId == customerId);

            if (profile == null)
            {
                return (true, null, "Profil klienta nie istnieje w tej firmie.");
            }

            var visitCount = await _context.Reservations
                .CountAsync(r => r.BusinessId == businessId && r.CustomerId == customerId && r.Status == ReservationStatus.Completed);

            var nextVisit = await _context.Reservations
                .AsNoTracking()
                .Where(r => r.BusinessId == businessId && r.CustomerId == customerId && r.StartTime > DateTime.UtcNow && r.Status == ReservationStatus.Confirmed)
                .OrderBy(r => r.StartTime)
                .Select(r => (DateTime?)r.StartTime)
                .FirstOrDefaultAsync();

            var pointsBalance = await _context.LoyaltyPoints
                 .AsNoTracking()
                 .Where(lp => lp.BusinessId == businessId && lp.CustomerId == customerId)
                 .Select(lp => lp.PointsBalance)
                 .FirstOrDefaultAsync();

            var dto = new CustomerDetailDto
            {
                ProfileId = profile.ProfileId,
                UserId = profile.CustomerId,
                FullName = profile.Customer.FirstName + " " + profile.Customer.LastName,
                PhoneNumber = profile.Customer.PhoneNumber,
                Email = profile.Customer.Email,
                PhotoUrl = profile.Customer.PhotoUrl,
                LastVisitDate = profile.LastVisitDate,
                NextVisitDate = nextVisit,
                TotalSpent = profile.TotalSpent,
                PointsBalance = pointsBalance,
                IsVIP = profile.IsVIP,
                IsBanned = profile.IsBanned,
                NoShowCount = profile.NoShowCount,
                PrivateNotes = profile.PrivateNotes,
                Allergies = profile.Allergies,
                Formulas = profile.Formulas,
                VisitCount = visitCount,
                History = new List<ReservationHistoryDto>()
            };

            return (true, dto, null);
        }

        public async Task<(bool Success, PagedResultDto<ReservationHistoryDto>? Data)> GetCustomerHistoryAsync(
            int businessId, string customerId, ClaimsPrincipal user, int page, int pageSize)
        {
            var ownerId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            var businessExists = await _context.Businesses.AnyAsync(b => b.BusinessId == businessId && b.OwnerId == ownerId);
            if (!businessExists) return (false, null);

            var query = _context.Reservations
                .AsNoTracking()
                .Include(r => r.ServiceVariant.Service)
                .Include(r => r.Employee)
                .Include(r => r.ServiceBundle)
                .Where(r => r.BusinessId == businessId && r.CustomerId == customerId);

            var rawReservations = await query
                .OrderByDescending(r => r.StartTime)
                .ToListAsync();

            var groupedHistory = rawReservations
                .GroupBy(r => r.ServiceBundleId.HasValue
                    ? $"Bundle_{r.ServiceBundleId}_{r.StartTime.Date:yyyy-MM-dd}"
                    : $"Single_{r.ReservationId}")
                .Select(g =>
                {
                    var first = g.OrderBy(x => x.StartTime).First();
                    if (first.ServiceBundleId.HasValue && first.ServiceBundle != null)
                    {
                        var empNames = g.Select(x => x.Employee.FirstName + " " + x.Employee.LastName).Distinct();
                        return new ReservationHistoryDto
                        {
                            ReservationId = first.ReservationId,
                            StartTime = first.StartTime,
                            ServiceName = $"[PAKIET] {first.ServiceBundle.Name} ({g.Count()} usługi)",
                            EmployeeName = string.Join(", ", empNames),
                            Price = g.Sum(x => x.AgreedPrice),
                            Status = first.Status.ToString()
                        };
                    }

                    return new ReservationHistoryDto
                    {
                        ReservationId = first.ReservationId,
                        StartTime = first.StartTime,
                        ServiceName = first.ServiceVariant?.Service?.Name + " (" + first.ServiceVariant?.Name + ")",
                        EmployeeName = first.Employee?.FirstName + " " + first.Employee?.LastName,
                        Price = first.AgreedPrice,
                        Status = first.Status.ToString()
                    };
                })
                .OrderByDescending(h => h.StartTime)
                .ToList();

            var totalCount = groupedHistory.Count;
            var pagedItems = groupedHistory
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return (true, new PagedResultDto<ReservationHistoryDto>
            {
                Items = pagedItems,
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = pageSize
            });
        }

        public async Task<bool> UpdateNotesAsync(int businessId, string customerId, UpdateCustomerNoteDto dto, ClaimsPrincipal user)
        {
            var ownerId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            var businessExists = await _context.Businesses.AnyAsync(b => b.BusinessId == businessId && b.OwnerId == ownerId);
            if (!businessExists) return false;

            var profile = await _context.CustomerBusinessProfiles
                .FirstOrDefaultAsync(p => p.BusinessId == businessId && p.CustomerId == customerId);

            if (profile == null) return false;

            profile.PrivateNotes = dto.PrivateNotes;
            profile.Allergies = dto.Allergies;
            profile.Formulas = dto.Formulas;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateStatusAsync(int businessId, string customerId, UpdateCustomerStatusDto dto, ClaimsPrincipal user)
        {
            var ownerId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            var businessExists = await _context.Businesses.AnyAsync(b => b.BusinessId == businessId && b.OwnerId == ownerId);
            if (!businessExists) return false;

            var profile = await _context.CustomerBusinessProfiles
                .FirstOrDefaultAsync(p => p.BusinessId == businessId && p.CustomerId == customerId);

            if (profile == null) return false;

            profile.IsVIP = dto.IsVIP;
            profile.IsBanned = dto.IsBanned;

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
