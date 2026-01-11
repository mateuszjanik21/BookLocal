using BookLocal.API.DTOs;
using BookLocal.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "owner")]
public class SchedulesController : ControllerBase
{
    private readonly AppDbContext _context;

    public SchedulesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("{employeeId}")]
    public async Task<ActionResult<IEnumerable<WorkScheduleDto>>> GetSchedule(int employeeId)
    {
        var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var employee = await _context.Employees
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.EmployeeId == employeeId && e.Business.OwnerId == ownerId);

        if (employee == null)
        {
            return Forbid("Brak dostępu do tego pracownika.");
        }

        var schedule = await _context.WorkSchedules
            .Where(ws => ws.EmployeeId == employeeId)
            .OrderBy(ws => ws.DayOfWeek)
            .Select(ws => new WorkScheduleDto
            {
                DayOfWeek = ws.DayOfWeek,
                IsDayOff = ws.IsDayOff,
                StartTime = ws.StartTime != null ? ws.StartTime.Value.ToString(@"hh\:mm") : null,
                EndTime = ws.EndTime != null ? ws.EndTime.Value.ToString(@"hh\:mm") : null
            })
            .ToListAsync();

        return Ok(schedule);
    }

    [HttpPut("{employeeId}")]
    public async Task<IActionResult> UpdateSchedule(int employeeId, [FromBody] List<WorkScheduleDto> schedulePayload)
    {
        var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var employee = await _context.Employees
            .Include(e => e.WorkSchedules)
            .Include(e => e.Business)
            .FirstOrDefaultAsync(e => e.EmployeeId == employeeId && e.Business.OwnerId == ownerId);

        if (employee == null)
        {
            return Forbid();
        }

        foreach (var dayPayload in schedulePayload)
        {
            var scheduleDay = employee.WorkSchedules.FirstOrDefault(ws => ws.DayOfWeek == dayPayload.DayOfWeek);

            TimeSpan? start = null;
            TimeSpan? end = null;

            if (!string.IsNullOrEmpty(dayPayload.StartTime) && TimeSpan.TryParse(dayPayload.StartTime, out var s))
                start = s;

            if (!string.IsNullOrEmpty(dayPayload.EndTime) && TimeSpan.TryParse(dayPayload.EndTime, out var e))
                end = e;

            if (scheduleDay != null)
            {
                scheduleDay.IsDayOff = dayPayload.IsDayOff;
                scheduleDay.StartTime = dayPayload.IsDayOff ? null : start;
                scheduleDay.EndTime = dayPayload.IsDayOff ? null : end;
            }
            else
            {
                var newSchedule = new WorkSchedule
                {
                    EmployeeId = employeeId,
                    DayOfWeek = dayPayload.DayOfWeek,
                    IsDayOff = dayPayload.IsDayOff,
                    StartTime = dayPayload.IsDayOff ? null : start,
                    EndTime = dayPayload.IsDayOff ? null : end
                };
                _context.WorkSchedules.Add(newSchedule);
            }
        }

        await _context.SaveChangesAsync();

        return NoContent();
    }
}