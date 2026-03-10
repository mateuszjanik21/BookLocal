using BookLocal.API.DTOs;
using BookLocal.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BookLocal.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PaymentsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;

        public PaymentsController(AppDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // POST: api/payments
        [HttpPost]
        public async Task<IActionResult> CreatePayment(CreatePaymentDto paymentDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var reservation = await _context.Reservations
                .Include(r => r.Business)
                .FirstOrDefaultAsync(r => r.ReservationId == paymentDto.ReservationId);

            if (reservation == null) return NotFound("Nie znaleziono rezerwacji.");

            // Only Owner of the business or the Customer can make payments (Customer logic simplified for now)
            // For now, assume this endpoint is mostly for Owners adding payments manually
            if (reservation.Business.OwnerId != userId && reservation.CustomerId != userId)
                return Forbid();

            var payment = new Payment
            {
                ReservationId = paymentDto.ReservationId,
                BusinessId = reservation.BusinessId,
                PaymentMethod = paymentDto.Method,
                Amount = paymentDto.Amount,
                Status = PaymentStatus.Completed,
                TransactionDate = DateTime.UtcNow
            };

            // Calculate Commission for Online payments
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

            return Ok(new { Message = "Płatność została dodana." });
        }

        [HttpGet("business/{businessId}")]
        [Authorize(Roles = "owner")]
        public async Task<IActionResult> GetBusinessPayments(
            int businessId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 15,
            [FromQuery] string? sort = null,
            [FromQuery] string? sortDir = null,
            [FromQuery] string? methodFilter = null,
            [FromQuery] string? statusFilter = null)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var business = await _context.Businesses.FindAsync(businessId);

            if (business == null) return NotFound();
            if (business.OwnerId != userId) return Forbid();

            var query = _context.Payments
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

            return Ok(new
            {
                Items = payments,
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            });
        }

        // GET: api/payments/reservation/10
        [HttpGet("reservation/{reservationId}")]
        public async Task<ActionResult<IEnumerable<PaymentDto>>> GetReservationPayments(int reservationId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var reservation = await _context.Reservations
                .Include(r => r.Business)
                .FirstOrDefaultAsync(r => r.ReservationId == reservationId);

            if (reservation == null) return NotFound();

            if (reservation.CustomerId != userId && reservation.Business.OwnerId != userId)
                return Forbid();

            var payments = await _context.Payments
                .Where(p => p.ReservationId == reservationId)
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

            return Ok(payments);
        }
        // PUT: api/payments/5
        [HttpPut("{id}")]
        [Authorize(Roles = "owner")]
        public async Task<IActionResult> UpdatePayment(int id, [FromBody] UpdatePaymentDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var payment = await _context.Payments
                .Include(p => p.Business)
                .FirstOrDefaultAsync(p => p.PaymentId == id);

            if (payment == null) return NotFound("Płatność nie istnieje.");
            if (payment.Business.OwnerId != userId) return Forbid();

            payment.Amount = dto.Amount;
            payment.PaymentMethod = dto.Method;
            payment.Status = dto.Status;

            await _context.SaveChangesAsync();
            return Ok(new { Message = "Płatność została zaktualizowana." });
        }

        // DELETE: api/payments/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "owner")]
        public async Task<IActionResult> DeletePayment(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var payment = await _context.Payments
                .Include(p => p.Business)
                .FirstOrDefaultAsync(p => p.PaymentId == id);

            if (payment == null) return NotFound("Płatność nie istnieje.");
            if (payment.Business.OwnerId != userId) return Forbid();

            _context.Payments.Remove(payment);
            await _context.SaveChangesAsync();
            return Ok(new { Message = "Płatność została usunięta." });
        }
    }
}
