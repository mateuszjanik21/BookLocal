using BookLocal.API.DTOs;
using BookLocal.API.Interfaces;
using BookLocal.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BookLocal.API.Services
{
    public class FinanceService : IFinanceService
    {
        private readonly AppDbContext _context;

        public FinanceService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<(bool Success, IEnumerable<FinanceReportSqlDto>? Data, string? ErrorMessage)> GetLiveReportsAsync(int businessId, DateOnly startDate, DateOnly endDate, ClaimsPrincipal user)
        {
            var ownerId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!await _context.Businesses.AnyAsync(b => b.BusinessId == businessId && b.OwnerId == ownerId))
                return (false, null, "Brak uprawnień.");

            var reports = await _context.Database
                .SqlQueryRaw<FinanceReportSqlDto>(
                    "EXEC GetFinanceReport @BusinessId, @StartDate, @EndDate",
                    new Microsoft.Data.SqlClient.SqlParameter("@BusinessId", businessId),
                    new Microsoft.Data.SqlClient.SqlParameter("@StartDate", startDate.ToDateTime(TimeOnly.MinValue)),
                    new Microsoft.Data.SqlClient.SqlParameter("@EndDate", endDate.ToDateTime(TimeOnly.MinValue))
                )
                .ToListAsync();

            return (true, reports, null);
        }

        public async Task<(bool Success, string? Message, string? ErrorMessage)> GenerateReportRangeAsync(int businessId, DateOnly startDate, DateOnly endDate, ClaimsPrincipal user)
        {
            var ownerId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!await _context.Businesses.AnyAsync(b => b.BusinessId == businessId && b.OwnerId == ownerId))
                return (false, null, "Brak uprawnień.");

            if (endDate < startDate) return (false, null, "Data końcowa nie może być wcześniejsza niż początkowa.");
            if (startDate.AddDays(31) < endDate) return (false, null, "Maksymalny zakres to 31 dni.");

            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                await GenerateDailyReportInternal(businessId, date);
            }

            return (true, "Raporty wygenerowane.", null);
        }

        public async Task<(bool Success, string? Message, string? ErrorMessage)> DeleteReportAsync(int businessId, DateOnly date, ClaimsPrincipal user)
        {
            var ownerId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!await _context.Businesses.AnyAsync(b => b.BusinessId == businessId && b.OwnerId == ownerId))
                return (false, null, "Brak uprawnień.");

            var report = await _context.DailyFinancialReports
                .FirstOrDefaultAsync(r => r.BusinessId == businessId && r.ReportDate == date);

            if (report != null)
            {
                _context.DailyFinancialReports.Remove(report);
                await _context.SaveChangesAsync();
                return (true, "Raport usunięty.", null);
            }

            return (false, null, "Raport nie istnieje.");
        }

        public async Task<(bool Success, DailyFinancialReport? Data, string? ErrorMessage)> GenerateDailyReportAsync(int businessId, DateOnly date, ClaimsPrincipal user)
        {
            var ownerId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            var business = await _context.Businesses
                .FirstOrDefaultAsync(b => b.BusinessId == businessId && b.OwnerId == ownerId);

            if (business == null) return (false, null, "Brak uprawnień.");

            var report = await GenerateDailyReportInternal(businessId, date);
            return (true, report, null);
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
            var endOfDay = date.ToDateTime(new TimeOnly(23, 59, 59));

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
                .Where(s => s.BusinessId == businessId && s.StartDate <= date.ToDateTime(new TimeOnly(23, 59, 59)) && s.EndDate >= date.ToDateTime(TimeOnly.MinValue))
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

        public async Task<(bool Success, IEnumerable<DailyFinancialReport>? Data, string? ErrorMessage)> GetReportsAsync(int businessId, int month, int year, ClaimsPrincipal user)
        {
            var ownerId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!await _context.Businesses.AnyAsync(b => b.BusinessId == businessId && b.OwnerId == ownerId))
                return (false, null, "Brak uprawnień.");

            var reports = await _context.DailyFinancialReports
               .AsNoTracking()
               .Where(r => r.BusinessId == businessId && r.ReportDate.Month == month && r.ReportDate.Year == year)
               .OrderByDescending(r => r.ReportDate)
               .ToListAsync();

            return (true, reports, null);
        }

        public async Task<(bool Success, IEnumerable<DailyEmployeePerformanceDto>? Data, string? ErrorMessage)> GetEmployeePerformanceAsync(int businessId, DateOnly? date, DateOnly? startDate, DateOnly? endDate, ClaimsPrincipal user)
        {
            var ownerId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!await _context.Businesses.AnyAsync(b => b.BusinessId == businessId && b.OwnerId == ownerId))
                return (false, null, "Brak uprawnień.");

            DateTime startDateTime;
            DateTime endDateTime;

            if (startDate.HasValue && endDate.HasValue)
            {
                startDateTime = startDate.Value.ToDateTime(TimeOnly.MinValue);
                endDateTime = endDate.Value.ToDateTime(new TimeOnly(23, 59, 59));
            }
            else if (date.HasValue)
            {
                startDateTime = date.Value.ToDateTime(TimeOnly.MinValue);
                endDateTime = date.Value.ToDateTime(new TimeOnly(23, 59, 59));
            }
            else
            {
                return (false, null, "Wymagana data lub zakres dat.");
            }

            var reservations = await _context.Reservations
                .AsNoTracking()
                .Include(r => r.Employee)
                .Include(r => r.ServiceVariant)
                .Where(r => r.BusinessId == businessId &&
                            r.StartTime >= startDateTime &&
                            r.StartTime <= endDateTime)
                .ToListAsync();

            var reservationIds = reservations.Select(r => r.ReservationId).ToList();

            var payments = await _context.Payments
                .AsNoTracking()
                .Where(p => reservationIds.Contains(p.ReservationId) && p.Status == PaymentStatus.Completed)
                .ToListAsync();

            var reviews = await _context.Reviews
                .AsNoTracking()
                .Where(r => r.ReservationId.HasValue && reservationIds.Contains(r.ReservationId.Value))
                .ToListAsync();

            var performanceList = new List<DailyEmployeePerformanceDto>();

            var allEmployees = await _context.Employees
                .AsNoTracking()
                .Include(e => e.FinanceSettings)
                .Where(e => e.BusinessId == businessId && !e.IsArchived)
                .ToListAsync();

            foreach (var emp in allEmployees)
            {
                var empReservations = reservations.Where(r => r.EmployeeId == emp.EmployeeId).ToList();

                var completed = empReservations.Where(r => r.Status == ReservationStatus.Completed).ToList();
                var cancelled = empReservations.Where(r => r.Status == ReservationStatus.Cancelled).ToList();

                var completedReservationIds = completed.Select(r => r.ReservationId).ToList();
                decimal revenue = payments
                    .Where(p => completedReservationIds.Contains(p.ReservationId))
                    .Sum(p => p.Amount);

                decimal empCommRate = emp.FinanceSettings?.CommissionPercentage ?? 0;
                decimal commission = revenue * (empCommRate / 100m);

                var empReservationIds = completed.Select(r => r.ReservationId).ToList();
                var empReviews = reviews.Where(r => r.ReservationId.HasValue && empReservationIds.Contains(r.ReservationId.Value)).ToList();

                double rating = 0;
                if (empReviews.Any())
                {
                    rating = empReviews.Average(r => r.Rating);
                }

                performanceList.Add(new DailyEmployeePerformanceDto
                {
                    EmployeeId = emp.EmployeeId,
                    Date = startDate ?? date ?? DateOnly.MinValue,
                    FullName = $"{emp.FirstName} {emp.LastName}",
                    TotalAppointments = empReservations.Count,
                    CompletedAppointments = completed.Count,
                    CancelledAppointments = cancelled.Count,
                    TotalRevenue = revenue,
                    Commission = commission,
                    AverageRating = rating,
                    PhotoUrl = emp.PhotoUrl
                });
            }

            return (true, performanceList, null);
        }
    }
}
