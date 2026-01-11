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
        public async Task<ActionResult<IEnumerable<CustomerListItemDto>>> GetCustomers(int businessId, [FromQuery] string? search)
        {
            var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            // Verify access
            var business = await _context.Businesses.FirstOrDefaultAsync(b => b.BusinessId == businessId && b.OwnerId == ownerId);
            if (business == null) return Forbid();

            var query = _context.CustomerBusinessProfiles
                .Include(p => p.Customer)
                .Where(p => p.BusinessId == businessId)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.ToLower();
                query = query.Where(p =>
                    (p.Customer.FirstName + " " + p.Customer.LastName).ToLower().Contains(term) ||
                    (p.Customer.Email != null && p.Customer.Email.ToLower().Contains(term)) ||
                    (p.Customer.PhoneNumber != null && p.Customer.PhoneNumber.Contains(term))
                );
            }

            var customers = await query
                .Select(p => new CustomerListItemDto
                {
                    ProfileId = p.ProfileId,
                    UserId = p.CustomerId,
                    FullName = p.Customer.FirstName + " " + p.Customer.LastName,
                    PhoneNumber = p.Customer.PhoneNumber,
                    Email = p.Customer.Email,
                    LastVisitDate = p.LastVisitDate,
                    NextVisitDate = p.Customer.Reservations
                        .Where(r => r.BusinessId == businessId && r.StartTime > DateTime.UtcNow && r.Status == ReservationStatus.Confirmed)
                        .OrderBy(r => r.StartTime)
                        .Select(r => (DateTime?)r.StartTime)
                        .FirstOrDefault(),
                    CancelledCount = p.Customer.Reservations
                        .Count(r => r.BusinessId == businessId && r.Status == ReservationStatus.Cancelled),
                    TotalSpent = p.TotalSpent,
                    IsVIP = p.IsVIP,
                    IsBanned = p.IsBanned,
                    PointsBalance = _context.LoyaltyPoints
                        .Where(lp => lp.BusinessId == businessId && lp.CustomerId == p.CustomerId)
                        .Select(lp => lp.PointsBalance)
                        .FirstOrDefault()
                })
                .OrderByDescending(c => c.LastVisitDate)
                .ToListAsync();

            return Ok(customers);
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
                // If profile doesn't exist but user exists and has reservations, we should maybe create it?
                // For now, return NotFound or create on the fly. Let's return NotFound.
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
