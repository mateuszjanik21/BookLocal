using BookLocal.API.DTOs;
using BookLocal.Data;
using BookLocal.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReservationsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly UserManager<User> _userManager;
    private readonly IHubContext<NotificationHub> _hubContext;

    public ReservationsController(AppDbContext context, UserManager<User> userManager, IHubContext<NotificationHub> hubContext)
    {
        _context = context;
        _userManager = userManager;
        _hubContext = hubContext;
    }

    [HttpPost]
    [Authorize(Roles = "customer")]
    public async Task<IActionResult> CreateReservation(ReservationCreateDto reservationDto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized("Nie można zidentyfikować użytkownika na podstawie tokenu.");
        }

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
            BusinessId = service.BusinessId,
            CustomerId = userId,
            ServiceId = reservationDto.ServiceId,
            EmployeeId = reservationDto.EmployeeId,
            StartTime = reservationDto.StartTime,
            EndTime = proposedEndTime,
            Status = ReservationStatus.Confirmed
        };

        _context.Reservations.Add(reservation);
        await _context.SaveChangesAsync();

        var customer = await _userManager.FindByIdAsync(userId);
        var notificationPayload = new
        {
            Message = $"Nowa rezerwacja od {customer.FirstName} na usługę '{service.Name}'.",
            ReservationId = reservation.ReservationId
        };

        await _hubContext.Clients.Group(service.BusinessId.ToString())
            .SendAsync("NewReservationNotification", notificationPayload);

        return Ok(new { Message = "Rezerwacja została pomyślnie utworzona." });
    }

    [HttpGet("my-reservations")]
    public async Task<ActionResult<IEnumerable<ReservationDto>>> GetMyReservations()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        IQueryable<Reservation> query = _context.Reservations;

        if (User.IsInRole("owner"))
        {
            query = query.Where(r => r.Business.OwnerId == userId);
        }
        else
        {
            query = query.Where(r => r.CustomerId == userId);
        }

        var reservationsFromDb = await query
            .Include(r => r.Employee)
            .Include(r => r.Customer)
            .Include(r => r.Business)
            .Include(r => r.Review)
            .OrderByDescending(r => r.StartTime)
            .AsNoTracking() 
            .ToListAsync();

        var serviceIds = reservationsFromDb.Select(r => r.ServiceId).Distinct().ToList();

        var services = await _context.Services
            .IgnoreQueryFilters()
            .Where(s => serviceIds.Contains(s.ServiceId))
            .ToDictionaryAsync(s => s.ServiceId);

        var reservationDtos = reservationsFromDb.Select(r => new ReservationDto
        {
            ReservationId = r.ReservationId,
            StartTime = r.StartTime,
            EndTime = r.EndTime,
            Status = r.Status.ToString(),
            ServiceId = r.ServiceId,
            ServiceName = services.ContainsKey(r.ServiceId) ? services[r.ServiceId].Name : "Usunięta usługa",
            BusinessName = r.Business.Name,
            EmployeeId = r.EmployeeId,
            EmployeeFullName = $"{r.Employee.FirstName} {r.Employee.LastName}",
            CustomerId = r.CustomerId,
            CustomerFullName = r.Customer != null ? $"{r.Customer.FirstName} {r.Customer.LastName}" : null,
            GuestName = r.GuestName,
            HasReview = r.Review != null
        }).ToList();


        return Ok(reservationDtos);
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult<ReservationDto>> GetReservationById(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var reservation = await _context.Reservations
            .Include(r => r.Service)
            .Include(r => r.Employee)
            .Include(r => r.Customer)
            .FirstOrDefaultAsync(r => r.ReservationId == id);

        if (reservation == null)
        {
            return NotFound();
        }

        var isOwner = User.IsInRole("owner") && await _context.Businesses
            .AnyAsync(b => b.OwnerId == userId && b.BusinessId == reservation.BusinessId);

        if (reservation.CustomerId != userId && !isOwner)
        {
            return Forbid();
        }

        var reservationDto = new ReservationDto
        {
            ReservationId = reservation.ReservationId,
            StartTime = reservation.StartTime,
            EndTime = reservation.EndTime,
            Status = reservation.Status.ToString(),
            ServiceId = reservation.ServiceId,
            ServiceName = reservation.Service.Name,
            EmployeeId = reservation.EmployeeId,
            EmployeeFullName = $"{reservation.Employee.FirstName} {reservation.Employee.LastName}",
            CustomerId = reservation.CustomerId,
            CustomerFullName = reservation.Customer != null ? $"{reservation.Customer.FirstName} {reservation.Customer.LastName}" : null,
            GuestName = reservation.GuestName
        };

        return Ok(reservationDto);
    }

    [HttpPatch("{id}/status")]
    [Authorize(Roles = "owner")]
    public async Task<IActionResult> UpdateReservationStatus(int id, [FromBody] UpdateReservationStatusDto statusDto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var reservation = await _context.Reservations
            .Include(r => r.Service)
            .ThenInclude(s => s.ServiceCategory)
            .ThenInclude(sc => sc.Business)
            .FirstOrDefaultAsync(r => r.ReservationId == id);

        if (reservation == null)
        {
            return NotFound();
        }

        if (reservation.Service.ServiceCategory.Business.OwnerId != userId)
        {
            return Forbid();
        }

        if (Enum.TryParse<ReservationStatus>(statusDto.Status, true, out var newStatus))
        {
            reservation.Status = newStatus;
            await _context.SaveChangesAsync();
            return Ok(new { Message = "Status rezerwacji został zaktualizowany." });
        }

        return BadRequest("Nieprawidłowy status.");
    }

    [HttpPatch("my-reservations/{id}/cancel")]
    [Authorize(Roles = "customer")]
    public async Task<IActionResult> CancelReservation(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        var reservation = await _context.Reservations
            .Include(r => r.Customer)
            .Include(r => r.Service)
            .FirstOrDefaultAsync(r => r.ReservationId == id && r.CustomerId == userId);

        if (reservation == null)
        {
            return NotFound("Nie znaleziono rezerwacji lub nie masz do niej dostępu.");
        }

        if (reservation.StartTime < DateTime.Now)
        {
            return BadRequest("Nie można anulować przeszłych rezerwacji.");
        }

        if (reservation.Status == ReservationStatus.Cancelled)
        {
            return BadRequest("Ta rezerwacja została już anulowana.");
        }

        reservation.Status = ReservationStatus.Cancelled;
        await _context.SaveChangesAsync();

        var notificationPayload = new
        {
            Message = $"Klient {reservation.Customer.FirstName} anulował wizytę na usługę '{reservation.Service.Name}'.",
            ReservationId = reservation.ReservationId,
            Status = reservation.Status.ToString()
        };

        await _hubContext.Clients.Group(reservation.BusinessId.ToString())
            .SendAsync("ReservationCancelledNotification", notificationPayload);

        return Ok(new { Message = "Rezerwacja została pomyślnie anulowana." });
    }

    [HttpPost("dashboard/reservations")]
    [Authorize(Roles = "owner")]
    public async Task<IActionResult> CreateReservationAsOwner([FromBody] OwnerCreateReservationDto reservationDto)
    {
        var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var service = await _context.Services.Include(s => s.ServiceCategory.Business)
            .FirstOrDefaultAsync(s => s.ServiceId == reservationDto.ServiceId);

        if (service == null) return NotFound("Usługa nie istnieje.");
        if (service.ServiceCategory.Business.OwnerId != ownerId) return Forbid();

        var dayOfWeek = reservationDto.StartTime.DayOfWeek;
        var workSchedule = await _context.WorkSchedules
            .FirstOrDefaultAsync(ws => ws.EmployeeId == reservationDto.EmployeeId && ws.DayOfWeek == dayOfWeek);

        if (workSchedule == null || workSchedule.IsDayOff || !workSchedule.StartTime.HasValue || !workSchedule.EndTime.HasValue)
        {
            return BadRequest("Pracownik nie pracuje w wybranym dniu.");
        }

        var proposedEndTime = reservationDto.StartTime.AddMinutes(service.DurationMinutes);
        var requestedStartTimeOfDay = reservationDto.StartTime.TimeOfDay;
        var requestedEndTimeOfDay = proposedEndTime.TimeOfDay;

        if (requestedStartTimeOfDay < workSchedule.StartTime.Value || requestedEndTimeOfDay > workSchedule.EndTime.Value)
        {
            return BadRequest("Wybrany termin wykracza poza godziny pracy pracownika.");
        }

        var isSlotTaken = await _context.Reservations
            .AnyAsync(r => r.EmployeeId == reservationDto.EmployeeId &&
                            r.Status == ReservationStatus.Confirmed &&
                            r.StartTime < proposedEndTime &&
                            r.EndTime > reservationDto.StartTime);

        if (isSlotTaken)
            return Conflict("Ten termin u wybranego pracownika jest już zajęty.");

        var reservation = new Reservation
        {
            BusinessId = service.BusinessId,
            ServiceId = reservationDto.ServiceId,
            EmployeeId = reservationDto.EmployeeId,
            StartTime = reservationDto.StartTime,
            EndTime = proposedEndTime,
            GuestName = reservationDto.GuestName,
            GuestPhoneNumber = reservationDto.GuestPhoneNumber,
            Status = ReservationStatus.Confirmed
        };

        _context.Reservations.Add(reservation);
        await _context.SaveChangesAsync();

        return Ok(new { Message = "Rezerwacja została pomyślnie utworzona." });
    }
}