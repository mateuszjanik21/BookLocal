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

            var activeSubscription = await _context.BusinessSubscriptions
                .Include(s => s.Plan)
                .Where(s => s.BusinessId == businessId && s.StartDate <= date.ToDateTime(TimeOnly.MaxValue) && s.EndDate >= date.ToDateTime(TimeOnly.MinValue))
                .OrderByDescending(s => s.SubscriptionId)
                .FirstOrDefaultAsync();

            decimal commRate = activeSubscription?.Plan?.CommissionPercentage ?? 0m;
            decimal platformFee = payments.Sum(p => p.Amount) * (commRate / 100m);

            var report = new DailyFinancialReport
            {
                BusinessId = businessId,
                ReportDate = date,
                TotalRevenue = payments.Sum(p => p.Amount),
                TipsAmount = 0,

                CashRevenue = payments
                    .Where(p => p.PaymentMethod == PaymentMethod.Cash)
                    .Sum(p => p.Amount),

                CardRevenue = payments
                    .Where(p => p.PaymentMethod == PaymentMethod.Card)
                    .Sum(p => p.Amount),

                OnlineRevenue = payments
                    .Where(p => p.PaymentMethod == PaymentMethod.Online)
                    .Sum(p => p.Amount),

                TotalCommission = platformFee,

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
        [HttpGet("employee-performance")]
        public async Task<ActionResult<IEnumerable<DTOs.DailyEmployeePerformanceDto>>> GetEmployeePerformance(
            int businessId,
            [FromQuery] DateOnly? date,
            [FromQuery] DateOnly? startDate,
            [FromQuery] DateOnly? endDate)
        {
            var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!await _context.Businesses.AnyAsync(b => b.BusinessId == businessId && b.OwnerId == ownerId))
                return Forbid();

            DateTime startDateTime;
            DateTime endDateTime;

            if (startDate.HasValue && endDate.HasValue)
            {
                startDateTime = startDate.Value.ToDateTime(TimeOnly.MinValue);
                endDateTime = endDate.Value.ToDateTime(TimeOnly.MaxValue);
            }
            else if (date.HasValue)
            {
                startDateTime = date.Value.ToDateTime(TimeOnly.MinValue);
                endDateTime = date.Value.ToDateTime(TimeOnly.MaxValue);
            }
            else
            {
                return BadRequest("Wymagana data lub zakres dat.");
            }

            var reservations = await _context.Reservations
                .Include(r => r.Employee)
                .Include(r => r.ServiceVariant)
                .Where(r => r.BusinessId == businessId &&
                            r.StartTime >= startDateTime &&
                            r.StartTime <= endDateTime)
                .ToListAsync();

            var reservationIds = reservations.Select(r => r.ReservationId).ToList();
            var reviews = await _context.Reviews
                .Where(r => r.ReservationId.HasValue && reservationIds.Contains(r.ReservationId.Value))
                .ToListAsync();

            var performanceList = new List<DTOs.DailyEmployeePerformanceDto>();

            var allEmployees = await _context.Employees
                .Include(e => e.FinanceSettings)
                .Where(e => e.BusinessId == businessId && !e.IsArchived)
                .ToListAsync();

            foreach (var emp in allEmployees)
            {
                var empReservations = reservations.Where(r => r.EmployeeId == emp.EmployeeId).ToList();

                var completed = empReservations.Where(r => r.Status == ReservationStatus.Completed).ToList();
                var cancelled = empReservations.Where(r => r.Status == ReservationStatus.Cancelled).ToList();

                decimal revenue = completed.Sum(r => r.ServiceVariant.Price);

                decimal empCommRate = emp.FinanceSettings?.CommissionPercentage ?? 0;
                decimal commission = revenue * (empCommRate / 100m);

                var empReservationIds = completed.Select(r => r.ReservationId).ToList();
                var empReviews = reviews.Where(r => r.ReservationId.HasValue && empReservationIds.Contains(r.ReservationId.Value)).ToList();

                double rating = 0;
                if (empReviews.Any())
                {
                    rating = empReviews.Average(r => r.Rating);
                }

                performanceList.Add(new DTOs.DailyEmployeePerformanceDto
                {
                    EmployeeId = emp.EmployeeId,
                    Date = startDate ?? date.Value,
                    FullName = $"{emp.FirstName} {emp.LastName}",
                    TotalAppointments = empReservations.Count,
                    CompletedAppointments = completed.Count,
                    CancelledAppointments = cancelled.Count,
                    TotalRevenue = revenue,
                    Commission = commission,
                    AverageRating = rating
                });
            }

            return Ok(performanceList);
        }
    }
}
