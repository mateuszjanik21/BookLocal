using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookLocal.API.Controllers
{
    [ApiController]
    [Route("api/employees/{employeeId}/availability")]
    public class AvailabilityController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AvailabilityController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<DateTime>>> GetAvailableSlots(int employeeId, [FromQuery] DateTime date, [FromQuery] int serviceVariantId)
        {
            var variant = await _context.ServiceVariants
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.ServiceVariantId == serviceVariantId);

            if (variant == null) return BadRequest("Wariant usługi nie istnieje.");

            var dayOfWeek = date.DayOfWeek;
            var workSchedule = await _context.WorkSchedules
                .AsNoTracking()
                .FirstOrDefaultAsync(ws => ws.EmployeeId == employeeId && ws.DayOfWeek == dayOfWeek);

            if (workSchedule == null || workSchedule.IsDayOff || !workSchedule.StartTime.HasValue || !workSchedule.EndTime.HasValue)
            {
                return Ok(new List<DateTime>());
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

            var now = DateTime.UtcNow.AddHours(1);
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

            return Ok(availableSlots);
        }

        private DateTime RoundUpToNearestInterval(DateTime dt, int intervalMinutes)
        {
            var delta = intervalMinutes - (dt.Minute % intervalMinutes);
            if (delta == intervalMinutes) delta = 0;

            return dt.AddMinutes(delta).AddSeconds(-dt.Second).AddMilliseconds(-dt.Millisecond);
        }
    }
}