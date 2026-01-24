using BookLocal.API.DTOs;
using BookLocal.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookLocal.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "superadmin")]
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;

        public AdminController(AppDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // --- SUBSCRIPTION PLANS ---

        [HttpGet("plans")]
        public async Task<ActionResult<IEnumerable<SubscriptionPlanDto>>> GetPlans()
        {
            var plans = await _context.SubscriptionPlans
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

            return Ok(dtos);
        }

        [HttpPost("plans")]
        public async Task<ActionResult<SubscriptionPlanDto>> CreatePlan(CreateSubscriptionPlanDto dto)
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

            return CreatedAtAction(nameof(GetPlans), new { }, new SubscriptionPlanDto
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
            });
        }

        [HttpPut("plans/{id}")]
        public async Task<IActionResult> UpdatePlan(int id, CreateSubscriptionPlanDto dto)
        {
            var plan = await _context.SubscriptionPlans.FindAsync(id);
            if (plan == null) return NotFound("Nie znaleziono planu.");

            plan.Name = dto.Name;
            plan.PriceMonthly = dto.PriceMonthly;
            plan.PriceYearly = dto.PriceYearly;
            plan.MaxEmployees = dto.MaxEmployees;
            plan.MaxServices = dto.MaxServices;
            plan.HasAdvancedReports = dto.HasAdvancedReports;
            plan.HasMarketingTools = dto.HasMarketingTools;
            plan.CommissionPercentage = dto.CommissionPercentage;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("plans/{id}")]
        public async Task<IActionResult> DeletePlan(int id)
        {
            var plan = await _context.SubscriptionPlans.FindAsync(id);
            if (plan == null) return NotFound("Nie znaleziono planu.");

            // Soft delete
            plan.IsActive = false;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // --- BUSINESS VERIFICATION ---

        [HttpGet("businesses")]
        public async Task<ActionResult<IEnumerable<AdminBusinessListDto>>> GetBusinesses([FromQuery] string? status = null)
        {
            var query = _context.Businesses
                .Include(b => b.Owner)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status) && Enum.TryParse<VerificationStatus>(status, true, out var statusEnum))
            {
                // Note: VerificationStatus is property in Business now, not joined table logic for MVP
                // But wait, we added VerificationStatus to Business model directly?
                // Yes, in Step 2742 we added public VerificationStatus VerificationStatus { get; set; }
                query = query.Where(b => b.VerificationStatus == statusEnum);
            }

            var businesses = await query.ToListAsync();

            // We might want to join with active subscription to show Plan Name
            // For now, let's just fetch it simply or do a manual join/lookup if generic
            // Let's rely on BusinessSubscription table

            var businessIds = businesses.Select(b => b.BusinessId).ToList();
            var activeSubs = await _context.BusinessSubscriptions
                .Include(bs => bs.Plan)
                .Where(bs => businessIds.Contains(bs.BusinessId) && bs.IsActive)
                .ToListAsync();

            var dtos = businesses.Select(b =>
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

            return Ok(dtos);
        }

        [HttpPatch("businesses/{id}/verify")]
        public async Task<IActionResult> VerifyBusiness(int id, [FromBody] VerifyBusinessDto dto)
        {
            var business = await _context.Businesses.FindAsync(id);
            if (business == null) return NotFound("Nie znaleziono firmy.");

            if (dto.IsApproved)
            {
                business.IsVerified = true;
                business.VerificationStatus = VerificationStatus.Approved;
                // Create Verification Record History
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
            return Ok(new { Message = dto.IsApproved ? "Firma zatwierdzona." : "Firma odrzucona." });
        }

        [HttpGet("stats")]
        public async Task<ActionResult<AdminStatsDto>> GetStats()
        {
            var now = DateTime.UtcNow;
            var startOfMonth = new DateTime(now.Year, now.Month, 1);

            var totalBusinesses = await _context.Businesses.CountAsync();
            var newBusinesses = await _context.Businesses.CountAsync(b => b.CreatedAt >= startOfMonth);

            var pendingVerifications = await _context.Businesses.CountAsync(b => b.VerificationStatus == VerificationStatus.Pending);

            // Active Subscriptions that are NOT Free (Price > 0)
            // Need to join Plans to check Price
            var paidSubs = await _context.BusinessSubscriptions
                .Include(bs => bs.Plan)
                .Where(bs => bs.IsActive && bs.Plan.PriceMonthly > 0)
                .ToListAsync();

            var activeSubsCount = paidSubs.Count;

            // Allow simulation of revenue: Sum of monthly prices of active paid subs
            // Ensure Plan is not null (Include used above)
            var monthlyRevenue = paidSubs.Sum(bs => bs.Plan.PriceMonthly);

            return Ok(new AdminStatsDto
            {
                TotalBusinesses = totalBusinesses,
                NewBusinessesThisMonth = newBusinesses,
                ActiveSubscriptions = activeSubsCount,
                TotalRevenue = monthlyRevenue,
                PendingVerifications = pendingVerifications
            });
        }
    }
}
