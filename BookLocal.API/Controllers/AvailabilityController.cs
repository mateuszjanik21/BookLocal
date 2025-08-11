using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
    public async Task<ActionResult<IEnumerable<DateTime>>> GetAvailableSlots(int employeeId, [FromQuery] DateTime date, [FromQuery] int serviceId)
    {
        var service = await _context.Services.FindAsync(serviceId);
        if (service == null) return BadRequest("Usługa nie istnieje.");

        var dayOfWeek = date.DayOfWeek;
        var workSchedule = await _context.WorkSchedules
            .FirstOrDefaultAsync(ws => ws.EmployeeId == employeeId && ws.DayOfWeek == dayOfWeek);

        if (workSchedule == null || workSchedule.IsDayOff || !workSchedule.StartTime.HasValue || !workSchedule.EndTime.HasValue)
        {
            return Ok(new List<DateTime>());
        }

        var reservations = await _context.Reservations
            .Where(r => r.EmployeeId == employeeId &&
                        r.StartTime.Date == date.Date &&
                        r.Status == ReservationStatus.Confirmed)
            .ToListAsync();

        var availableSlots = new List<DateTime>();
        var bookingInterval = 15;

        var dayStart = date.Date + workSchedule.StartTime.Value;
        var dayEnd = date.Date + workSchedule.EndTime.Value;

        for (var potentialStart = dayStart; potentialStart < dayEnd; potentialStart = potentialStart.AddMinutes(bookingInterval))
        {
            var potentialEnd = potentialStart.AddMinutes(service.DurationMinutes);

            if (potentialEnd > dayEnd)
            {
                break;
            }

            var isSlotTaken = reservations.Any(r => potentialStart < r.EndTime && r.StartTime < potentialEnd);

            if (!isSlotTaken)
            {
                availableSlots.Add(potentialStart);
            }
        }

        return Ok(availableSlots);
    }
}