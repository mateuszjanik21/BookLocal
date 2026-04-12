using BookLocal.API.DTOs;
using BookLocal.API.Interfaces;
using BookLocal.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BookLocal.API.Services
{
    public class ReservationsService : IReservationsService
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IHubContext<NotificationHub> _hubContext;

        public ReservationsService(AppDbContext context, UserManager<User> userManager, IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _userManager = userManager;
            _hubContext = hubContext;
        }

        public async Task<(bool Success, string? Message, string? ErrorMessage)> CreateReservationAsync(ReservationCreateDto reservationDto, ClaimsPrincipal user)
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return (false, null, "Nie można zidentyfikować użytkownika na podstawie tokenu.");

            var variant = await _context.ServiceVariants
                .Include(v => v.Service)
                .FirstOrDefaultAsync(v => v.ServiceVariantId == reservationDto.ServiceVariantId);

            if (variant == null) return (false, null, "Wybrany wariant usługi nie istnieje.");

            var service = variant.Service;

            var employee = await _context.Employees.FindAsync(reservationDto.EmployeeId);
            if (employee == null || employee.BusinessId != service.BusinessId)
                return (false, null, "Wybrany pracownik nie istnieje lub nie pracuje w tej firmie.");

            var canPerformService = await _context.EmployeeServices
                .AnyAsync(es => es.EmployeeId == reservationDto.EmployeeId && es.ServiceId == service.ServiceId);

            if (!canPerformService)
                return (false, null, "Ten pracownik nie wykonuje wybranej usługi.");

            var totalDuration = variant.DurationMinutes + variant.CleanupTimeMinutes;
            var proposedEndTime = reservationDto.StartTime.AddMinutes(totalDuration);

            var isSlotTaken = await _context.Reservations
                .AnyAsync(r => r.EmployeeId == reservationDto.EmployeeId &&
                               r.Status != ReservationStatus.Cancelled &&
                               r.StartTime < proposedEndTime &&
                               r.EndTime > reservationDto.StartTime);

            if (isSlotTaken)
                return (false, null, "Wybrany termin u tego pracownika jest już zajęty.");

            decimal discountAmount = 0;
            int? discountId = null;

            if (!string.IsNullOrEmpty(reservationDto.DiscountCode))
            {
                var discountResult = await ApplyDiscount(service.BusinessId, reservationDto.DiscountCode, variant.Price, service.ServiceId, userId);
                if (discountResult.error != null)
                    return (false, null, discountResult.error);
                discountAmount = discountResult.discountAmount;
                discountId = discountResult.discountId;
            }

            decimal loyaltyDiscount = 0;
            if (reservationDto.LoyaltyPointsUsed > 0)
            {
                var loyaltyResult = await RedeemLoyaltyPoints(service.BusinessId, userId, reservationDto.LoyaltyPointsUsed, variant.Price - discountAmount);
                if (loyaltyResult.error != null)
                    return (false, null, loyaltyResult.error);
                loyaltyDiscount = loyaltyResult.discount;
            }

            var reservation = new Reservation
            {
                BusinessId = service.BusinessId,
                CustomerId = userId,
                ServiceVariantId = reservationDto.ServiceVariantId,
                EmployeeId = reservationDto.EmployeeId,
                StartTime = reservationDto.StartTime,
                EndTime = proposedEndTime,
                AgreedPrice = variant.Price - discountAmount - loyaltyDiscount,
                DiscountAmount = discountAmount,
                DiscountId = discountId,
                LoyaltyPointsUsed = reservationDto.LoyaltyPointsUsed,
                Status = ReservationStatus.Confirmed,
                PaymentMethod = reservationDto.PaymentMethod
            };

            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();

            if (reservation.PaymentMethod == PaymentMethod.Online && reservation.AgreedPrice > 0)
            {
                var payment = new Payment
                {
                    ReservationId = reservation.ReservationId,
                    BusinessId = service.BusinessId,
                    PaymentMethod = PaymentMethod.Online,
                    Amount = reservation.AgreedPrice,
                    Status = PaymentStatus.Completed,
                    TransactionDate = DateTime.UtcNow
                };

                var activeSubscription = await _context.BusinessSubscriptions
                    .Include(s => s.Plan)
                    .FirstOrDefaultAsync(s => s.BusinessId == service.BusinessId && s.IsActive);

                if (activeSubscription != null)
                {
                    var commissionRate = (decimal)activeSubscription.Plan.CommissionPercentage / 100m;
                    payment.CommissionAmount = Math.Round(payment.Amount * commissionRate, 2);
                }

                _context.Payments.Add(payment);
                await _context.SaveChangesAsync();
            }

            var customer = await _userManager.FindByIdAsync(userId);
            var notificationPayload = new
            {
                Message = $"Nowa rezerwacja od {customer?.FirstName} na usługę '{service.Name}' ({variant.Name}).",
                reservation.ReservationId
            };

            await _hubContext.Clients.Group(service.BusinessId.ToString())
                .SendAsync("NewReservationNotification", notificationPayload);

            await EnsureCustomerProfile(service.BusinessId, userId);

            return (true, "Rezerwacja została pomyślnie utworzona.", null);
        }

        public async Task<(bool Success, CustomerStatsDto? Data, string? ErrorMessage)> GetMyStatsAsync(ClaimsPrincipal user)
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return (false, null, "Unauthorized");

            var completedReservations = _context.Reservations
                .AsNoTracking()
                .Where(r => r.CustomerId == userId && r.Status == ReservationStatus.Completed);

            var totalVisits = await completedReservations.CountAsync();
            var totalSpent = totalVisits > 0 ? await completedReservations.SumAsync(r => r.AgreedPrice) : 0;
            var uniqueBusinesses = await completedReservations.Select(r => r.BusinessId).Distinct().CountAsync();

            string? favoriteBusinessName = null;
            if (totalVisits > 0)
            {
                favoriteBusinessName = await completedReservations
                    .GroupBy(r => new { r.BusinessId, r.Business.Name })
                    .OrderByDescending(g => g.Count())
                    .Select(g => g.Key.Name)
                    .FirstOrDefaultAsync();
            }

            return (true, new CustomerStatsDto
            {
                TotalVisits = totalVisits,
                TotalSpent = totalSpent,
                UniqueBusinesses = uniqueBusinesses,
                FavoriteBusinessName = favoriteBusinessName
            }, null);
        }

        public async Task<(bool Success, PagedResultDto<ReservationDto>? Data)> GetMyReservationsAsync(string scope, int pageNumber, int pageSize, ClaimsPrincipal user)
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            var now = DateTime.UtcNow;

            IQueryable<Reservation> query = _context.Reservations;

            if (user.IsInRole("owner"))
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

            var reservations = await query
                .Include(r => r.Employee)
                .Include(r => r.Customer)
                .Include(r => r.Business)
                .Include(r => r.Review)
                .Include(r => r.ServiceVariant)
                    .ThenInclude(v => v.Service)
                .Include(r => r.ServiceBundle)
                .AsNoTracking()
                .ToListAsync();

            var reservationsToComplete = reservations.Where(r => r.EndTime <= now && r.Status == ReservationStatus.Confirmed).ToList();
            if (reservationsToComplete.Any())
            {
                var idsToComplete = reservationsToComplete.Select(r => r.ReservationId).ToList();
                var trackedReservations = await _context.Reservations.Where(r => idsToComplete.Contains(r.ReservationId)).ToListAsync();

                foreach (var tr in trackedReservations)
                {
                    tr.Status = ReservationStatus.Completed;
                    if (tr.CustomerId != null)
                    {
                        await ProcessLoyaltyPoints(tr.BusinessId, tr.CustomerId, tr.AgreedPrice, tr.ReservationId);
                        await EnsureCustomerProfile(tr.BusinessId, tr.CustomerId);
                        await UpdateCustomerStats(tr.BusinessId, tr.CustomerId);
                    }
                }

                if (trackedReservations.Any()) await _context.SaveChangesAsync();
                foreach (var r in reservationsToComplete) r.Status = ReservationStatus.Completed;
            }

            var reservationDtos = reservations
                .GroupBy(r => (r.ServiceBundleId.HasValue && r.ServiceBundleId.Value > 0) ? $"B_{r.ServiceBundleId}" : $"R_{r.ReservationId}")
                .Select(g => {
                    var first = g.OrderBy(r => r.StartTime).First();
                    var isBundle = g.Any(r => r.ServiceBundleId.HasValue && r.ServiceBundleId.Value > 0);

                    return new ReservationDto
                    {
                        ReservationId = first.ReservationId,
                        StartTime = first.StartTime,
                        EndTime = g.Max(r => r.EndTime),
                        Status = first.Status.ToString(),
                        ServiceVariantId = first.ServiceVariantId,
                        ServiceName = isBundle ? first.ServiceBundle?.Name ?? "Pakiet" : (first.ServiceVariant?.Service?.Name ?? "Usługa"),
                        VariantName = isBundle ? "Pakiet usług" : (first.ServiceVariant?.Name ?? ""),
                        AgreedPrice = g.Sum(r => r.AgreedPrice),
                        BusinessName = first.Business?.Name ?? "",
                        BusinessId = first.BusinessId,
                        EmployeeId = first.EmployeeId,
                        EmployeeFullName = first.Employee != null ? $"{first.Employee.FirstName} {first.Employee.LastName}" : "Nieznany",
                        EmployeePhotoUrl = first.Employee?.PhotoUrl,
                        CustomerId = first.CustomerId,
                        CustomerFullName = first.Customer != null ? $"{first.Customer.FirstName} {first.Customer.LastName}" : "Gość",
                        GuestName = first.GuestName,
                        HasReview = g.Any(r => r.Review != null),
                        ServiceBundleId = first.ServiceBundleId,
                        BundleName = first.ServiceBundle?.Name,
                        IsBundle = isBundle,
                        LoyaltyPointsUsed = g.Sum(r => r.LoyaltyPointsUsed),
                        PaymentMethod = first.PaymentMethod.ToString(),
                        SubItems = isBundle ? g.Select(r => new BundleSubItemDto
                        {
                            ReservationId = r.ReservationId,
                            ServiceName = r.ServiceVariant?.Service?.Name ?? "Usługa",
                            VariantName = r.ServiceVariant?.Name ?? ""
                        }).ToList() : null
                    };
                })
                .ToList();

            if (scope == "past") reservationDtos = reservationDtos.OrderByDescending(d => d.StartTime).ToList();
            else reservationDtos = reservationDtos.OrderBy(d => d.StartTime).ToList();

            var totalCount = reservationDtos.Count;
            var pagedResults = reservationDtos
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var pagedResult = new PagedResultDto<ReservationDto>
            {
                Items = pagedResults,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            return (true, pagedResult);
        }

        public async Task<(bool Success, IEnumerable<ReservationDto>? Data)> GetCalendarEventsAsync(DateTime? start, DateTime? end, int? employeeId, ClaimsPrincipal user)
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);

            var startDate = start ?? new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
            var endDate = end ?? startDate.AddMonths(1).AddDays(-1);

            var sqlEvents = await _context.Database
                .SqlQueryRaw<ReservationSqlDto>(
                    "EXEC GetOwnerCalendarEvents @OwnerId, @StartDate, @EndDate",
                    new Microsoft.Data.SqlClient.SqlParameter("@OwnerId", userId),
                    new Microsoft.Data.SqlClient.SqlParameter("@StartDate", startDate),
                    new Microsoft.Data.SqlClient.SqlParameter("@EndDate", endDate)
                )
                .ToListAsync();

            if (employeeId.HasValue)
            {
                sqlEvents = sqlEvents.Where(e => e.EmployeeId == employeeId.Value).ToList();
            }

            var reservationDtos = sqlEvents.Select(r => new ReservationDto
            {
                ReservationId = r.ReservationId,
                StartTime = r.StartTime,
                EndTime = r.EndTime,
                Status = r.Status,
                ServiceVariantId = r.ServiceVariantId,
                ServiceName = r.ServiceName,
                VariantName = r.VariantName,
                AgreedPrice = r.AgreedPrice,
                BusinessName = r.BusinessName,
                BusinessId = r.BusinessId,
                EmployeeId = r.EmployeeId,
                EmployeeFullName = r.EmployeeFullName,
                CustomerId = r.CustomerId,
                CustomerFullName = r.CustomerFullName,
                GuestName = r.GuestName,
                HasReview = r.HasReview,
                ServiceBundleId = r.ServiceBundleId,
                PaymentMethod = r.PaymentMethod
            }).ToList();

            return (true, reservationDtos);
        }

        public async Task<(bool Success, ReservationDto? Data, string? ErrorMessage)> GetReservationByIdAsync(int id, ClaimsPrincipal user)
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);

            var reservation = await _context.Reservations
                .Include(r => r.Employee)
                .Include(r => r.Customer)
                .Include(r => r.Business)
                .Include(r => r.ServiceVariant)
                    .ThenInclude(v => v.Service)
                .IgnoreQueryFilters()
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.ReservationId == id);

            if (reservation == null) return (false, null, "Nie znaleziono rezerwacji.");

            var now = DateTime.UtcNow;
            var pastConfirmedReservations = await _context.Reservations
                .Where(r => r.BusinessId == reservation.BusinessId && r.Status == ReservationStatus.Confirmed && r.EndTime <= now)
                .ToListAsync();

            if (pastConfirmedReservations.Any())
            {
                foreach (var tracked in pastConfirmedReservations)
                {
                    tracked.Status = ReservationStatus.Completed;
                    if (tracked.CustomerId != null)
                    {
                        await ProcessLoyaltyPoints(tracked.BusinessId, tracked.CustomerId, tracked.AgreedPrice, tracked.ReservationId);
                        await EnsureCustomerProfile(tracked.BusinessId, tracked.CustomerId);
                        await UpdateCustomerStats(tracked.BusinessId, tracked.CustomerId);
                    }
                }
                await _context.SaveChangesAsync();

                if (reservation.EndTime <= now && reservation.Status == ReservationStatus.Confirmed)
                {
                    reservation.Status = ReservationStatus.Completed;
                }
            }

            var isOwner = user.IsInRole("owner") && reservation.Business?.OwnerId == userId;
            if (reservation.CustomerId != userId && !isOwner) return (false, null, "Brak uprawnień.");

            var loyaltyPointsUsed = reservation.LoyaltyPointsUsed;
            if (reservation.ServiceBundleId.HasValue)
            {
                loyaltyPointsUsed = await _context.Reservations
                    .Where(r => r.ServiceBundleId == reservation.ServiceBundleId &&
                                r.CustomerId == reservation.CustomerId &&
                                r.StartTime.Date == reservation.StartTime.Date)
                    .SumAsync(r => r.LoyaltyPointsUsed);
            }

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
                BusinessName = reservation.Business?.Name ?? "",
                BusinessId = reservation.BusinessId,
                EmployeeId = reservation.EmployeeId,
                EmployeeFullName = reservation.Employee != null ? $"{reservation.Employee.FirstName} {reservation.Employee.LastName}" : "Usunięty pracownik",
                EmployeePhotoUrl = reservation.Employee?.PhotoUrl,
                EmployeePhoneNumber = null,
                CustomerId = reservation.CustomerId,
                CustomerFullName = reservation.Customer != null ? $"{reservation.Customer.FirstName} {reservation.Customer.LastName}" : null,
                CustomerPhotoUrl = reservation.Customer?.PhotoUrl,
                CustomerPhoneNumber = reservation.Customer?.PhoneNumber ?? reservation.GuestPhoneNumber,
                GuestName = reservation.GuestName,
                HasReview = await _context.Reviews.AnyAsync(r => r.ReservationId == id),
                IsServiceArchived = reservation.ServiceVariant?.Service?.IsArchived ?? true,
                ServiceBundleId = reservation.ServiceBundleId,
                LoyaltyPointsUsed = loyaltyPointsUsed,
                PaymentMethod = reservation.PaymentMethod.ToString()
            };

            return (true, reservationDto, null);
        }

        public async Task<(bool Success, string? Message, string? ErrorMessage)> UpdateReservationStatusAsync(int id, UpdateReservationStatusDto statusDto, ClaimsPrincipal user)
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            var reservation = await _context.Reservations
                .Include(r => r.Business)
                .FirstOrDefaultAsync(r => r.ReservationId == id);

            if (reservation == null) return (false, null, "Nie znaleziono rezerwacji.");
            if (reservation.Business?.OwnerId != userId) return (false, null, "Brak uprawnień.");

            if (Enum.TryParse<ReservationStatus>(statusDto.Status, out var newStatus))
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

                return (true, "Status rezerwacji został zaktualizowany.", null);
            }

            return (false, null, "Nieprawidłowy status.");
        }

        public async Task<(bool Success, string? Message, string? ErrorMessage)> CancelReservationAsync(int id, ClaimsPrincipal user)
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return (false, null, "Unauthorized");

            var reservation = await _context.Reservations
                .Include(r => r.Customer)
                .Include(r => r.ServiceVariant)
                    .ThenInclude(v => v.Service)
                .FirstOrDefaultAsync(r => r.ReservationId == id && r.CustomerId == userId);

            if (reservation == null) return (false, null, "Nie znaleziono rezerwacji lub nie masz do niej dostępu.");
            if (reservation.StartTime < DateTime.UtcNow) return (false, null, "Nie można anulować przeszłych rezerwacji.");
            if (reservation.Status == ReservationStatus.Cancelled) return (false, null, "Ta rezerwacja została już anulowana.");

            var reservationsToCancel = new List<Reservation> { reservation };

            if (reservation.ServiceBundleId.HasValue && reservation.ServiceBundleId.Value > 0)
            {
                var bundleReservations = await _context.Reservations
                    .Where(r => r.ServiceBundleId == reservation.ServiceBundleId &&
                                r.CustomerId == userId &&
                                r.Status == ReservationStatus.Confirmed)
                    .ToListAsync();

                foreach (var res in bundleReservations)
                {
                    if (!reservationsToCancel.Any(r => r.ReservationId == res.ReservationId))
                    {
                        reservationsToCancel.Add(res);
                    }
                }
            }

            foreach (var res in reservationsToCancel)
            {
                res.Status = ReservationStatus.Cancelled;
            }

            await _context.SaveChangesAsync();

            var serviceName = reservation.ServiceBundleId.HasValue ? "Pakiet usług" : (reservation.ServiceVariant?.Service?.Name ?? "Usługa");
            var notificationPayload = new
            {
                Message = $"Klient {reservation.Customer?.FirstName} anulował wizytę na '{serviceName}'.",
                reservation.ReservationId,
                Status = ReservationStatus.Cancelled.ToString(),
                IsBundle = reservation.ServiceBundleId.HasValue
            };

            await _hubContext.Clients.Group(reservation.BusinessId.ToString())
                .SendAsync("ReservationCancelledNotification", notificationPayload);

            var message = reservation.ServiceBundleId.HasValue ? "Pakiet został pomyślnie anulowany." : "Rezerwacja została pomyślnie anulowana.";
            return (true, message, null);
        }

        public async Task<(bool Success, string? Message, string? ErrorMessage)> CreateBundleReservationAsync(BundleReservationCreateDto reservationDto, ClaimsPrincipal user)
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return (false, null, "Unauthorized");

            var bundle = await _context.ServiceBundles
                .Include(sb => sb.BundleItems)
                    .ThenInclude(i => i.ServiceVariant)
                        .ThenInclude(sv => sv.Service)
                .FirstOrDefaultAsync(sb => sb.ServiceBundleId == reservationDto.ServiceBundleId);

            if (bundle == null) return (false, null, "Pakiet nie istnieje.");

            var bundleItems = bundle.BundleItems.OrderBy(i => i.SequenceOrder).ToList();
            if (!bundleItems.Any()) return (false, null, "Pakiet nie zawiera żadnych usług.");

            var dayOfWeek = reservationDto.StartTime.DayOfWeek;
            var workSchedule = await _context.WorkSchedules
                .AsNoTracking()
                .FirstOrDefaultAsync(ws => ws.EmployeeId == reservationDto.EmployeeId && ws.DayOfWeek == dayOfWeek);

            if (workSchedule == null || workSchedule.IsDayOff || !workSchedule.StartTime.HasValue || !workSchedule.EndTime.HasValue)
                return (false, null, "Pracownik nie pracuje w wybranym dniu.");

            var totalDuration = bundleItems.Sum(i => i.ServiceVariant.DurationMinutes + i.ServiceVariant.CleanupTimeMinutes);
            var sequenceEndTime = reservationDto.StartTime.AddMinutes(totalDuration);

            if (reservationDto.StartTime.TimeOfDay < workSchedule.StartTime.Value || sequenceEndTime.TimeOfDay > workSchedule.EndTime.Value)
                return (false, null, "Wybrany termin pakietu wykracza poza godziny pracy.");

            var isTaken = await _context.Reservations
                .AnyAsync(r => r.EmployeeId == reservationDto.EmployeeId &&
                               r.Status == ReservationStatus.Confirmed &&
                               r.StartTime < sequenceEndTime &&
                               r.EndTime > reservationDto.StartTime);

            if (isTaken) return (false, null, "Jeden z terminów w ramach pakietu jest już zajęty.");

            var currentStartTime = reservationDto.StartTime;
            var reservationsToCreate = new List<Reservation>();

            decimal sumOfVariantsPrice = bundleItems.Sum(i => i.ServiceVariant.Price);
            decimal globalDiscountRatio = 1.0m;
            if (sumOfVariantsPrice > 0)
            {
                globalDiscountRatio = bundle.TotalPrice / sumOfVariantsPrice;
            }

            decimal loyaltyDiscountTotal = 0;
            if (reservationDto.LoyaltyPointsUsed > 0)
            {
                var loyaltyResult = await RedeemLoyaltyPoints(bundle.BusinessId, userId, reservationDto.LoyaltyPointsUsed, bundle.TotalPrice);
                if (loyaltyResult.error != null)
                    return (false, null, loyaltyResult.error);
                loyaltyDiscountTotal = loyaltyResult.discount;
            }

            decimal loyaltyRatio = bundle.TotalPrice > 0 ? (bundle.TotalPrice - loyaltyDiscountTotal) / bundle.TotalPrice : 1.0m;

            foreach (var item in bundleItems)
            {
                var variantDuration = item.ServiceVariant.DurationMinutes + item.ServiceVariant.CleanupTimeMinutes;
                var currentEndTime = currentStartTime.AddMinutes(variantDuration);

                var itemPrice = item.ServiceVariant.Price * globalDiscountRatio * loyaltyRatio;

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
                    LoyaltyPointsUsed = bundleItems.IndexOf(item) == 0 ? reservationDto.LoyaltyPointsUsed : 0,
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

                if (reservationDto.PaymentMethod == PaymentMethod.Online)
                {
                    var totalAmount = reservationsToCreate.Sum(r => r.AgreedPrice);
                    if (totalAmount > 0)
                    {
                        var payment = new Payment
                        {
                            ReservationId = reservationsToCreate.First().ReservationId,
                            BusinessId = bundle.BusinessId,
                            PaymentMethod = PaymentMethod.Online,
                            Amount = totalAmount,
                            Status = PaymentStatus.Completed,
                            TransactionDate = DateTime.UtcNow
                        };

                        var activeSubscription = await _context.BusinessSubscriptions
                            .Include(s => s.Plan)
                            .FirstOrDefaultAsync(s => s.BusinessId == bundle.BusinessId && s.IsActive);

                        if (activeSubscription != null)
                        {
                            var commissionRate = (decimal)activeSubscription.Plan.CommissionPercentage / 100m;
                            payment.CommissionAmount = Math.Round(payment.Amount * commissionRate, 2);
                        }

                        _context.Payments.Add(payment);
                        await _context.SaveChangesAsync();
                    }
                }

                var customer = await _userManager.FindByIdAsync(userId);
                var notificationPayload = new
                {
                    Message = $"Nowa rezerwacja pakietowa od {customer?.FirstName} na '{bundle.Name}'.",
                    reservationsToCreate.First().ReservationId
                };

                await _hubContext.Clients.Group(bundle.BusinessId.ToString())
                    .SendAsync("NewReservationNotification", notificationPayload);

                await EnsureCustomerProfile(bundle.BusinessId, userId);

                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return (false, null, "Wystąpił błąd podczas tworzenia rezerwacji pakietowej.");
            }

            return (true, "Pakiet został pomyślnie zarezerwowany.", null);
        }

        public async Task<(bool Success, string? Message, string? ErrorMessage)> CreateReservationAsOwnerAsync(OwnerCreateReservationDto reservationDto, ClaimsPrincipal user)
        {
            var ownerId = user.FindFirstValue(ClaimTypes.NameIdentifier);

            var variant = await _context.ServiceVariants
                .Include(v => v.Service)
                    .ThenInclude(s => s.ServiceCategory)
                        .ThenInclude(sc => sc.Business)
                .FirstOrDefaultAsync(v => v.ServiceVariantId == reservationDto.ServiceVariantId);

            if (variant == null) return (false, null, "Wariant usługi nie istnieje.");
            if (variant.Service.ServiceCategory.Business.OwnerId != ownerId) return (false, null, "Brak uprawnień.");

            var dayOfWeek = reservationDto.StartTime.DayOfWeek;
            var workSchedule = await _context.WorkSchedules
                .FirstOrDefaultAsync(ws => ws.EmployeeId == reservationDto.EmployeeId && ws.DayOfWeek == dayOfWeek);

            if (workSchedule == null || workSchedule.IsDayOff || !workSchedule.StartTime.HasValue || !workSchedule.EndTime.HasValue)
                return (false, null, "Pracownik nie pracuje w wybranym dniu.");

            var totalDuration = variant.DurationMinutes + variant.CleanupTimeMinutes;
            var proposedEndTime = reservationDto.StartTime.AddMinutes(totalDuration);

            var requestedStartTimeOfDay = reservationDto.StartTime.TimeOfDay;
            var requestedEndTimeOfDay = proposedEndTime.TimeOfDay;

            if (requestedStartTimeOfDay < workSchedule.StartTime.Value || requestedEndTimeOfDay > workSchedule.EndTime.Value)
                return (false, null, "Wybrany termin wykracza poza godziny pracy pracownika.");

            var isSlotTaken = await _context.Reservations
                .AnyAsync(r => r.EmployeeId == reservationDto.EmployeeId &&
                               r.Status == ReservationStatus.Confirmed &&
                               r.StartTime < proposedEndTime &&
                               r.EndTime > reservationDto.StartTime);

            if (isSlotTaken)
                return (false, null, "Ten termin u wybranego pracownika jest już zajęty.");

            decimal discountAmount = 0;
            int? discountId = null;

            if (!string.IsNullOrEmpty(reservationDto.DiscountCode))
            {
                var discountResult = await ApplyDiscount(variant.Service.BusinessId, reservationDto.DiscountCode, variant.Price, variant.Service.ServiceId, reservationDto.GuestPhoneNumber);
                if (discountResult.error != null)
                    return (false, null, discountResult.error);
                discountAmount = discountResult.discountAmount;
                discountId = discountResult.discountId;
            }

            decimal loyaltyDiscount = 0;
            if (reservationDto.LoyaltyPointsUsed > 0 && !string.IsNullOrEmpty(reservationDto.CustomerId))
            {
                var loyaltyResult = await RedeemLoyaltyPoints(variant.Service.BusinessId, reservationDto.CustomerId, reservationDto.LoyaltyPointsUsed, variant.Price - discountAmount);
                if (loyaltyResult.error != null)
                    return (false, null, loyaltyResult.error);
                loyaltyDiscount = loyaltyResult.discount;
            }

            var reservation = new Reservation
            {
                BusinessId = variant.Service.BusinessId,
                ServiceVariantId = reservationDto.ServiceVariantId,
                EmployeeId = reservationDto.EmployeeId,
                StartTime = reservationDto.StartTime,
                EndTime = proposedEndTime,
                AgreedPrice = variant.Price - discountAmount - loyaltyDiscount,
                DiscountAmount = discountAmount,
                DiscountId = discountId,
                LoyaltyPointsUsed = reservationDto.LoyaltyPointsUsed,
                GuestName = reservationDto.GuestName,
                GuestPhoneNumber = reservationDto.GuestPhoneNumber,
                CustomerId = reservationDto.CustomerId,
                Status = ReservationStatus.Confirmed,
                PaymentMethod = reservationDto.PaymentMethod
            };

            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();

            return (true, "Rezerwacja została pomyślnie utworzona.", null);
        }

        public async Task<(bool Success, string? Message, string? ErrorMessage)> CreateBundleReservationAsOwnerAsync(OwnerCreateBundleReservationDto reservationDto, ClaimsPrincipal user)
        {
            var ownerId = user.FindFirstValue(ClaimTypes.NameIdentifier);

            var bundle = await _context.ServiceBundles
                .Include(sb => sb.BundleItems)
                    .ThenInclude(i => i.ServiceVariant)
                .FirstOrDefaultAsync(sb => sb.ServiceBundleId == reservationDto.ServiceBundleId);

            if (bundle == null) return (false, null, "Pakiet nie istnieje.");

            if (!await _context.Businesses.AnyAsync(b => b.BusinessId == bundle.BusinessId && b.OwnerId == ownerId))
                return (false, null, "Brak uprawnień.");

            var bundleItems = bundle.BundleItems.OrderBy(i => i.SequenceOrder).ToList();
            if (!bundleItems.Any()) return (false, null, "Pakiet nie zawiera żadnych usług.");

            var dayOfWeek = reservationDto.StartTime.DayOfWeek;
            var workSchedule = await _context.WorkSchedules
                .AsNoTracking()
                .FirstOrDefaultAsync(ws => ws.EmployeeId == reservationDto.EmployeeId && ws.DayOfWeek == dayOfWeek);

            if (workSchedule == null || workSchedule.IsDayOff || !workSchedule.StartTime.HasValue || !workSchedule.EndTime.HasValue)
                return (false, null, "Pracownik nie pracuje w wybranym dniu.");

            var totalDuration = bundleItems.Sum(i => i.ServiceVariant.DurationMinutes + i.ServiceVariant.CleanupTimeMinutes);
            var sequenceEndTime = reservationDto.StartTime.AddMinutes(totalDuration);

            if (reservationDto.StartTime.TimeOfDay < workSchedule.StartTime.Value || sequenceEndTime.TimeOfDay > workSchedule.EndTime.Value)
                return (false, null, "Wybrany termin pakietu wykracza poza godziny pracy.");

            var isTaken = await _context.Reservations
                .AnyAsync(r => r.EmployeeId == reservationDto.EmployeeId &&
                               r.Status == ReservationStatus.Confirmed &&
                               r.StartTime < sequenceEndTime &&
                               r.EndTime > reservationDto.StartTime);

            if (isTaken) return (false, null, "Jeden z terminów w ramach pakietu jest już zajęty.");

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
                return (false, null, "Wystąpił błąd podczas tworzenia rezerwacji pakietowej.");
            }

            return (true, "Pakiet został pomyślnie zarezerwowany.", null);
        }

        public async Task<(bool Success, object? Data, string? ErrorMessage)> GetAdjacentReservationsAsync(int id, ClaimsPrincipal user)
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            var reservation = await _context.Reservations
                .Include(r => r.Business)
                .FirstOrDefaultAsync(r => r.ReservationId == id);

            if (reservation == null) return (false, null, "Nie znaleziono rezerwacji.");
            if (reservation.Business?.OwnerId != userId && reservation.CustomerId != userId)
                return (false, null, "Brak uprawnień.");

            var previousId = await _context.Reservations
                .Where(r => r.BusinessId == reservation.BusinessId && r.StartTime < reservation.StartTime)
                .OrderByDescending(r => r.StartTime)
                .Select(r => (int?)r.ReservationId)
                .FirstOrDefaultAsync();

            var nextId = await _context.Reservations
                .Where(r => r.BusinessId == reservation.BusinessId && r.StartTime > reservation.StartTime)
                .OrderBy(r => r.StartTime)
                .Select(r => (int?)r.ReservationId)
                .FirstOrDefaultAsync();

            return (true, new { previousId, nextId }, null);
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

        private async Task<(decimal discountAmount, int? discountId, string? error)> ApplyDiscount(int businessId, string code, decimal originalPrice, int? serviceId, string? customerId = null)
        {
            if (string.IsNullOrWhiteSpace(code)) return (0, null, null);

            var discount = await _context.Discounts
                .FirstOrDefaultAsync(d => d.BusinessId == businessId && d.Code == code && d.IsActive);

            if (discount == null) return (0, null, "Kod rabatowy nie istnieje lub jest nieaktywny.");

            if (discount.MaxUses.HasValue && discount.UsedCount >= discount.MaxUses.Value)
                return (0, null, "Limit użycia tego kodu został wyczerpany.");

            if (discount.MaxUsesPerCustomer.HasValue && !string.IsNullOrEmpty(customerId))
            {
                var userUses = await _context.Reservations
                    .CountAsync(r => r.CustomerId == customerId &&
                                     r.DiscountId == discount.DiscountId &&
                                     r.Status != ReservationStatus.Cancelled &&
                                     r.Status != ReservationStatus.NoShow);

                if (userUses >= discount.MaxUsesPerCustomer.Value)
                    return (0, null, "Osiągnięto limit użyć tego kodu dla Twojego konta.");
            }

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

            var loyaltyTransaction = new LoyaltyTransaction
            {
                LoyaltyPoint = loyaltyPoint,
                PointsAmount = pointsToEarn,
                Type = LoyaltyTransactionType.Earned,
                Description = $"Wizyta (ID: {reservationId})",
                ReservationId = reservationId,
                CreatedAt = DateTime.UtcNow
            };

            _context.LoyaltyTransactions.Add(loyaltyTransaction);
            await _context.SaveChangesAsync();
        }

        private async Task<(decimal discount, string? error)> RedeemLoyaltyPoints(int businessId, string customerId, int pointsToUse, decimal priceAfterDiscounts)
        {
            if (pointsToUse <= 0) return (0, null);

            var loyaltyPoint = await _context.LoyaltyPoints
                .FirstOrDefaultAsync(p => p.BusinessId == businessId && p.CustomerId == customerId);

            if (loyaltyPoint == null || loyaltyPoint.PointsBalance < pointsToUse)
                return (0, "Nie masz wystarczającej liczby punktów lojalnościowych.");

            decimal discount = (decimal)pointsToUse;

            if (priceAfterDiscounts - discount < 1.00m)
            {
                discount = priceAfterDiscounts - 1.00m;
                if (discount < 0) discount = 0;
            }

            int actualPointsUsed = (int)discount;
            if (actualPointsUsed <= 0) return (0, null);

            loyaltyPoint.PointsBalance -= actualPointsUsed;
            loyaltyPoint.LastUpdated = DateTime.UtcNow;

            var loyaltyTransaction = new LoyaltyTransaction
            {
                LoyaltyPoint = loyaltyPoint,
                PointsAmount = actualPointsUsed,
                Type = LoyaltyTransactionType.Redeemed,
                Description = $"Wykorzystano {actualPointsUsed} pkt przy rezerwacji",
                CreatedAt = DateTime.UtcNow
            };

            _context.LoyaltyTransactions.Add(loyaltyTransaction);
            await _context.SaveChangesAsync();

            return (discount, null);
        }
    }
}
