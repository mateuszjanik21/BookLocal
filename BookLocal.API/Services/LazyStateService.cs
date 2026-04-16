using BookLocal.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace BookLocal.API.Services
{
    public class LazyStateService : ILazyStateService
    {
        private readonly AppDbContext _context;

        public LazyStateService(AppDbContext context)
        {
            _context = context;
        }

        public async Task SyncUserStateAsync(string userId, string userRole)
        {
            var now = DateTime.UtcNow;

            List<Reservation> reservationsToComplete = new List<Reservation>();

            if (userRole == "owner")
            {
                var business = await _context.Businesses.AsNoTracking().FirstOrDefaultAsync(b => b.OwnerId == userId);
                if (business == null) return;

                reservationsToComplete = await _context.Reservations
                    .Where(r => r.BusinessId == business.BusinessId
                             && r.Status == ReservationStatus.Confirmed
                             && r.EndTime <= now)
                    .ToListAsync();
            }
            else if (userRole == "customer")
            {
                reservationsToComplete = await _context.Reservations
                    .Where(r => r.CustomerId == userId
                             && r.Status == ReservationStatus.Confirmed
                             && r.EndTime <= now)
                    .ToListAsync();
            }

            if (!reservationsToComplete.Any()) return;

            using var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
            try
            {
                var businessIds = reservationsToComplete.Select(r => r.BusinessId).Distinct().ToList();
                var customerIds = reservationsToComplete.Where(r => r.CustomerId != null).Select(r => r.CustomerId).Distinct().ToList();

                var configs = await _context.LoyaltyProgramConfigs
                    .Where(c => businessIds.Contains(c.BusinessId))
                    .ToDictionaryAsync(c => c.BusinessId);

                var existingPoints = await _context.LoyaltyPoints
                    .Where(lp => businessIds.Contains(lp.BusinessId) && customerIds.Contains(lp.CustomerId))
                    .ToListAsync();

                var reservationIds = reservationsToComplete.Select(r => r.ReservationId).ToList();
                var processedTransactions = await _context.LoyaltyTransactions
                    .Where(lt => lt.ReservationId != null && reservationIds.Contains(lt.ReservationId.Value))
                    .Select(lt => lt.ReservationId.Value)
                    .ToListAsync();

                foreach (var tr in reservationsToComplete)
                {
                    tr.Status = ReservationStatus.Completed;

                    if (tr.CustomerId != null && !processedTransactions.Contains(tr.ReservationId))
                    {
                        if (configs.TryGetValue(tr.BusinessId, out var config) && config.IsActive && config.SpendAmountForOnePoint > 0)
                        {
                            int earnedPoints = (int)Math.Floor(tr.AgreedPrice / config.SpendAmountForOnePoint);
                            if (earnedPoints > 0)
                            {
                                var loyaltyPoint = existingPoints.FirstOrDefault(p => p.BusinessId == tr.BusinessId && p.CustomerId == tr.CustomerId);

                                if (loyaltyPoint == null)
                                {
                                    loyaltyPoint = new LoyaltyPoint
                                    {
                                        BusinessId = tr.BusinessId,
                                        CustomerId = tr.CustomerId,
                                        PointsBalance = 0,
                                        TotalPointsEarned = 0,
                                        LastUpdated = now
                                    };
                                    _context.LoyaltyPoints.Add(loyaltyPoint);
                                    existingPoints.Add(loyaltyPoint);
                                }

                                loyaltyPoint.PointsBalance += earnedPoints;
                                loyaltyPoint.TotalPointsEarned += earnedPoints;
                                loyaltyPoint.LastUpdated = now;

                                _context.LoyaltyTransactions.Add(new LoyaltyTransaction
                                {
                                    LoyaltyPoint = loyaltyPoint,
                                    PointsAmount = earnedPoints,
                                    Type = LoyaltyTransactionType.Earned,
                                    ReservationId = tr.ReservationId,
                                    Description = "Automatyczne dodanie punktów za ukończoną wizytę",
                                    CreatedAt = now
                                });
                            }
                        }
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
