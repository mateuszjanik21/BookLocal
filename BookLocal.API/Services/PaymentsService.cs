using BookLocal.API.DTOs;
using BookLocal.API.Interfaces;
using BookLocal.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BookLocal.API.Services
{
    public class PaymentsService : IPaymentsService
    {
        private readonly AppDbContext _context;

        public PaymentsService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<(bool Success, string? Message, string? ErrorMessage, int StatusCode)> CreatePaymentAsync(CreatePaymentDto paymentDto, ClaimsPrincipal user)
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);

            var reservation = await _context.Reservations
                .Include(r => r.Business)
                .FirstOrDefaultAsync(r => r.ReservationId == paymentDto.ReservationId);

            if (reservation == null) return (false, null, "Nie znaleziono rezerwacji.", 404);

            if (reservation.Business.OwnerId != userId && reservation.CustomerId != userId)
                return (false, null, "Brak uprawnień.", 403);

            var payment = new Payment
            {
                ReservationId = paymentDto.ReservationId,
                BusinessId = reservation.BusinessId,
                PaymentMethod = paymentDto.Method,
                Amount = paymentDto.Amount,
                Status = PaymentStatus.Completed,
                TransactionDate = DateTime.UtcNow
            };

            if (paymentDto.Method == PaymentMethod.Online)
            {
                var activeSubscription = await _context.BusinessSubscriptions
                    .Include(s => s.Plan)
                    .FirstOrDefaultAsync(s => s.BusinessId == reservation.BusinessId && s.IsActive);

                if (activeSubscription != null)
                {
                    var commissionRate = (decimal)activeSubscription.Plan.CommissionPercentage / 100m;
                    payment.CommissionAmount = Math.Round(paymentDto.Amount * commissionRate, 2);
                }
            }

            reservation.PaymentMethod = paymentDto.Method;

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            return (true, "Płatność została dodana.", null, 200);
        }

        public async Task<(bool Success, object? Data, string? ErrorMessage, int StatusCode)> GetBusinessPaymentsAsync(int businessId, int page, int pageSize, string? sort, string? sortDir, string? methodFilter, string? statusFilter, ClaimsPrincipal user)
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            var business = await _context.Businesses.FindAsync(businessId);

            if (business == null) return (false, null, "Nie znaleziono firmy.", 404);
            if (business.OwnerId != userId) return (false, null, "Brak uprawnień.", 403);

            var query = _context.Payments
                .AsNoTracking()
                .Include(p => p.Reservation)
                    .ThenInclude(r => r.Customer)
                .Include(p => p.Reservation)
                    .ThenInclude(r => r.ServiceVariant)
                        .ThenInclude(sv => sv.Service)
                .Where(p => p.BusinessId == businessId);

            if (!string.IsNullOrWhiteSpace(methodFilter))
            {
                if (Enum.TryParse<PaymentMethod>(methodFilter, out var method))
                    query = query.Where(p => p.PaymentMethod == method);
            }

            if (!string.IsNullOrWhiteSpace(statusFilter))
            {
                if (Enum.TryParse<PaymentStatus>(statusFilter, out var status))
                    query = query.Where(p => p.Status == status);
            }

            var totalCount = await query.CountAsync();

            query = sort?.ToLower() switch
            {
                "id" => sortDir == "asc" ? query.OrderBy(p => p.PaymentId) : query.OrderByDescending(p => p.PaymentId),
                "amount" => sortDir == "asc" ? query.OrderBy(p => p.Amount) : query.OrderByDescending(p => p.Amount),
                "date" => sortDir == "asc" ? query.OrderBy(p => p.TransactionDate) : query.OrderByDescending(p => p.TransactionDate),
                _ => query.OrderByDescending(p => p.TransactionDate)
            };

            var payments = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new PaymentDto
                {
                    PaymentId = p.PaymentId,
                    ReservationId = p.ReservationId,
                    BusinessId = p.BusinessId,
                    Method = p.PaymentMethod.ToString(),
                    Amount = p.Amount,
                    CommissionAmount = p.CommissionAmount,
                    Currency = p.Currency,
                    TransactionDate = p.TransactionDate,
                    Status = p.Status.ToString(),
                    CustomerName = p.Reservation.Customer != null
                        ? p.Reservation.Customer.FirstName + " " + p.Reservation.Customer.LastName
                        : "Gość",
                    ServiceName = p.Reservation.ServiceVariant != null && p.Reservation.ServiceVariant.Service != null
                        ? p.Reservation.ServiceVariant.Service.Name + " - " + p.Reservation.ServiceVariant.Name
                        : "—",
                    ReservationAmount = p.Reservation.AgreedPrice
                })
                .ToListAsync();

            var result = new
            {
                Items = payments,
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };

            return (true, result, null, 200);
        }

        public async Task<(bool Success, IEnumerable<PaymentDto>? Data, string? ErrorMessage, int StatusCode)> GetReservationPaymentsAsync(int reservationId, ClaimsPrincipal user)
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            var reservation = await _context.Reservations
                .Include(r => r.Business)
                .FirstOrDefaultAsync(r => r.ReservationId == reservationId);

            if (reservation == null) return (false, null, "Nie znaleziono rezerwacji.", 404);

            if (reservation.CustomerId != userId && reservation.Business.OwnerId != userId)
                return (false, null, "Brak uprawnień.", 403);

            var query = _context.Payments.AsNoTracking().AsQueryable();

            if (reservation.ServiceBundleId.HasValue)
            {
                var bundleReservationIds = await _context.Reservations
                    .Where(r => r.ServiceBundleId == reservation.ServiceBundleId &&
                                r.CustomerId == reservation.CustomerId &&
                                r.StartTime.Date == reservation.StartTime.Date)
                    .Select(r => r.ReservationId)
                    .ToListAsync();

                query = query.Where(p => bundleReservationIds.Contains(p.ReservationId));
            }
            else
            {
                query = query.Where(p => p.ReservationId == reservationId);
            }

            var payments = await query
                .OrderByDescending(p => p.TransactionDate)
                .Select(p => new PaymentDto
                {
                    PaymentId = p.PaymentId,
                    ReservationId = p.ReservationId,
                    BusinessId = p.BusinessId,
                    Method = p.PaymentMethod.ToString(),
                    Amount = p.Amount,
                    Currency = p.Currency,
                    TransactionDate = p.TransactionDate,
                    Status = p.Status.ToString()
                })
                .ToListAsync();

            return (true, payments, null, 200);
        }

        public async Task<(bool Success, string? Message, string? ErrorMessage, int StatusCode)> UpdatePaymentAsync(int id, UpdatePaymentDto dto, ClaimsPrincipal user)
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            var payment = await _context.Payments
                .Include(p => p.Business)
                .FirstOrDefaultAsync(p => p.PaymentId == id);

            if (payment == null) return (false, null, "Płatność nie istnieje.", 404);
            if (payment.Business.OwnerId != userId) return (false, null, "Brak uprawnień.", 403);

            payment.Amount = dto.Amount;
            payment.PaymentMethod = dto.Method;
            payment.Status = dto.Status;

            await _context.SaveChangesAsync();
            return (true, "Płatność została zaktualizowana.", null, 200);
        }

        public async Task<(bool Success, string? Message, string? ErrorMessage, int StatusCode)> DeletePaymentAsync(int id, ClaimsPrincipal user)
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            var payment = await _context.Payments
                .Include(p => p.Business)
                .FirstOrDefaultAsync(p => p.PaymentId == id);

            if (payment == null) return (false, null, "Płatność nie istnieje.", 404);
            if (payment.Business.OwnerId != userId) return (false, null, "Brak uprawnień.", 403);

            _context.Payments.Remove(payment);
            await _context.SaveChangesAsync();
            return (true, "Płatność została usunięta.", null, 200);
        }
    }
}
