using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookLocal.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "owner")]
    public class MaintenanceController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MaintenanceController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("recalculate-stats")]
        public async Task<IActionResult> RecalculateCustomerStats()
        {
            var profiles = await _context.CustomerBusinessProfiles.ToListAsync();
            int count = 0;

            foreach (var profile in profiles)
            {
                var stats = await _context.Reservations
                .Where(r => r.BusinessId == profile.BusinessId && r.CustomerId == profile.CustomerId)
                .GroupBy(r => 1)
                .Select(g => new
                {
                    TotalSpent = g.Where(r => r.Status == ReservationStatus.Completed).Sum(r => r.AgreedPrice),
                    NoShowCount = g.Count(r => r.Status == ReservationStatus.NoShow),
                    LastVisit = g.Where(r => r.Status == ReservationStatus.Completed).Max(r => (DateTime?)r.StartTime),
                    CancelledCount = g.Count(r => r.Status == ReservationStatus.Cancelled),
                    NextVisit = g.Where(r => r.StartTime > DateTime.UtcNow && r.Status == ReservationStatus.Confirmed)
                        .OrderBy(r => r.StartTime)
                        .Select(r => (DateTime?)r.StartTime)
                        .FirstOrDefault()
                })
                .FirstOrDefaultAsync();

                if (stats != null)
                {
                    profile.TotalSpent = stats.TotalSpent;
                    profile.NoShowCount = stats.NoShowCount;
                    profile.LastVisitDate = stats.LastVisit ?? profile.LastVisitDate;
                    profile.CancelledCount = stats.CancelledCount;
                    profile.NextVisitDate = stats.NextVisit;
                }
                count++;
            }

            await _context.SaveChangesAsync();
            return Ok(new { Message = $"Zaktualizowano statystyki dla {count} profili." });
        }
    }
}
