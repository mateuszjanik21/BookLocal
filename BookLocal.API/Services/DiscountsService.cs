using BookLocal.API.DTOs;
using BookLocal.API.Interfaces;
using BookLocal.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BookLocal.API.Services
{
    public class DiscountsService : IDiscountsService
    {
        private readonly AppDbContext _context;

        public DiscountsService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<(bool Success, IEnumerable<DiscountDto>? Data)> GetDiscountsAsync(int businessId, ClaimsPrincipal user)
        {
            var ownerId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            var businessExists = await _context.Businesses.AnyAsync(b => b.BusinessId == businessId && b.OwnerId == ownerId);
            if (!businessExists) return (false, null);

            var discounts = await _context.Discounts
                .AsNoTracking()
                .Where(d => d.BusinessId == businessId)
                .OrderByDescending(d => d.DiscountId)
                .Select(d => new DiscountDto
                {
                    DiscountId = d.DiscountId,
                    Code = d.Code,
                    Type = d.Type.ToString(),
                    Value = d.Value,
                    MaxUses = d.MaxUses,
                    MaxUsesPerCustomer = d.MaxUsesPerCustomer,
                    UsedCount = d.UsedCount,
                    ValidFrom = d.ValidFrom,
                    ValidTo = d.ValidTo,
                    IsActive = d.IsActive,
                    ServiceId = d.ServiceId
                })
                .ToListAsync();

            return (true, discounts);
        }

        public async Task<(bool Success, DiscountDto? Data, string? ErrorMessage)> CreateDiscountAsync(int businessId, CreateDiscountDto dto, ClaimsPrincipal user)
        {
            var ownerId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            var businessExists = await _context.Businesses.AnyAsync(b => b.BusinessId == businessId && b.OwnerId == ownerId);
            if (!businessExists) return (false, null, "Nie masz dostępu lub firma nie istnieje");

            if (await _context.Discounts.AnyAsync(d => d.BusinessId == businessId && d.Code == dto.Code && d.IsActive))
            {
                return (true, null, "Kod rabatowy o tej nazwie już istnieje.");
            }

            var discount = new Discount
            {
                BusinessId = businessId,
                Code = dto.Code.ToUpper(),
                Type = dto.Type,
                Value = dto.Value,
                MaxUses = dto.MaxUses,
                MaxUsesPerCustomer = dto.MaxUsesPerCustomer,
                ValidFrom = dto.ValidFrom,
                ValidTo = dto.ValidTo,
                ServiceId = dto.ServiceId,
                IsActive = true
            };

            _context.Discounts.Add(discount);
            await _context.SaveChangesAsync();

            return (true, new DiscountDto
            {
                DiscountId = discount.DiscountId,
                Code = discount.Code,
                Type = discount.Type.ToString(),
                Value = discount.Value,
                MaxUses = discount.MaxUses,
                MaxUsesPerCustomer = discount.MaxUsesPerCustomer,
                UsedCount = 0,
                ValidFrom = discount.ValidFrom,
                ValidTo = discount.ValidTo,
                ServiceId = discount.ServiceId,
                IsActive = true
            }, null);
        }

        public async Task<(bool Success, bool? IsActive)> ToggleDiscountAsync(int businessId, int discountId, ClaimsPrincipal user)
        {
            var ownerId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            var businessExists = await _context.Businesses.AnyAsync(b => b.BusinessId == businessId && b.OwnerId == ownerId);
            if (!businessExists) return (false, null);

            var discount = await _context.Discounts.FirstOrDefaultAsync(d => d.DiscountId == discountId && d.BusinessId == businessId);
            if (discount == null) return (false, null);

            discount.IsActive = !discount.IsActive;
            await _context.SaveChangesAsync();

            return (true, discount.IsActive);
        }

        public async Task<(bool Success, VerifyDiscountResult? Data)> VerifyDiscountAsync(int businessId, VerifyDiscountRequest request)
        {
            var code = request.Code?.Trim().ToUpper();
            if (string.IsNullOrEmpty(code)) return (true, new VerifyDiscountResult { IsValid = false, Message = "Kod jest wymagany." });

            var discount = await _context.Discounts
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.BusinessId == businessId && d.Code == code && d.IsActive);

            if (discount == null)
            {
                return (true, new VerifyDiscountResult { IsValid = false, Message = "Kod nieprawidłowy lub nieaktywny." });
            }

            if (discount.MaxUses.HasValue && discount.UsedCount >= discount.MaxUses.Value)
            {
                return (true, new VerifyDiscountResult { IsValid = false, Message = "Limit użycia kodu został wyczerpany." });
            }

            if (discount.MaxUsesPerCustomer.HasValue && !string.IsNullOrEmpty(request.CustomerId))
            {
                var userUses = await _context.Reservations
                    .CountAsync(r => r.CustomerId == request.CustomerId &&
                                     r.DiscountId == discount.DiscountId &&
                                     r.Status != ReservationStatus.Cancelled &&
                                     r.Status != ReservationStatus.NoShow);

                if (userUses >= discount.MaxUsesPerCustomer.Value)
                {
                    return (true, new VerifyDiscountResult { IsValid = false, Message = "Osiągnięto limit użyć tego kodu dla Twojego konta." });
                }
            }

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            if (discount.ValidFrom.HasValue && today < discount.ValidFrom.Value)
            {
                return (true, new VerifyDiscountResult { IsValid = false, Message = "Kod jeszcze nie jest aktywny." });
            }
            if (discount.ValidTo.HasValue && today > discount.ValidTo.Value)
            {
                return (true, new VerifyDiscountResult { IsValid = false, Message = "Kod wygasł." });
            }

            if (discount.ServiceId.HasValue && request.ServiceId.HasValue && discount.ServiceId != request.ServiceId)
            {
                return (true, new VerifyDiscountResult { IsValid = false, Message = "Kod nie dotyczy tej usługi." });
            }

            decimal discountAmount = 0;
            if (discount.Type == DiscountType.Percentage)
            {
                discountAmount = request.OriginalPrice * (discount.Value / 100m);
            }
            else
            {
                discountAmount = discount.Value;
            }

            if (discountAmount > request.OriginalPrice) discountAmount = request.OriginalPrice;

            return (true, new VerifyDiscountResult
            {
                IsValid = true,
                DiscountId = discount.DiscountId,
                DiscountAmount = discountAmount,
                FinalPrice = request.OriginalPrice - discountAmount,
                Message = "Kod poprawny."
            });
        }
    }
}
