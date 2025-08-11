using BookLocal.API.DTOs;
using BookLocal.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

[ApiController]
[Route("api/schedules")]
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
            .FirstOrDefaultAsync(e => e.EmployeeId == employeeId && e.Business.OwnerId == ownerId);

        if (employee == null) return Forbid();

        var schedule = await _context.WorkSchedules
            .Where(ws => ws.EmployeeId == employeeId)
            .Select(ws => new WorkScheduleDto
            {
                DayOfWeek = ws.DayOfWeek,
                StartTime = ws.StartTime.HasValue ? ws.StartTime.Value.ToString(@"hh\:mm") : null,
                EndTime = ws.EndTime.HasValue ? ws.EndTime.Value.ToString(@"hh\:mm") : null,
                IsDayOff = ws.IsDayOff
            })
            .ToListAsync();

        return Ok(schedule);
    }

    [HttpPost("{employeeId}")]
    public async Task<IActionResult> UpdateSchedule(int employeeId, List<WorkScheduleDto> scheduleDtos)
    {
        var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var employee = await _context.Employees
            .Include(e => e.WorkSchedules)
            .FirstOrDefaultAsync(e => e.EmployeeId == employeeId && e.Business.OwnerId == ownerId);

        if (employee == null) return Forbid();

        _context.WorkSchedules.RemoveRange(employee.WorkSchedules);

        foreach (var dto in scheduleDtos)
        {
            var schedule = new WorkSchedule
            {
                EmployeeId = employeeId,
                DayOfWeek = dto.DayOfWeek,
                IsDayOff = dto.IsDayOff,
                StartTime = dto.StartTime != null ? TimeSpan.Parse(dto.StartTime) : null,
                EndTime = dto.EndTime != null ? TimeSpan.Parse(dto.EndTime) : null
            };
            _context.WorkSchedules.Add(schedule);
        }

        await _context.SaveChangesAsync();
        return Ok(new { Message = "Grafik pracownika został zaktualizowany." });
    }
}