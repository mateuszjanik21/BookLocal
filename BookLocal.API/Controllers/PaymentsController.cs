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

        // GET: api/payments/business/5
        [HttpGet("business/{businessId}")]
        [Authorize(Roles = "owner")]
        public async Task<ActionResult<IEnumerable<PaymentDto>>> GetBusinessPayments(int businessId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var business = await _context.Businesses.FindAsync(businessId);

            if (business == null) return NotFound();
            if (business.OwnerId != userId) return Forbid();

            var payments = await _context.Payments
                .Where(p => p.BusinessId == businessId)
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
    }
}
