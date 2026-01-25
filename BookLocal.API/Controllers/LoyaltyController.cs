using BookLocal.API.DTOs;
using BookLocal.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookLocal.API.Controllers
{
    [Route("api/businesses/{businessId}/loyalty")]
    [ApiController]
    [Authorize]
    public class LoyaltyController : ControllerBase
    {
        private readonly AppDbContext _context;

        public LoyaltyController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("config")]
        public async Task<ActionResult<LoyaltyConfigDto>> GetConfig(int businessId)
        {
            var config = await _context.LoyaltyProgramConfigs
                .FirstOrDefaultAsync(c => c.BusinessId == businessId);

            if (config == null)
            {
                return Ok(new LoyaltyConfigDto { IsActive = false, SpendAmountForOnePoint = 10.00m });
            }

            return Ok(new LoyaltyConfigDto
            {
                IsActive = config.IsActive,
                SpendAmountForOnePoint = config.SpendAmountForOnePoint
            });
        }

        [HttpPut("config")]
        [Authorize(Roles = "owner")]
        public async Task<IActionResult> UpdateConfig(int businessId, [FromBody] LoyaltyConfigDto dto)
        {
            var config = await _context.LoyaltyProgramConfigs
                .FirstOrDefaultAsync(c => c.BusinessId == businessId);

            if (config == null)
            {
                config = new LoyaltyProgramConfig { BusinessId = businessId };
                _context.LoyaltyProgramConfigs.Add(config);
            }

            config.IsActive = dto.IsActive;
            config.SpendAmountForOnePoint = dto.SpendAmountForOnePoint;

            await _context.SaveChangesAsync();
            return Ok(config);
        }

        [HttpGet("customer/{customerId}")]
        public async Task<ActionResult<object>> GetCustomerLoyalty(int businessId, string customerId)
        {
            var points = await _context.LoyaltyPoints
                .FirstOrDefaultAsync(p => p.BusinessId == businessId && p.CustomerId == customerId);

            var balanceDto = new LoyaltyBalanceDto
            {
                PointsBalance = points?.PointsBalance ?? 0,
                TotalPointsEarned = points?.TotalPointsEarned ?? 0
            };

            var transactions = await _context.LoyaltyTransactions
                .Where(t => t.LoyaltyPoint != null && t.LoyaltyPoint.BusinessId == businessId && t.LoyaltyPoint.CustomerId == customerId)
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => new LoyaltyTransactionDto
                {
                    TransactionId = t.TransactionId,
                    PointsAmount = t.PointsAmount,
                    Type = t.Type.ToString(),
                    Description = t.Description,
                    CreatedAt = t.CreatedAt
                })
                .ToListAsync();

            return Ok(new { Balance = balanceDto, Transactions = transactions });
        }
        [HttpPost("recalculate")]
        [Authorize(Roles = "owner")]
        public async Task<IActionResult> RecalculatePoints(int businessId)
        {
            var config = await _context.LoyaltyProgramConfigs
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.BusinessId == businessId);

            if (config == null || !config.IsActive || config.SpendAmountForOnePoint <= 0)
            {
                return BadRequest("Program lojalnościowy nie jest aktywny lub nie został skonfigurowany.");
            }

            var completedReservations = await _context.Reservations
                .Where(r => r.BusinessId == businessId && r.Status == ReservationStatus.Completed && r.CustomerId != null)
                .ToListAsync();

            var existingTransactions = await _context.LoyaltyTransactions
                .Where(t => t.LoyaltyPoint.BusinessId == businessId && t.ReservationId != null)
                .Select(t => t.ReservationId.Value)
                .ToListAsync();

            var existingTransactionsSet = new HashSet<int>(existingTransactions);
            int addedPointsCount = 0;
            int processedReservations = 0;

            foreach (var reservation in completedReservations)
            {
                if (existingTransactionsSet.Contains(reservation.ReservationId)) continue;
                if (reservation.CustomerId == null) continue;

                var pointsToEarn = (int)(reservation.AgreedPrice / config.SpendAmountForOnePoint);
                if (pointsToEarn <= 0) continue;

                var loyaltyPoint = await _context.LoyaltyPoints
                    .FirstOrDefaultAsync(p => p.BusinessId == businessId && p.CustomerId == reservation.CustomerId);

                if (loyaltyPoint == null)
                {
                    loyaltyPoint = new LoyaltyPoint
                    {
                        BusinessId = businessId,
                        CustomerId = reservation.CustomerId,
                        PointsBalance = 0,
                        TotalPointsEarned = 0
                    };
                    _context.LoyaltyPoints.Add(loyaltyPoint);
                }

                loyaltyPoint.PointsBalance += pointsToEarn;
                loyaltyPoint.TotalPointsEarned += pointsToEarn;
                loyaltyPoint.LastUpdated = DateTime.UtcNow;

                var transaction = new LoyaltyTransaction
                {
                    LoyaltyPoint = loyaltyPoint,
                    PointsAmount = pointsToEarn,
                    Type = LoyaltyTransactionType.Earned,
                    Description = $"Wizyta (ID: {reservation.ReservationId}) - Przeliczenie",
                    ReservationId = reservation.ReservationId,
                    CreatedAt = DateTime.UtcNow
                };

                _context.LoyaltyTransactions.Add(transaction);
                processedReservations++;
                addedPointsCount += pointsToEarn;
            }

            await _context.SaveChangesAsync();

            return Ok(new { Message = $"Przeliczono punkty. Dodano {addedPointsCount} punktów dla {processedReservations} wizyt." });
        }
    }
}
