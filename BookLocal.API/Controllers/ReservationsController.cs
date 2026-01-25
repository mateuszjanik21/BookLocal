using BookLocal.API.DTOs;
using BookLocal.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BookLocal.API.Controllers
{
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

        // POST: api/reservations
        [HttpPost]
        [Authorize(Roles = "customer")]
        public async Task<IActionResult> CreateReservation(ReservationCreateDto reservationDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Nie można zidentyfikować użytkownika na podstawie tokenu.");
            }

            var variant = await _context.ServiceVariants
                .Include(v => v.Service)
                .FirstOrDefaultAsync(v => v.ServiceVariantId == reservationDto.ServiceVariantId);

            if (variant == null) return NotFound("Wybrany wariant usługi nie istnieje.");

            var service = variant.Service;


            var employee = await _context.Employees.FindAsync(reservationDto.EmployeeId);

            if (employee == null || employee.BusinessId != service.BusinessId)
                return BadRequest("Wybrany pracownik nie istnieje lub nie pracuje w tej firmie.");

            var canPerformService = await _context.EmployeeServices
                .AnyAsync(es => es.EmployeeId == reservationDto.EmployeeId && es.ServiceId == service.ServiceId);

            if (!canPerformService)
                return BadRequest("Ten pracownik nie wykonuje wybranej usługi.");

            var totalDuration = variant.DurationMinutes + variant.CleanupTimeMinutes;
            var proposedEndTime = reservationDto.StartTime.AddMinutes(totalDuration);

            var isSlotTaken = await _context.Reservations
                .AnyAsync(r => r.EmployeeId == reservationDto.EmployeeId &&
                               r.Status != ReservationStatus.Cancelled &&
                               r.StartTime < proposedEndTime &&
                               r.EndTime > reservationDto.StartTime);

            if (isSlotTaken)
                return Conflict("Wybrany termin u tego pracownika jest już zajęty.");

            decimal discountAmount = 0;
            int? discountId = null;

            if (!string.IsNullOrEmpty(reservationDto.DiscountCode))
            {
                var discountResult = await ApplyDiscount(service.BusinessId, reservationDto.DiscountCode, variant.Price, service.ServiceId);
                if (discountResult.error != null)
                {
                    return BadRequest(discountResult.error);
                }
                discountAmount = discountResult.discountAmount;
                discountId = discountResult.discountId;
            }

            var reservation = new Reservation
            {
                BusinessId = service.BusinessId,
                CustomerId = userId,
                ServiceVariantId = reservationDto.ServiceVariantId,
                EmployeeId = reservationDto.EmployeeId,
                StartTime = reservationDto.StartTime,
                EndTime = proposedEndTime,
                AgreedPrice = variant.Price - discountAmount,
                DiscountAmount = discountAmount,
                DiscountId = discountId,
                Status = ReservationStatus.Confirmed,
                PaymentMethod = reservationDto.PaymentMethod
            };

            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();

            var customer = await _userManager.FindByIdAsync(userId);
            var notificationPayload = new
            {
                Message = $"Nowa rezerwacja od {customer.FirstName} na usługę '{service.Name}' ({variant.Name}).",
                ReservationId = reservation.ReservationId
            };


            await _hubContext.Clients.Group(service.BusinessId.ToString())
                .SendAsync("NewReservationNotification", notificationPayload);

            await EnsureCustomerProfile(service.BusinessId, userId);

            return Ok(new { Message = "Rezerwacja została pomyślnie utworzona." });
        }

        // GET: api/reservations/my-reservations
        [HttpGet("my-reservations")]
        public async Task<ActionResult<IEnumerable<ReservationDto>>> GetMyReservations(
            [FromQuery] string scope = "upcoming",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var now = DateTime.UtcNow;

            IQueryable<Reservation> query = _context.Reservations;

            if (User.IsInRole("owner"))
            {
                query = query.Where(r => r.Business.OwnerId == userId);
            }
            else
            {
                query = query.Where(r => r.CustomerId == userId);
            }

            switch (scope.ToLower())
            {
                case "upcoming":
                    query = query.Where(r => r.StartTime >= now).OrderBy(r => r.StartTime);
                    break;
                case "past":
                    query = query.Where(r => r.StartTime < now).OrderByDescending(r => r.StartTime);
                    break;
                default:
                    query = query.OrderByDescending(r => r.StartTime);
                    break;
            }

            var totalCount = await query.CountAsync();

            var reservations = await query
                .Include(r => r.Employee)
                .Include(r => r.Customer)
                .Include(r => r.Business)
                .Include(r => r.Review)
                .Include(r => r.ServiceVariant)
                    .ThenInclude(v => v.Service)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();

            var reservationDtos = reservations.Select(r => new ReservationDto
            {
                ReservationId = r.ReservationId,
                StartTime = r.StartTime,
                EndTime = r.EndTime,
                Status = r.StartTime < now && r.Status == ReservationStatus.Confirmed ? "Completed" : r.Status.ToString(),
                ServiceVariantId = r.ServiceVariantId,
                ServiceName = r.ServiceVariant?.Service?.Name ?? "Usługa usunięta",
                VariantName = r.ServiceVariant?.Name ?? "",
                AgreedPrice = r.AgreedPrice,
                BusinessName = r.Business.Name,
                BusinessId = r.BusinessId,
                EmployeeId = r.EmployeeId,
                EmployeeFullName = $"{r.Employee.FirstName} {r.Employee.LastName}",
                CustomerId = r.CustomerId,
                CustomerFullName = r.Customer != null ? $"{r.Customer.FirstName} {r.Customer.LastName}" : "Gość",
                GuestName = r.GuestName,
                HasReview = r.Review != null,
                PaymentMethod = r.PaymentMethod.ToString()
            }).ToList();

            var pagedResult = new PagedResultDto<ReservationDto>
            {
                Items = reservationDtos,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            return Ok(pagedResult);
        }

        // GET: api/reservations/calendar
        [HttpGet("calendar")]
        [Authorize(Roles = "owner")]
        public async Task<ActionResult<IEnumerable<ReservationDto>>> GetCalendarEvents()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var now = DateTime.UtcNow;

            var reservationsFromDb = await _context.Reservations
                .Where(r => r.Business.OwnerId == userId)
                .Include(r => r.Employee)
                .Include(r => r.Customer)
                .Include(r => r.Business)
                .Include(r => r.Review)
                .Include(r => r.ServiceVariant)
                    .ThenInclude(v => v.Service)
                .ToListAsync();

            var reservationsToUpdate = reservationsFromDb
                .Where(r => r.EndTime < now && r.Status == ReservationStatus.Confirmed)
                .ToList();

            if (reservationsToUpdate.Any())
            {
                foreach (var reservation in reservationsToUpdate)
                {
                    reservation.Status = ReservationStatus.Completed;
                    if (reservation.CustomerId != null)
                    {
                        await ProcessLoyaltyPoints(reservation.BusinessId, reservation.CustomerId, reservation.AgreedPrice, reservation.ReservationId);
                        await EnsureCustomerProfile(reservation.BusinessId, reservation.CustomerId);
                        await UpdateCustomerStats(reservation.BusinessId, reservation.CustomerId);
                    }
                }
                await _context.SaveChangesAsync();
            }

            var reservationDtos = reservationsFromDb.Select(r => new ReservationDto
            {
                ReservationId = r.ReservationId,
                StartTime = r.StartTime,
                EndTime = r.EndTime,
                Status = r.Status.ToString(),

                ServiceVariantId = r.ServiceVariantId,
                ServiceName = r.ServiceVariant?.Service?.Name ?? "Usunięta",
                VariantName = r.ServiceVariant?.Name ?? "",
                AgreedPrice = r.AgreedPrice,

                BusinessName = r.Business.Name,
                BusinessId = r.BusinessId,
                EmployeeId = r.EmployeeId,
                EmployeeFullName = $"{r.Employee.FirstName} {r.Employee.LastName}",
                CustomerId = r.CustomerId,
                CustomerFullName = r.Customer != null ? $"{r.Customer.FirstName} {r.Customer.LastName}" : null,
                GuestName = r.GuestName,
                HasReview = r.Review != null,
                PaymentMethod = r.PaymentMethod.ToString()
            }).ToList();

            return Ok(reservationDtos);
        }

        // GET: api/reservations/5
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<ReservationDto>> GetReservationById(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var reservation = await _context.Reservations
                .Include(r => r.Employee)
                .Include(r => r.Customer)
                .Include(r => r.Business)
                .Include(r => r.ServiceVariant)
                    .ThenInclude(v => v.Service)
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.ReservationId == id);

            if (reservation == null) return NotFound();

            var isOwner = User.IsInRole("owner") && reservation.Business.OwnerId == userId;
            if (reservation.CustomerId != userId && !isOwner) return Forbid();

            var reservationDto = new ReservationDto
            {
                ReservationId = reservation.ReservationId,
                StartTime = reservation.StartTime,
                EndTime = reservation.EndTime,
                Status = reservation.Status.ToString(),

                ServiceVariantId = reservation.ServiceVariantId,
                ServiceName = reservation.ServiceVariant?.Service?.Name ?? "Usunięta",
                VariantName = reservation.ServiceVariant?.Name ?? "",
                AgreedPrice = reservation.AgreedPrice,

                BusinessName = reservation.Business.Name,
                BusinessId = reservation.BusinessId,
                EmployeeId = reservation.EmployeeId,
                EmployeeFullName = $"{reservation.Employee.FirstName} {reservation.Employee.LastName}",
                CustomerId = reservation.CustomerId,
                CustomerFullName = reservation.Customer != null ? $"{reservation.Customer.FirstName} {reservation.Customer.LastName}" : null,
                GuestName = reservation.GuestName,
                HasReview = await _context.Reviews.AnyAsync(r => r.ReservationId == id),
                IsServiceArchived = reservation.ServiceVariant?.Service?.IsArchived ?? true,
                PaymentMethod = reservation.PaymentMethod.ToString()
            };

            return Ok(reservationDto);
        }

        // PATCH: api/reservations/5/status
        [HttpPatch("{id}/status")]
        [Authorize(Roles = "owner")]
        public async Task<IActionResult> UpdateReservationStatus(int id, [FromBody] UpdateReservationStatusDto statusDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var reservation = await _context.Reservations
                .Include(r => r.Business)
                .FirstOrDefaultAsync(r => r.ReservationId == id);

            if (reservation == null) return NotFound("Nie znaleziono rezerwacji.");

            if (reservation.Business.OwnerId != userId) return Forbid();


            if (Enum.TryParse<ReservationStatus>(statusDto.Status, true, out var newStatus))
            {
                reservation.Status = newStatus;
                await _context.SaveChangesAsync();

                if (newStatus == ReservationStatus.Completed || newStatus == ReservationStatus.NoShow)
                {
                    if (reservation.CustomerId != null)
                    {
                        if (newStatus == ReservationStatus.Completed)
                        {
                            await ProcessLoyaltyPoints(reservation.BusinessId, reservation.CustomerId, reservation.AgreedPrice, reservation.ReservationId);
                        }

                        await EnsureCustomerProfile(reservation.BusinessId, reservation.CustomerId);
                        await UpdateCustomerStats(reservation.BusinessId, reservation.CustomerId);
                    }
                }

                return Ok(new { Message = "Status rezerwacji został zaktualizowany." });
            }

            return BadRequest("Nieprawidłowy status.");
        }

        // PATCH: api/reservations/my-reservations/5/cancel
        [HttpPatch("my-reservations/{id}/cancel")]
        [Authorize(Roles = "customer")]
        public async Task<IActionResult> CancelReservation(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var reservation = await _context.Reservations
                .Include(r => r.Customer)
                .Include(r => r.ServiceVariant)
                    .ThenInclude(v => v.Service)
                .FirstOrDefaultAsync(r => r.ReservationId == id && r.CustomerId == userId);

            if (reservation == null) return NotFound("Nie znaleziono rezerwacji lub nie masz do niej dostępu.");

            if (reservation.StartTime < DateTime.UtcNow) return BadRequest("Nie można anulować przeszłych rezerwacji.");
            if (reservation.Status == ReservationStatus.Cancelled) return BadRequest("Ta rezerwacja została już anulowana.");

            reservation.Status = ReservationStatus.Cancelled;
            await _context.SaveChangesAsync();

            var serviceName = reservation.ServiceVariant?.Service?.Name ?? "Usługa";
            var notificationPayload = new
            {
                Message = $"Klient {reservation.Customer.FirstName} anulował wizytę na '{serviceName}'.",
                ReservationId = reservation.ReservationId,
                Status = reservation.Status.ToString()
            };

            await _hubContext.Clients.Group(reservation.BusinessId.ToString())
                .SendAsync("ReservationCancelledNotification", notificationPayload);

            return Ok(new { Message = "Rezerwacja została pomyślnie anulowana." });
        }

        // POST: api/reservations/bundle
        [HttpPost("bundle")]
        [Authorize(Roles = "customer")]
        public async Task<IActionResult> CreateBundleReservation([FromBody] BundleReservationCreateDto reservationDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var bundle = await _context.ServiceBundles
                .Include(sb => sb.BundleItems)
                    .ThenInclude(i => i.ServiceVariant)
                        .ThenInclude(sv => sv.Service)
                .FirstOrDefaultAsync(sb => sb.ServiceBundleId == reservationDto.ServiceBundleId);

            if (bundle == null) return NotFound("Pakiet nie istnieje.");

            var bundleItems = bundle.BundleItems.OrderBy(i => i.SequenceOrder).ToList();
            if (!bundleItems.Any()) return BadRequest("Pakiet nie zawiera żadnych usług.");

            var dayOfWeek = reservationDto.StartTime.DayOfWeek;
            var workSchedule = await _context.WorkSchedules
                .AsNoTracking()
                .FirstOrDefaultAsync(ws => ws.EmployeeId == reservationDto.EmployeeId && ws.DayOfWeek == dayOfWeek);

            if (workSchedule == null || workSchedule.IsDayOff || !workSchedule.StartTime.HasValue || !workSchedule.EndTime.HasValue)
            {
                return BadRequest("Pracownik nie pracuje w wybranym dniu.");
            }

            var totalDuration = bundleItems.Sum(i => i.ServiceVariant.DurationMinutes + i.ServiceVariant.CleanupTimeMinutes);
            var sequenceEndTime = reservationDto.StartTime.AddMinutes(totalDuration);

            if (reservationDto.StartTime.TimeOfDay < workSchedule.StartTime.Value || sequenceEndTime.TimeOfDay > workSchedule.EndTime.Value)
            {
                return BadRequest("Wybrany termin pakietu wykracza poza godziny pracy.");
            }

            var isTaken = await _context.Reservations
                .AnyAsync(r => r.EmployeeId == reservationDto.EmployeeId &&
                               r.Status == ReservationStatus.Confirmed &&
                               r.StartTime < sequenceEndTime &&
                               r.EndTime > reservationDto.StartTime);

            if (isTaken) return Conflict("Jeden z terminów w ramach pakietu jest już zajęty.");

            var currentStartTime = reservationDto.StartTime;
            var reservationsToCreate = new List<Reservation>();

            decimal sumOfVariantsPrice = bundleItems.Sum(i => i.ServiceVariant.Price);
            decimal globalDiscountRatio = 1.0m;
            if (sumOfVariantsPrice > 0)
            {
                globalDiscountRatio = bundle.TotalPrice / sumOfVariantsPrice;
            }

            foreach (var item in bundleItems)
            {
                var variantDuration = item.ServiceVariant.DurationMinutes + item.ServiceVariant.CleanupTimeMinutes;
                var currentEndTime = currentStartTime.AddMinutes(variantDuration);

                var itemPrice = item.ServiceVariant.Price * globalDiscountRatio;

                var reservation = new Reservation
                {
                    BusinessId = bundle.BusinessId,
                    CustomerId = userId,
                    ServiceVariantId = item.ServiceVariantId,
                    EmployeeId = reservationDto.EmployeeId,
                    ServiceBundleId = bundle.ServiceBundleId,
                    StartTime = currentStartTime,
                    EndTime = currentEndTime,
                    AgreedPrice = itemPrice,
                    Status = ReservationStatus.Confirmed,
                    PaymentMethod = reservationDto.PaymentMethod
                };

                reservationsToCreate.Add(reservation);
                currentStartTime = currentEndTime;
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.Reservations.AddRange(reservationsToCreate);
                await _context.SaveChangesAsync();

                var customer = await _userManager.FindByIdAsync(userId);
                var notificationPayload = new
                {
                    Message = $"Nowa rezerwacja pakietowa od {customer.FirstName} na '{bundle.Name}'.",
                    ReservationId = reservationsToCreate.First().ReservationId
                };

                await _hubContext.Clients.Group(bundle.BusinessId.ToString())
                    .SendAsync("NewReservationNotification", notificationPayload);

                await EnsureCustomerProfile(bundle.BusinessId, userId);

                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, "Wystąpił błąd podczas tworzenia rezerwacji pakietowej.");
            }

            return Ok(new { Message = "Pakiet został pomyślnie zarezerwowany." });
        }

        // POST: api/reservations/dashboard/reservations
        [HttpPost("dashboard/reservations")]
        [Authorize(Roles = "owner")]
        public async Task<IActionResult> CreateReservationAsOwner([FromBody] OwnerCreateReservationDto reservationDto)
        {
            var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var variant = await _context.ServiceVariants
                .Include(v => v.Service)
                    .ThenInclude(s => s.ServiceCategory)
                        .ThenInclude(sc => sc.Business)
                .FirstOrDefaultAsync(v => v.ServiceVariantId == reservationDto.ServiceVariantId);

            if (variant == null) return NotFound("Wariant usługi nie istnieje.");
            if (variant.Service.ServiceCategory.Business.OwnerId != ownerId) return Forbid();

            var dayOfWeek = reservationDto.StartTime.DayOfWeek;
            var workSchedule = await _context.WorkSchedules
                .FirstOrDefaultAsync(ws => ws.EmployeeId == reservationDto.EmployeeId && ws.DayOfWeek == dayOfWeek);

            if (workSchedule == null || workSchedule.IsDayOff || !workSchedule.StartTime.HasValue || !workSchedule.EndTime.HasValue)
            {
                return BadRequest("Pracownik nie pracuje w wybranym dniu.");
            }

            var totalDuration = variant.DurationMinutes + variant.CleanupTimeMinutes;
            var proposedEndTime = reservationDto.StartTime.AddMinutes(totalDuration);

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

            decimal discountAmount = 0;
            int? discountId = null;

            if (!string.IsNullOrEmpty(reservationDto.DiscountCode))
            {
                var discountResult = await ApplyDiscount(variant.Service.BusinessId, reservationDto.DiscountCode, variant.Price, variant.Service.ServiceId);
                if (discountResult.error != null)
                {
                    return BadRequest(discountResult.error);
                }
                discountAmount = discountResult.discountAmount;
                discountId = discountResult.discountId;
            }

            var reservation = new Reservation
            {
                BusinessId = variant.Service.BusinessId,
                ServiceVariantId = reservationDto.ServiceVariantId,
                EmployeeId = reservationDto.EmployeeId,
                StartTime = reservationDto.StartTime,
                EndTime = proposedEndTime,
                AgreedPrice = variant.Price - discountAmount,
                DiscountAmount = discountAmount,
                DiscountId = discountId,
                GuestName = reservationDto.GuestName,
                GuestPhoneNumber = reservationDto.GuestPhoneNumber,
                Status = ReservationStatus.Confirmed
            };

            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Rezerwacja została pomyślnie utworzona." });
        }

        // POST: api/reservations/dashboard/reservations/bundle
        [HttpPost("dashboard/reservations/bundle")]
        [Authorize(Roles = "owner")]
        public async Task<IActionResult> CreateBundleReservationAsOwner([FromBody] OwnerCreateBundleReservationDto reservationDto)
        {
            var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var bundle = await _context.ServiceBundles
                .Include(sb => sb.BundleItems)
                    .ThenInclude(i => i.ServiceVariant)
                .FirstOrDefaultAsync(sb => sb.ServiceBundleId == reservationDto.ServiceBundleId);

            if (bundle == null) return NotFound("Pakiet nie istnieje.");

            if (!await _context.Businesses.AnyAsync(b => b.BusinessId == bundle.BusinessId && b.OwnerId == ownerId))
                return Forbid();

            var bundleItems = bundle.BundleItems.OrderBy(i => i.SequenceOrder).ToList();
            if (!bundleItems.Any()) return BadRequest("Pakiet nie zawiera żadnych usług.");


            var dayOfWeek = reservationDto.StartTime.DayOfWeek;
            var workSchedule = await _context.WorkSchedules
                .AsNoTracking()
                .FirstOrDefaultAsync(ws => ws.EmployeeId == reservationDto.EmployeeId && ws.DayOfWeek == dayOfWeek);

            if (workSchedule == null || workSchedule.IsDayOff || !workSchedule.StartTime.HasValue || !workSchedule.EndTime.HasValue)
            {
                return BadRequest("Pracownik nie pracuje w wybranym dniu.");
            }

            var totalDuration = bundleItems.Sum(i => i.ServiceVariant.DurationMinutes + i.ServiceVariant.CleanupTimeMinutes);
            var sequenceEndTime = reservationDto.StartTime.AddMinutes(totalDuration);

            if (reservationDto.StartTime.TimeOfDay < workSchedule.StartTime.Value || sequenceEndTime.TimeOfDay > workSchedule.EndTime.Value)
            {
                return BadRequest("Wybrany termin pakietu wykracza poza godziny pracy.");
            }

            var isTaken = await _context.Reservations
                .AnyAsync(r => r.EmployeeId == reservationDto.EmployeeId &&
                               r.Status == ReservationStatus.Confirmed &&
                               r.StartTime < sequenceEndTime &&
                               r.EndTime > reservationDto.StartTime);

            if (isTaken) return Conflict("Jeden z terminów w ramach pakietu jest już zajęty.");

            var currentStartTime = reservationDto.StartTime;
            var reservationsToCreate = new List<Reservation>();


            decimal sumOfVariantsPrice = bundleItems.Sum(i => i.ServiceVariant.Price);
            decimal globalDiscountRatio = 1.0m;
            if (sumOfVariantsPrice > 0)
            {
                globalDiscountRatio = bundle.TotalPrice / sumOfVariantsPrice;
            }

            foreach (var item in bundleItems)
            {
                var variantDuration = item.ServiceVariant.DurationMinutes + item.ServiceVariant.CleanupTimeMinutes;
                var currentEndTime = currentStartTime.AddMinutes(variantDuration);

                var itemPrice = item.ServiceVariant.Price * globalDiscountRatio;

                var reservation = new Reservation
                {
                    BusinessId = bundle.BusinessId,
                    ServiceVariantId = item.ServiceVariantId,
                    EmployeeId = reservationDto.EmployeeId,
                    ServiceBundleId = bundle.ServiceBundleId,
                    StartTime = currentStartTime,
                    EndTime = currentEndTime,
                    AgreedPrice = itemPrice,
                    GuestName = reservationDto.GuestName,
                    GuestPhoneNumber = reservationDto.GuestPhoneNumber,
                    Status = ReservationStatus.Confirmed,
                    PaymentMethod = reservationDto.PaymentMethod
                };

                reservationsToCreate.Add(reservation);
                currentStartTime = currentEndTime;
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.Reservations.AddRange(reservationsToCreate);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, "Wystąpił błąd podczas tworzenia rezerwacji pakietowej.");
            }

            return Ok(new { Message = "Pakiet został pomyślnie zarezerwowany." });
        }

        private async Task EnsureCustomerProfile(int businessId, string customerId)
        {
            var exists = await _context.CustomerBusinessProfiles
               .AnyAsync(p => p.BusinessId == businessId && p.CustomerId == customerId);

            if (!exists)
            {
                var profile = new CustomerBusinessProfile
                {
                    BusinessId = businessId,
                    CustomerId = customerId,
                    LastVisitDate = DateTime.MinValue
                };
                _context.CustomerBusinessProfiles.Add(profile);
                await _context.SaveChangesAsync();
            }
        }

        private async Task UpdateCustomerStats(int businessId, string customerId)
        {
            var profile = await _context.CustomerBusinessProfiles
                .FirstOrDefaultAsync(p => p.BusinessId == businessId && p.CustomerId == customerId);

            if (profile == null) return;

            var stats = await _context.Reservations
                .Where(r => r.BusinessId == businessId && r.CustomerId == customerId)
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
                await _context.SaveChangesAsync();
            }
        }

        private async Task<(decimal discountAmount, int? discountId, string? error)> ApplyDiscount(int businessId, string code, decimal originalPrice, int? serviceId)
        {
            if (string.IsNullOrWhiteSpace(code)) return (0, null, null);

            var discount = await _context.Discounts
                .FirstOrDefaultAsync(d => d.BusinessId == businessId && d.Code == code && d.IsActive);

            if (discount == null) return (0, null, "Kod rabatowy nie istnieje lub jest nieaktywny.");

            if (discount.MaxUses.HasValue && discount.UsedCount >= discount.MaxUses.Value)
                return (0, null, "Limit użycia tego kodu został wyczerpany.");

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            if (discount.ValidFrom.HasValue && today < discount.ValidFrom.Value)
                return (0, null, "Kod nie jest jeszcze aktywny.");
            if (discount.ValidTo.HasValue && today > discount.ValidTo.Value)
                return (0, null, "Kod wygasł.");

            if (discount.ServiceId.HasValue && serviceId.HasValue && discount.ServiceId != serviceId)
                return (0, null, "Kod nie dotyczy tej usługi.");

            decimal amount = 0;
            if (discount.Type == DiscountType.Percentage)
            {
                amount = originalPrice * (discount.Value / 100m);
            }
            else
            {
                amount = discount.Value;
            }

            if (amount > originalPrice) amount = originalPrice;

            discount.UsedCount++;
            await _context.SaveChangesAsync();

            return (amount, discount.DiscountId, null);
        }

        private async Task ProcessLoyaltyPoints(int businessId, string customerId, decimal price, int reservationId)
        {
            var config = await _context.LoyaltyProgramConfigs
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.BusinessId == businessId);

            if (config == null || !config.IsActive || config.SpendAmountForOnePoint <= 0) return;

            int pointsToEarn = (int)(price / config.SpendAmountForOnePoint);
            if (pointsToEarn <= 0) return;

            var loyaltyPoint = await _context.LoyaltyPoints
                .FirstOrDefaultAsync(p => p.BusinessId == businessId && p.CustomerId == customerId);

            if (loyaltyPoint == null)
            {
                loyaltyPoint = new LoyaltyPoint
                {
                    BusinessId = businessId,
                    CustomerId = customerId,
                    PointsBalance = 0,
                    TotalPointsEarned = 0
                };
                _context.LoyaltyPoints.Add(loyaltyPoint);
            }

            loyaltyPoint.PointsBalance += pointsToEarn;
            loyaltyPoint.TotalPointsEarned += pointsToEarn;
            loyaltyPoint.LastUpdated = DateTime.UtcNow;

            var transaction = new LoyaltyTransaction
            {
                LoyaltyPoint = loyaltyPoint,
                PointsAmount = pointsToEarn,
                Type = LoyaltyTransactionType.Earned,
                Description = $"Wizyta (ID: {reservationId})",
                ReservationId = reservationId,
                CreatedAt = DateTime.UtcNow
            };

            _context.LoyaltyTransactions.Add(transaction);
            await _context.SaveChangesAsync();
        }
    }
}