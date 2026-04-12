using BookLocal.API.DTOs;
using BookLocal.API.Interfaces;
using BookLocal.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace BookLocal.API.Services
{
    public class AdminService : IAdminService
    {
        private readonly AppDbContext _context;

        public AdminService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<SubscriptionPlanDto>> GetPlansAsync()
        {
            var plans = await _context.SubscriptionPlans.ToListAsync();

            return plans.Select(p => new SubscriptionPlanDto
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
        }

        public async Task<SubscriptionPlanDto> CreatePlanAsync(CreateSubscriptionPlanDto dto)
        {
            var plan = new SubscriptionPlan
            {
                Name = dto.Name,
                PriceMonthly = dto.PriceMonthly,
                PriceYearly = dto.PriceYearly,
                MaxEmployees = dto.MaxEmployees,
                MaxServices = dto.MaxServices,
                HasAdvancedReports = dto.HasAdvancedReports,
                HasMarketingTools = dto.HasMarketingTools,
                CommissionPercentage = dto.CommissionPercentage,
                IsActive = true
            };

            _context.SubscriptionPlans.Add(plan);
            await _context.SaveChangesAsync();

            return new SubscriptionPlanDto
            {
                PlanId = plan.PlanId,
                Name = plan.Name,
                PriceMonthly = plan.PriceMonthly,
                PriceYearly = plan.PriceYearly,
                MaxEmployees = plan.MaxEmployees,
                MaxServices = plan.MaxServices,
                HasAdvancedReports = plan.HasAdvancedReports,
                HasMarketingTools = plan.HasMarketingTools,
                CommissionPercentage = plan.CommissionPercentage,
                IsActive = plan.IsActive
            };
        }

        public async Task<bool> UpdatePlanAsync(int id, CreateSubscriptionPlanDto dto)
        {
            var plan = await _context.SubscriptionPlans.FindAsync(id);
            if (plan == null) return false;

            plan.Name = dto.Name;
            plan.PriceMonthly = dto.PriceMonthly;
            plan.PriceYearly = dto.PriceYearly;
            plan.MaxEmployees = dto.MaxEmployees;
            plan.MaxServices = dto.MaxServices;
            plan.HasAdvancedReports = dto.HasAdvancedReports;
            plan.HasMarketingTools = dto.HasMarketingTools;
            plan.CommissionPercentage = dto.CommissionPercentage;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeletePlanAsync(int id)
        {
            var plan = await _context.SubscriptionPlans.FindAsync(id);
            if (plan == null) return false;

            plan.IsActive = false;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<IEnumerable<AdminBusinessListDto>> GetBusinessesAsync(string? status)
        {
            var query = _context.Businesses
                .Include(b => b.Owner)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status) && Enum.TryParse<VerificationStatus>(status, true, out var statusEnum))
            {
                query = query.Where(b => b.VerificationStatus == statusEnum);
            }

            var businesses = await query.ToListAsync();

            var businessIds = businesses.Select(b => b.BusinessId).ToList();
            var activeSubs = await _context.BusinessSubscriptions
                .Include(bs => bs.Plan)
                .Where(bs => businessIds.Contains(bs.BusinessId) && bs.IsActive)
                .ToListAsync();

            return businesses.Select(b =>
            {
                var sub = activeSubs.FirstOrDefault(s => s.BusinessId == b.BusinessId);
                return new AdminBusinessListDto
                {
                    BusinessId = b.BusinessId,
                    Name = b.Name ?? "Brak nazwy",
                    OwnerEmail = b.Owner.Email,
                    CreatedAt = b.CreatedAt,
                    IsVerified = b.IsVerified,
                    VerificationStatus = b.VerificationStatus.ToString(),
                    SubscriptionPlanName = sub?.Plan?.Name ?? "Brak (Free?)"
                };
            }).ToList();
        }

        public async Task<(bool Success, string Message, string? RejectionReason)> VerifyBusinessAsync(int id, VerifyBusinessDto dto)
        {
            var business = await _context.Businesses.FindAsync(id);
            if (business == null) return (false, "Nie znaleziono firmy.", null);

            if (dto.IsApproved)
            {
                business.IsVerified = true;
                business.VerificationStatus = VerificationStatus.Approved;
                var verification = new BusinessVerification
                {
                    BusinessId = id,
                    Status = VerificationStatus.Approved,
                    ReviewedAt = DateTime.UtcNow,
                    AdminNotes = "Zatwierdzono przez Administratora."
                };
                _context.BusinessVerifications.Add(verification);
            }
            else
            {
                business.IsVerified = false;
                business.VerificationStatus = VerificationStatus.Rejected;
                var verification = new BusinessVerification
                {
                    BusinessId = id,
                    Status = VerificationStatus.Rejected,
                    ReviewedAt = DateTime.UtcNow,
                    RejectionReason = dto.RejectionReason,
                    AdminNotes = "Odrzucono przez Administratora."
                };
                _context.BusinessVerifications.Add(verification);
            }

            await _context.SaveChangesAsync();
            return (true, dto.IsApproved ? "Firma zatwierdzona." : "Firma odrzucona.", null);
        }

        public async Task<AdminStatsDto> GetStatsAsync()
        {
            var now = DateTime.UtcNow;
            var startOfMonth = new DateTime(now.Year, now.Month, 1);

            var totalBusinesses = await _context.Businesses.CountAsync();
            var newBusinesses = await _context.Businesses.CountAsync(b => b.CreatedAt >= startOfMonth);

            var pendingVerifications = await _context.Businesses.CountAsync(b => b.VerificationStatus == VerificationStatus.Pending);

            var paidSubs = await _context.BusinessSubscriptions
                .Include(bs => bs.Plan)
                .Where(bs => bs.IsActive && bs.Plan.PriceMonthly > 0)
                .ToListAsync();

            var activeSubsCount = paidSubs.Count;
            var monthlyRevenue = paidSubs.Sum(bs => bs.Plan.PriceMonthly);

            return new AdminStatsDto
            {
                TotalBusinesses = totalBusinesses,
                NewBusinessesThisMonth = newBusinesses,
                ActiveSubscriptions = activeSubsCount,
                TotalRevenue = monthlyRevenue,
                PendingVerifications = pendingVerifications
            };
        }
    }
}
