using BookLocal.API.DTOs;
using BookLocal.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BookLocal.API.Controllers
{
    [ApiController]
    [Route("api/businesses/{businessId}/[controller]")]
    [Authorize(Roles = "owner")]
    public class CustomersController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;

        public CustomersController(AppDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResultDto<CustomerListItemDto>>> GetCustomers(
            int businessId,
            [FromQuery] string? search,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var business = await _context.Businesses.FirstOrDefaultAsync(b => b.BusinessId == businessId && b.OwnerId == ownerId);
            if (business == null) return Forbid();

            var query = from p in _context.CustomerBusinessProfiles
                        where p.BusinessId == businessId
                        let loyalty = _context.LoyaltyPoints.FirstOrDefault(lp => lp.BusinessId == businessId && lp.CustomerId == p.CustomerId)
                        select new CustomerListItemDto
                        {
                            ProfileId = p.ProfileId,
                            UserId = p.CustomerId,
                            FullName = p.Customer.FirstName + " " + p.Customer.LastName,
                            PhoneNumber = p.Customer.PhoneNumber,
                            Email = p.Customer.Email,
                            LastVisitDate = p.LastVisitDate,
                            NextVisitDate = p.NextVisitDate,
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

            return Ok(result);
        }

        [HttpGet("{customerId}")]
        public async Task<ActionResult<CustomerDetailDto>> GetCustomerDetails(int businessId, string customerId)
        {
            var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var business = await _context.Businesses.FirstOrDefaultAsync(b => b.BusinessId == businessId && b.OwnerId == ownerId);
            if (business == null) return Forbid();

            var profile = await _context.CustomerBusinessProfiles
                .Include(p => p.Customer)
                .FirstOrDefaultAsync(p => p.BusinessId == businessId && p.CustomerId == customerId);

            if (profile == null)
            {
                return NotFound("Profil klienta nie istnieje w tej firmie.");
            }

            var visitCount = await _context.Reservations
                .CountAsync(r => r.BusinessId == businessId && r.CustomerId == customerId && r.Status == ReservationStatus.Completed);

            var dto = new CustomerDetailDto
            {
                ProfileId = profile.ProfileId,
                UserId = profile.CustomerId,
                FullName = profile.Customer.FirstName + " " + profile.Customer.LastName,
                PhoneNumber = profile.Customer.PhoneNumber,
                Email = profile.Customer.Email,
                LastVisitDate = profile.LastVisitDate,
                NextVisitDate = await _context.Reservations
                    .Where(r => r.BusinessId == businessId && r.CustomerId == customerId && r.StartTime > DateTime.UtcNow && r.Status == ReservationStatus.Confirmed)
                    .OrderBy(r => r.StartTime)
                    .Select(r => (DateTime?)r.StartTime)
                    .FirstOrDefaultAsync(),
                TotalSpent = profile.TotalSpent,
                PointsBalance = await _context.LoyaltyPoints
                        .Where(lp => lp.BusinessId == businessId && lp.CustomerId == customerId)
                        .Select(lp => lp.PointsBalance)
                        .FirstOrDefaultAsync(),
                IsVIP = profile.IsVIP,
                IsBanned = profile.IsBanned,
                NoShowCount = profile.NoShowCount,
                PrivateNotes = profile.PrivateNotes,
                Allergies = profile.Allergies,
                Formulas = profile.Formulas,
                VisitCount = visitCount,
                History = await _context.Reservations
                    .Where(r => r.BusinessId == businessId && r.CustomerId == customerId)
                    .OrderByDescending(r => r.StartTime)
                    .Select(r => new ReservationHistoryDto
                    {
                        ReservationId = r.ReservationId,
                        StartTime = r.StartTime,
                        ServiceName = r.ServiceVariant.Service.Name + " (" + r.ServiceVariant.Name + ")",
                        EmployeeName = r.Employee.FirstName + " " + r.Employee.LastName,
                        Price = r.AgreedPrice,
                        Status = r.Status.ToString()
                    })
                    .ToListAsync()
            };

            return Ok(dto);
        }

        [HttpPut("{customerId}/notes")]
        public async Task<IActionResult> UpdateNotes(int businessId, string customerId, UpdateCustomerNoteDto dto)
        {
            var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var business = await _context.Businesses.FirstOrDefaultAsync(b => b.BusinessId == businessId && b.OwnerId == ownerId);
            if (business == null) return Forbid();

            var profile = await _context.CustomerBusinessProfiles
                .FirstOrDefaultAsync(p => p.BusinessId == businessId && p.CustomerId == customerId);

            if (profile == null) return NotFound();

            profile.PrivateNotes = dto.PrivateNotes;
            profile.Allergies = dto.Allergies;
            profile.Formulas = dto.Formulas;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPut("{customerId}/status")]
        public async Task<IActionResult> UpdateStatus(int businessId, string customerId, UpdateCustomerStatusDto dto)
        {
            var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var business = await _context.Businesses.FirstOrDefaultAsync(b => b.BusinessId == businessId && b.OwnerId == ownerId);
            if (business == null) return Forbid();

            var profile = await _context.CustomerBusinessProfiles
                .FirstOrDefaultAsync(p => p.BusinessId == businessId && p.CustomerId == customerId);

            if (profile == null) return NotFound();

            profile.IsVIP = dto.IsVIP;
            profile.IsBanned = dto.IsBanned;

            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
