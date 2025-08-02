using BookLocal.API.DTOs;
using BookLocal.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReservationsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly UserManager<User> _userManager;

    public ReservationsController(AppDbContext context, UserManager<User> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpPost]
    [Authorize(Roles = "customer")]
    public async Task<IActionResult> CreateReservation(ReservationCreateDto reservationDto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var service = await _context.Services.FindAsync(reservationDto.ServiceId);
        if (service == null) return NotFound("Wybrana usługa nie istnieje.");

        var employee = await _context.Employees.FindAsync(reservationDto.EmployeeId);
        if (employee == null || employee.BusinessId != service.BusinessId)
            return BadRequest("Wybrany pracownik nie istnieje lub nie pracuje w tej firmie.");

        var canPerformService = await _context.EmployeeServices
            .AnyAsync(es => es.EmployeeId == reservationDto.EmployeeId && es.ServiceId == reservationDto.ServiceId);

        if (!canPerformService)
            return BadRequest("Ten pracownik nie wykonuje wybranej usługi.");

        var proposedEndTime = reservationDto.StartTime.AddMinutes(service.DurationMinutes);
        var isSlotTaken = await _context.Reservations
            .AnyAsync(r => r.EmployeeId == reservationDto.EmployeeId &&
                           r.StartTime < proposedEndTime &&
                           r.EndTime > reservationDto.StartTime);

        if (isSlotTaken)
            return Conflict("Wybrany termin u tego pracownika jest już zajęty.");

        var reservation = new Reservation
        {
            CustomerId = userId,
            ServiceId = reservationDto.ServiceId,
            EmployeeId = reservationDto.EmployeeId,
            StartTime = reservationDto.StartTime,
            EndTime = proposedEndTime,
            Status = ReservationStatus.confirmed
        };

        _context.Reservations.Add(reservation);
        await _context.SaveChangesAsync();

        return Ok(new { Message = "Rezerwacja została pomyślnie utworzona." });
    }

    [HttpGet("my-reservations")]
    public async Task<ActionResult<IEnumerable<ReservationDto>>> GetMyReservations()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        IQueryable<Reservation> query = _context.Reservations;

        if (User.IsInRole("owner"))
        {
            query = query.Where(r => r.Service.Business.OwnerId == userId);
        }
        else
        {
            query = query.Where(r => r.CustomerId == userId);
        }

        var reservations = await query
            .Include(r => r.Service)
            .Include(r => r.Employee)
            .Include(r => r.Customer)
            .OrderByDescending(r => r.StartTime)
            .Select(r => new ReservationDto
            {
                ReservationId = r.ReservationId,
                StartTime = r.StartTime,
                EndTime = r.EndTime,
                Status = r.Status.ToString(),
                ServiceId = r.ServiceId,
                ServiceName = r.Service.Name,
                EmployeeId = r.EmployeeId,
                EmployeeFullName = $"{r.Employee.FirstName} {r.Employee.LastName}",
                CustomerId = r.CustomerId,
                CustomerFullName = $"{r.Customer.FirstName} {r.Customer.LastName}"
            })
            .ToListAsync();

        return Ok(reservations);
    }
}