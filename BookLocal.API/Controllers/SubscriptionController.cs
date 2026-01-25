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
    public class SubscriptionController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;

        public SubscriptionController(AppDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet("plans")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<SubscriptionPlanDto>>> GetPublicPlans()
        {
            var plans = await _context.SubscriptionPlans
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

            return Ok(dtos);
        }

        [HttpPost("subscribe")]
        [Authorize(Roles = "owner")]
        public async Task<IActionResult> Subscribe([FromBody] int planId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var business = await _context.Businesses.FirstOrDefaultAsync(b => b.OwnerId == userId);
            if (business == null) return NotFound("Nie znaleziono firmy dla tego użytkownika.");

            var plan = await _context.SubscriptionPlans.FindAsync(planId);
            if (plan == null || !plan.IsActive) return BadRequest("Nieprawidłowy plan.");

            var currentEmployeeCount = await _context.Employees.CountAsync(e => e.BusinessId == business.BusinessId);

            var currentServiceCount = await _context.Services.CountAsync(s => s.BusinessId == business.BusinessId);

            if (currentEmployeeCount > plan.MaxEmployees)
            {
                return BadRequest($"Nie można zmienić planu. Twój obecny stan pracowników ({currentEmployeeCount}) przekracza limit nowego planu ({plan.MaxEmployees}). Aby zmienić plan, zmniejsz liczbę pracowników.");
            }

            if (currentServiceCount > plan.MaxServices)
            {
                return BadRequest($"Nie można zmienić planu. Twoja obecna liczba usług ({currentServiceCount}) przekracza limit nowego planu ({plan.MaxServices}). Aby zmienić plan, usuń zbędne usługi.");
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

            return Ok(new { Message = $"Pomyślnie zasubskrybowano plan {plan.Name}." });
        }

        [HttpGet("current")]
        [Authorize(Roles = "owner")]
        public async Task<ActionResult<object>> GetCurrentSubscription()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var business = await _context.Businesses.FirstOrDefaultAsync(b => b.OwnerId == userId);
            if (business == null) return NotFound();

            var sub = await _context.BusinessSubscriptions
                .Include(bs => bs.Plan)
                .FirstOrDefaultAsync(bs => bs.BusinessId == business.BusinessId && bs.IsActive);

            if (sub == null)
            {
                return Ok(new { PlanName = "Brak", IsActive = false });
            }

            return Ok(new
            {
                PlanName = sub.Plan.Name,
                PlanId = sub.PlanId,
                StartDate = sub.StartDate,
                EndDate = sub.EndDate,
                Price = sub.Plan.PriceMonthly,
                IsActive = true,
                HasAdvancedReports = sub.Plan.HasAdvancedReports,
                HasMarketingTools = sub.Plan.HasMarketingTools
            });
        }
    }
}
