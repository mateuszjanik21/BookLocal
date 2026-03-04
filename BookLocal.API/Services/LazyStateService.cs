using BookLocal.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

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
            var now = DateTime.Now;

            if (userRole == "owner")
            {
                var business = await _context.Businesses.FirstOrDefaultAsync(b => b.OwnerId == userId);
                if (business == null) return;

                int businessId = business.BusinessId;

                var reservationsToComplete = await _context.Reservations
                    .Where(r => r.BusinessId == businessId
                             && r.Status == ReservationStatus.Confirmed
                             && r.EndTime <= now)
                    .ToListAsync();

                if (!reservationsToComplete.Any()) return;

                foreach (var tr in reservationsToComplete)
                {
                    tr.Status = ReservationStatus.Completed;

                    if (tr.CustomerId != null)
                    {
                        var existingPoints = await _context.LoyaltyPoints
                            .FirstOrDefaultAsync(lp => lp.CustomerId == tr.CustomerId && lp.BusinessId == businessId);

                        int earnedPoints = (int)Math.Floor(tr.AgreedPrice);

                        if (existingPoints != null)
                        {
                            existingPoints.PointsBalance += earnedPoints;
                            existingPoints.TotalPointsEarned += earnedPoints;
                            existingPoints.LastUpdated = now;

                            _context.LoyaltyTransactions.Add(new LoyaltyTransaction
                            {
                                LoyaltyPointId = existingPoints.LoyaltyId,
                                PointsAmount = earnedPoints,
                                Type = LoyaltyTransactionType.Earned,
                                TransactionId = tr.ReservationId,
                                Description = "Automatyczne dodanie punktów za ukończoną wizytę",
                                CreatedAt = now
                            });
                        }
                        else
                        {
                            var newLoyaltyPoint = new LoyaltyPoint
                            {
                                CustomerId = tr.CustomerId,
                                BusinessId = businessId,
                                PointsBalance = earnedPoints,
                                TotalPointsEarned = earnedPoints,
                                LastUpdated = now
                            };
                            _context.LoyaltyPoints.Add(newLoyaltyPoint);

                            await _context.SaveChangesAsync();

                            _context.LoyaltyTransactions.Add(new LoyaltyTransaction
                            {
                                LoyaltyPointId = newLoyaltyPoint.LoyaltyId,
                                PointsAmount = earnedPoints,
                                Type = LoyaltyTransactionType.Earned,
                                TransactionId = tr.ReservationId,
                                Description = "Automatyczne dodanie punktów za ukończoną wizytę (Pierwsza wizyta)",
                                CreatedAt = now
                            });
                        }
                    }
                }

                await _context.SaveChangesAsync();
            }
            else if (userRole == "customer")
            {
                var reservationsToComplete = await _context.Reservations
                    .Where(r => r.CustomerId == userId
                             && r.Status == ReservationStatus.Confirmed
                             && r.EndTime <= now)
                    .ToListAsync();

                if (!reservationsToComplete.Any()) return;

                foreach (var tr in reservationsToComplete)
                {
                    tr.Status = ReservationStatus.Completed;

                    int businessId = tr.BusinessId;
                    int earnedPoints = (int)Math.Floor(tr.AgreedPrice);

                    var existingPoints = await _context.LoyaltyPoints
                        .FirstOrDefaultAsync(lp => lp.CustomerId == userId && lp.BusinessId == businessId);

                    if (existingPoints != null)
                    {
                        existingPoints.PointsBalance += earnedPoints;
                        existingPoints.TotalPointsEarned += earnedPoints;
                        existingPoints.LastUpdated = now;

                        _context.LoyaltyTransactions.Add(new LoyaltyTransaction
                        {
                            LoyaltyPointId = existingPoints.LoyaltyId,
                            PointsAmount = earnedPoints,
                            Type = LoyaltyTransactionType.Earned,
                            TransactionId = tr.ReservationId,
                            Description = "Automatyczne dodanie punktów za ukończoną wizytę",
                            CreatedAt = now
                        });
                    }
                    else
                    {
                        var newLoyaltyPoint = new LoyaltyPoint
                        {
                            CustomerId = userId,
                            BusinessId = businessId,
                            PointsBalance = earnedPoints,
                            TotalPointsEarned = earnedPoints,
                            LastUpdated = now
                        };
                        _context.LoyaltyPoints.Add(newLoyaltyPoint);

                        await _context.SaveChangesAsync();

                        _context.LoyaltyTransactions.Add(new LoyaltyTransaction
                        {
                            LoyaltyPointId = newLoyaltyPoint.LoyaltyId,
                            PointsAmount = earnedPoints,
                            Type = LoyaltyTransactionType.Earned,
                            TransactionId = tr.ReservationId,
                            Description = "Automatyczne dodanie punktów za ukończoną wizytę (Pierwsza wizyta)",
                            CreatedAt = now
                        });
                    }
                }

                await _context.SaveChangesAsync();
            }
        }
    }
}
