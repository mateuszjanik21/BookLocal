using BookLocal.API.Interfaces;
using BookLocal.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace BookLocal.API.Services
{
    public class AvailabilityService : IAvailabilityService
    {
        private readonly AppDbContext _context;

        public AvailabilityService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<(bool Success, IEnumerable<DateTime>? Slots, string? ErrorMessage)> GetAvailableSlotsAsync(int employeeId, DateTime date, int serviceVariantId)
        {
            var variant = await _context.ServiceVariants
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.ServiceVariantId == serviceVariantId);

            if (variant == null) return (false, null, "Wariant usługi nie istnieje.");

            var dayOfWeek = date.DayOfWeek;
            var workSchedule = await _context.WorkSchedules
                .AsNoTracking()
                .FirstOrDefaultAsync(ws => ws.EmployeeId == employeeId && ws.DayOfWeek == dayOfWeek);

            if (workSchedule == null || workSchedule.IsDayOff || !workSchedule.StartTime.HasValue || !workSchedule.EndTime.HasValue)
            {
                return (true, new List<DateTime>(), null);
            }

            var reservations = await _context.Reservations
                .AsNoTracking()
                .Where(r => r.EmployeeId == employeeId &&
                            r.StartTime.Date == date.Date &&
                            r.Status != ReservationStatus.Cancelled)
                .Select(r => new { r.StartTime, r.EndTime })
                .ToListAsync();

            var availableSlots = new List<DateTime>();
            var bookingInterval = 15;

            var requiredDuration = variant.DurationMinutes + variant.CleanupTimeMinutes;

            var dayStart = date.Date + workSchedule.StartTime.Value;
            var dayEnd = date.Date + workSchedule.EndTime.Value;

            var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Europe/Warsaw"));
            var firstPossibleMoment = (date.Date == now.Date && now > dayStart) ? RoundUpToNearestInterval(now, bookingInterval) : dayStart;

            for (var potentialStart = dayStart; potentialStart < dayEnd; potentialStart = potentialStart.AddMinutes(bookingInterval))
            {
                if (potentialStart < firstPossibleMoment)
                {
                    continue;
                }

                var potentialEnd = potentialStart.AddMinutes(requiredDuration);

                if (potentialEnd > dayEnd)
                {
                    break;
                }

                var isSlotTaken = reservations.Any(r =>
                    potentialStart < r.EndTime && r.StartTime < potentialEnd);

                if (!isSlotTaken)
                {
                    availableSlots.Add(potentialStart);
                }
            }

            return (true, availableSlots, null);
        }

        public async Task<(bool Success, IEnumerable<DateTime>? Slots, string? ErrorMessage)> GetBundleAvailableSlotsAsync(int employeeId, DateTime date, int bundleId)
        {
            var bundle = await _context.ServiceBundles
                .Include(sb => sb.BundleItems)
                .ThenInclude(i => i.ServiceVariant)
                .AsNoTracking()
                .FirstOrDefaultAsync(sb => sb.ServiceBundleId == bundleId);

            if (bundle == null) return (false, null, "Pakiet nie istnieje.");

            var totalDurationMinutes = bundle.BundleItems.Sum(i => i.ServiceVariant.DurationMinutes + i.ServiceVariant.CleanupTimeMinutes);
            if (totalDurationMinutes == 0) return (true, new List<DateTime>(), null);

            var dayOfWeek = date.DayOfWeek;
            var workSchedule = await _context.WorkSchedules
                .AsNoTracking()
                .FirstOrDefaultAsync(ws => ws.EmployeeId == employeeId && ws.DayOfWeek == dayOfWeek);

            if (workSchedule == null || workSchedule.IsDayOff || !workSchedule.StartTime.HasValue || !workSchedule.EndTime.HasValue)
            {
                return (true, new List<DateTime>(), null);
            }

            var reservations = await _context.Reservations
                .AsNoTracking()
                .Where(r => r.EmployeeId == employeeId &&
                            r.StartTime.Date == date.Date &&
                            r.Status != ReservationStatus.Cancelled)
                .Select(r => new { r.StartTime, r.EndTime })
                .ToListAsync();

            var availableSlots = new List<DateTime>();
            var bookingInterval = 15;

            var dayStart = date.Date + workSchedule.StartTime.Value;
            var dayEnd = date.Date + workSchedule.EndTime.Value;

            var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Europe/Warsaw"));
            var firstPossibleMoment = (date.Date == now.Date && now > dayStart) ? RoundUpToNearestInterval(now, bookingInterval) : dayStart;

            for (var potentialStart = dayStart; potentialStart < dayEnd; potentialStart = potentialStart.AddMinutes(bookingInterval))
            {
                if (potentialStart < firstPossibleMoment) continue;

                var potentialEnd = potentialStart.AddMinutes(totalDurationMinutes);

                if (potentialEnd > dayEnd) break;

                var isSlotTaken = reservations.Any(r =>
                    potentialStart < r.EndTime && r.StartTime < potentialEnd);

                if (!isSlotTaken)
                {
                    availableSlots.Add(potentialStart);
                }
            }

            return (true, availableSlots, null);
        }

        private DateTime RoundUpToNearestInterval(DateTime dt, int intervalMinutes)
        {
            var delta = intervalMinutes - (dt.Minute % intervalMinutes);
            if (delta == intervalMinutes) delta = 0;

            return dt.AddMinutes(delta).AddSeconds(-dt.Second).AddMilliseconds(-dt.Millisecond);
        }
    }
}
