using BookLocal.API.Interfaces;
using BookLocal.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace BookLocal.API.Services
{
    public class MaintenanceService : IMaintenanceService
    {
        private readonly AppDbContext _context;

        public MaintenanceService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<(bool Success, string Message)> RecalculateCustomerStatsAsync()
        {
            var profiles = await _context.CustomerBusinessProfiles.ToListAsync();
            int count = 0;

            var statsDict = await _context.Reservations
                .Where(r => r.CustomerId != null)
                .GroupBy(r => new { r.BusinessId, r.CustomerId })
                .Select(g => new
                {
                    g.Key.BusinessId,
                    g.Key.CustomerId,
                    TotalSpent = g.Where(r => r.Status == ReservationStatus.Completed).Sum(r => r.AgreedPrice),
                    NoShowCount = g.Count(r => r.Status == ReservationStatus.NoShow),
                    LastVisit = g.Where(r => r.Status == ReservationStatus.Completed).Max(r => (DateTime?)r.StartTime),
                    CancelledCount = g.Count(r => r.Status == ReservationStatus.Cancelled),
                    NextVisit = g.Where(r => r.StartTime > DateTime.UtcNow && r.Status == ReservationStatus.Confirmed).Min(r => (DateTime?)r.StartTime)
                })
                .ToDictionaryAsync(x => new { x.BusinessId, x.CustomerId });

            foreach (var profile in profiles)
            {
                var key = new { profile.BusinessId, profile.CustomerId };
                if (statsDict.TryGetValue(key, out var stats))
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
            return (true, $"Zaktualizowano statystyki dla {count} profili.");
        }
    }
}
