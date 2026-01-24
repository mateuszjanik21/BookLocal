using BookLocal.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BookLocal.API.Controllers
{
    [ApiController]
    [Route("api/businesses/{businessId}/finance")]
    [Authorize(Roles = "owner")]
    public class FinanceController : ControllerBase
    {
        private readonly AppDbContext _context;

        public FinanceController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("generate-range")]
        public async Task<ActionResult> GenerateReportRange(int businessId, [FromQuery] DateOnly startDate, [FromQuery] DateOnly endDate)
        {
            var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!await _context.Businesses.AnyAsync(b => b.BusinessId == businessId && b.OwnerId == ownerId))
                return Forbid();

            if (endDate < startDate) return BadRequest("Data końcowa nie może być wcześniejsza niż początkowa.");
            if (startDate.AddDays(31) < endDate) return BadRequest("Maksymalny zakres to 31 dni.");

            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                await GenerateDailyReportInternal(businessId, date);
            }

            return Ok(new { message = "Raporty wygenerowane." });
        }

        [HttpDelete("report")]
        public async Task<ActionResult> DeleteReport(int businessId, [FromQuery] DateOnly date)
        {
            var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!await _context.Businesses.AnyAsync(b => b.BusinessId == businessId && b.OwnerId == ownerId))
                return Forbid();

            var report = await _context.DailyFinancialReports
                .FirstOrDefaultAsync(r => r.BusinessId == businessId && r.ReportDate == date);

            if (report != null)
            {
                _context.DailyFinancialReports.Remove(report);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Raport usunięty." });
            }

            return NotFound("Raport nie istnieje.");
        }

        [HttpPost("generate-daily-report")]
        public async Task<ActionResult<DailyFinancialReport>> GenerateDailyReport(int businessId, [FromQuery] DateOnly date)
        {
            var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var business = await _context.Businesses
                .FirstOrDefaultAsync(b => b.BusinessId == businessId && b.OwnerId == ownerId);

            if (business == null) return Forbid();

            return Ok(await GenerateDailyReportInternal(businessId, date));
        }

        private async Task<DailyFinancialReport> GenerateDailyReportInternal(int businessId, DateOnly date)
        {
            // Clean existing
            var existingReport = await _context.DailyFinancialReports
                .FirstOrDefaultAsync(r => r.BusinessId == businessId && r.ReportDate == date);

            if (existingReport != null)
            {
                _context.DailyFinancialReports.Remove(existingReport);
            }

            var startOfDay = date.ToDateTime(TimeOnly.MinValue);
            var endOfDay = date.ToDateTime(TimeOnly.MaxValue);

            var reservations = await _context.Reservations
                .Include(r => r.ServiceVariant)
                .Where(r => r.BusinessId == businessId &&
                            r.StartTime >= startOfDay &&
                            r.StartTime <= endOfDay)
                .ToListAsync();

            var completedReservations = reservations.Where(r => r.Status == ReservationStatus.Completed).ToList();
            var completedReservationIds = completedReservations.Select(r => r.ReservationId).ToList();

            var payments = await _context.Payments
                .Where(p => completedReservationIds.Contains(p.ReservationId) && p.Status == PaymentStatus.Completed)
                .ToListAsync();

            var report = new DailyFinancialReport
            {
                BusinessId = businessId,
                ReportDate = date,
                TotalRevenue = payments.Sum(p => p.Amount), // Revenue based on actual payments
                TipsAmount = 0, // Placeholder

                CashRevenue = payments
                    .Where(p => p.PaymentMethod == PaymentMethod.Cash)
                    .Sum(p => p.Amount),

                CardRevenue = payments
                    .Where(p => p.PaymentMethod == PaymentMethod.Card)
                    .Sum(p => p.Amount),

                OnlineRevenue = payments
                    .Where(p => p.PaymentMethod == PaymentMethod.Online)
                    .Sum(p => p.Amount),

                TotalCommission = payments.Sum(p => p.CommissionAmount),

                TotalAppointments = reservations.Count,
                CompletedAppointments = completedReservations.Count,
                CancelledAppointments = reservations.Count(r => r.Status == ReservationStatus.Cancelled),
                NoShowCount = reservations.Count(r => r.Status == ReservationStatus.NoShow),

                NewCustomersCount = 0,
                ReturningCustomersCount = 0,
                OccupancyRate = 0
            };

            report.AverageTicketValue = report.CompletedAppointments > 0
                ? report.TotalRevenue / report.CompletedAppointments
                : 0;

            var topService = completedReservations
                .GroupBy(r => r.ServiceVariant.Name)
                .OrderByDescending(g => g.Count())
                .FirstOrDefault();

            report.TopSellingServiceName = topService?.Key;

            // Advanced Metrics Implementation
            var customerIdsToday = completedReservations
                .Where(r => r.CustomerId != null)
                .Select(r => r.CustomerId)
                .Distinct()
                .ToList();

            if (customerIdsToday.Any())
            {
                var returningCount = await _context.Reservations
                    .Where(r => r.BusinessId == businessId &&
                                customerIdsToday.Contains(r.CustomerId) &&
                                r.Status == ReservationStatus.Completed &&
                                r.StartTime < startOfDay)
                    .Select(r => r.CustomerId)
                    .Distinct()
                    .CountAsync();

                report.NewCustomersCount = customerIdsToday.Count - returningCount;
                report.ReturningCustomersCount = returningCount;
            }

            _context.DailyFinancialReports.Add(report);
            await _context.SaveChangesAsync();

            return report;
        }

        [HttpGet("reports")]
        public async Task<ActionResult<IEnumerable<DailyFinancialReport>>> GetReports(int businessId, [FromQuery] int month, [FromQuery] int year)
        {
            var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!await _context.Businesses.AnyAsync(b => b.BusinessId == businessId && b.OwnerId == ownerId))
                return Forbid();

            var reports = await _context.DailyFinancialReports
               .Where(r => r.BusinessId == businessId && r.ReportDate.Month == month && r.ReportDate.Year == year)
               .OrderByDescending(r => r.ReportDate)
               .ToListAsync();

            return Ok(reports);
        }
    }
}
