using BookLocal.API.DTOs;
using BookLocal.API.Interfaces;
using BookLocal.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BookLocal.API.Services
{
    public class SubscriptionService : ISubscriptionService
    {
        private readonly AppDbContext _context;

        public SubscriptionService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<(bool Success, IEnumerable<SubscriptionPlanDto>? Data, string? ErrorMessage)> GetPublicPlansAsync()
        {
            var plans = await _context.SubscriptionPlans
                .AsNoTracking()
                .Where(p => p.IsActive)
                .ToListAsync();

            var dtos = plans.Select(p => new SubscriptionPlanDto
            {
                PlanId = p.PlanId,
                Name = p.Name,
                PriceMonthly = p.PriceMonthly,
                PriceYearly = p.PriceYearly,
                MaxEmployees = p.MaxEmployees,
                MaxServices = p.MaxServices,
                HasAdvancedReports = p.HasAdvancedReports,
                HasMarketingTools = p.HasMarketingTools,
                CommissionPercentage = p.CommissionPercentage,
                IsActive = p.IsActive
            }).ToList();

            return (true, dtos, null);
        }

        public async Task<(bool Success, string? Message, string? ErrorMessage)> SubscribeAsync(int planId, ClaimsPrincipal user)
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            var business = await _context.Businesses.FirstOrDefaultAsync(b => b.OwnerId == userId);
            if (business == null) return (false, null, "Nie znaleziono firmy dla tego użytkownika.");

            var plan = await _context.SubscriptionPlans.FindAsync(planId);
            if (plan == null || !plan.IsActive) return (false, null, "Nieprawidłowy plan.");

            var currentEmployeeCount = await _context.Employees.CountAsync(e => e.BusinessId == business.BusinessId && !e.IsArchived);
            var currentServiceCount = await _context.Services.CountAsync(s => s.BusinessId == business.BusinessId && !s.IsArchived);

            if (currentEmployeeCount > plan.MaxEmployees)
            {
                return (false, null, $"Nie można zmienić planu. Twój obecny stan pracowników ({currentEmployeeCount}) przekracza limit nowego planu ({plan.MaxEmployees}). Aby zmienić plan, zmniejsz liczbę pracowników.");
            }

            if (currentServiceCount > plan.MaxServices)
            {
                return (false, null, $"Nie można zmienić planu. Twoja obecna liczba usług ({currentServiceCount}) przekracza limit nowego planu ({plan.MaxServices}). Aby zmienić plan, usuń zbędne usługi.");
            }

            var currentSub = await _context.BusinessSubscriptions
                .FirstOrDefaultAsync(bs => bs.BusinessId == business.BusinessId && bs.IsActive);

            if (currentSub != null)
            {
                currentSub.IsActive = false;
                currentSub.EndDate = DateTime.UtcNow;
            }

            var newSub = new BusinessSubscription
            {
                BusinessId = business.BusinessId,
                PlanId = planId,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(1),
                IsActive = true,
                IsAutoRenew = true
            };

            _context.BusinessSubscriptions.Add(newSub);
            await _context.SaveChangesAsync();

            return (true, $"Pomyślnie zasubskrybowano plan {plan.Name}.", null);
        }

        public async Task<(bool Success, object? Data, string? ErrorMessage)> GetCurrentSubscriptionAsync(ClaimsPrincipal user)
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            var business = await _context.Businesses.AsNoTracking().FirstOrDefaultAsync(b => b.OwnerId == userId);
            if (business == null) return (false, null, "Nie znaleziono firmy.");

            var sub = await _context.BusinessSubscriptions
                .Include(bs => bs.Plan)
                .AsNoTracking()
                .FirstOrDefaultAsync(bs => bs.BusinessId == business.BusinessId && bs.IsActive);

            if (sub == null)
            {
                return (true, new { PlanName = "Brak", IsActive = false }, null);
            }

            return (true, new
            {
                PlanName = sub.Plan.Name,
                sub.PlanId,
                sub.StartDate,
                sub.EndDate,
                Price = sub.Plan.PriceMonthly,
                IsActive = true,
                sub.Plan.HasAdvancedReports,
                sub.Plan.HasMarketingTools
            }, null);
        }
    }
}
