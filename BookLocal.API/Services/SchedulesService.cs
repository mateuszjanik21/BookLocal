using BookLocal.API.DTOs;
using BookLocal.API.Interfaces;
using BookLocal.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BookLocal.API.Services
{
    public class SchedulesService : ISchedulesService
    {
        private readonly AppDbContext _context;

        public SchedulesService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<(bool Success, IEnumerable<WorkScheduleDto>? Data, string? ErrorMessage)> GetScheduleAsync(int employeeId, ClaimsPrincipal user)
        {
            var ownerId = user.FindFirstValue(ClaimTypes.NameIdentifier);

            var employee = await _context.Employees
                .Include(e => e.Business)
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.EmployeeId == employeeId && e.Business.OwnerId == ownerId);

            if (employee == null)
            {
                return (false, null, "Brak dostępu do tego pracownika.");
            }

            var schedule = await _context.WorkSchedules
                .AsNoTracking()
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

            return (true, schedule, null);
        }

        public async Task<(bool Success, string? Message, string? ErrorMessage)> UpdateScheduleAsync(int employeeId, List<WorkScheduleDto> schedulePayload, ClaimsPrincipal user)
        {
            var ownerId = user.FindFirstValue(ClaimTypes.NameIdentifier);

            var employee = await _context.Employees
                .Include(e => e.WorkSchedules)
                .Include(e => e.Business)
                .FirstOrDefaultAsync(e => e.EmployeeId == employeeId && e.Business.OwnerId == ownerId);

            if (employee == null)
            {
                return (false, null, "Brak uprawnień.");
            }

            var now = DateTime.UtcNow;
            var futureReservations = await _context.Reservations
                .Where(r => r.EmployeeId == employeeId
                    && r.Status == ReservationStatus.Confirmed
                    && r.StartTime > now)
                .ToListAsync();

            if (futureReservations.Any())
            {
                var polishDays = new Dictionary<DayOfWeek, string>
                {
                    { DayOfWeek.Monday, "Poniedziałek" },
                    { DayOfWeek.Tuesday, "Wtorek" },
                    { DayOfWeek.Wednesday, "Środa" },
                    { DayOfWeek.Thursday, "Czwartek" },
                    { DayOfWeek.Friday, "Piątek" },
                    { DayOfWeek.Saturday, "Sobota" },
                    { DayOfWeek.Sunday, "Niedziela" }
                };

                var conflicts = new List<string>();

                foreach (var dayPayload in schedulePayload)
                {
                    var reservationsOnDay = futureReservations
                        .Where(r => r.StartTime.DayOfWeek == dayPayload.DayOfWeek)
                        .ToList();

                    if (!reservationsOnDay.Any()) continue;

                    var dayName = polishDays.GetValueOrDefault(dayPayload.DayOfWeek, dayPayload.DayOfWeek.ToString());

                    if (dayPayload.IsDayOff)
                    {
                        conflicts.Add($"{dayName} (dzień wolny)");
                        continue;
                    }

                    TimeSpan? newStart = null, newEnd = null;
                    if (!string.IsNullOrEmpty(dayPayload.StartTime) && TimeSpan.TryParse(dayPayload.StartTime, out var ns))
                        newStart = ns;
                    if (!string.IsNullOrEmpty(dayPayload.EndTime) && TimeSpan.TryParse(dayPayload.EndTime, out var ne))
                        newEnd = ne;

                    if (newStart == null || newEnd == null) continue;

                    var hasConflict = reservationsOnDay.Any(r =>
                    {
                        var resStart = r.StartTime.TimeOfDay;
                        var resEnd = r.EndTime.TimeOfDay;
                        return resStart < newStart.Value || resEnd > newEnd.Value;
                    });

                    if (hasConflict)
                    {
                        conflicts.Add($"{dayName} (skrócenie godzin)");
                    }
                }

                if (conflicts.Any())
                {
                    return (false, null, $"Nie można zmienić grafiku. Pracownik ma potwierdzone wizyty kolidujące ze zmianami: {string.Join(", ", conflicts)}. Najpierw anuluj te wizyty.");
                }
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

            return (true, "Grafik został zaktualizowany.", null);
        }
    }
}
