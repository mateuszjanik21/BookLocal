using BookLocal.API.DTOs;
using BookLocal.API.Interfaces;
using BookLocal.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BookLocal.API.Services
{
    public class ReviewsService : IReviewsService
    {
        private readonly AppDbContext _context;

        public ReviewsService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<(bool Success, PagedResultDto<ReviewDto>? Data, string? ErrorMessage, int StatusCode)> GetReviewsAsync(int businessId, int pageNumber, int pageSize, int? rating, string? search, string? sortBy, ClaimsPrincipal user)
        {
            if (!await _context.Businesses.AnyAsync(b => b.BusinessId == businessId))
                return (false, null, "Firma nie istnieje.", 404);

            var query = _context.Reviews
                .AsNoTracking()
                .Where(r => r.BusinessId == businessId)
                .AsQueryable();

            if (rating.HasValue && rating.Value > 0)
            {
                query = query.Where(r => r.Rating == rating.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var lowerSearch = search.ToLower();
                query = query.Where(r =>
                    r.ReviewerName.ToLower().Contains(lowerSearch) ||
                    (r.Comment != null && r.Comment.ToLower().Contains(lowerSearch)) ||
                    (r.Reservation != null && r.Reservation.Employee.FirstName.ToLower().Contains(lowerSearch)) ||
                    (r.Reservation != null && r.Reservation.Employee.LastName.ToLower().Contains(lowerSearch)) ||
                    (r.Reservation != null && r.Reservation.ServiceVariant != null && r.Reservation.ServiceVariant.Service.Name.ToLower().Contains(lowerSearch))
                );
            }

            var currentUserId = user.FindFirstValue(ClaimTypes.NameIdentifier);

            if (currentUserId != null)
            {
                query = sortBy switch
                {
                    "highest" => query.OrderByDescending(r => r.UserId == currentUserId).ThenByDescending(r => r.Rating).ThenByDescending(r => r.CreatedAt),
                    "lowest" => query.OrderByDescending(r => r.UserId == currentUserId).ThenBy(r => r.Rating).ThenByDescending(r => r.CreatedAt),
                    _ => query.OrderByDescending(r => r.UserId == currentUserId).ThenByDescending(r => r.CreatedAt)
                };
            }
            else
            {
                query = sortBy switch
                {
                    "highest" => query.OrderByDescending(r => r.Rating).ThenByDescending(r => r.CreatedAt),
                    "lowest" => query.OrderBy(r => r.Rating).ThenByDescending(r => r.CreatedAt),
                    _ => query.OrderByDescending(r => r.CreatedAt)
                };
            }

            var totalCount = await query.CountAsync();

            var reviewsData = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Include(r => r.User)
                .Include(r => r.Reservation)
                    .ThenInclude(res => res.ServiceVariant)
                        .ThenInclude(v => v.Service)
                .Include(r => r.Reservation)
                    .ThenInclude(res => res.Employee)
                .Select(r => new ReviewDto
                {
                    ReviewId = r.ReviewId,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    ReviewerName = r.User != null ? $"{r.User.FirstName} {r.User.LastName}" : r.ReviewerName,
                    CreatedAt = r.CreatedAt,
                    UserId = r.UserId,
                    ReviewerPhotoUrl = r.User != null ? r.User.PhotoUrl : null,
                    ServiceName = r.Reservation != null && r.Reservation.ServiceVariant != null
                        ? $"{r.Reservation.ServiceVariant.Service.Name} ({r.Reservation.ServiceVariant.Name})"
                        : "Usługa nieznana",
                    EmployeeFullName = r.Reservation != null && r.Reservation.Employee != null
                        ? $"{r.Reservation.Employee.FirstName} {r.Reservation.Employee.LastName}"
                        : null,
                    ReservationDate = r.Reservation != null ? r.Reservation.StartTime : (DateTime?)null
                })
                .ToListAsync();

            var paginatedResult = new PagedResultDto<ReviewDto>
            {
                Items = reviewsData,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount
            };

            return (true, paginatedResult, null, 200);
        }

        public async Task<(bool Success, string? ErrorMessage, int StatusCode)> UpdateReviewAsync(int businessId, int reviewId, UpdateReviewDto dto, ClaimsPrincipal user)
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            var review = await _context.Reviews
                .FirstOrDefaultAsync(r => r.ReviewId == reviewId && r.BusinessId == businessId);

            if (review == null) return (false, "Nie znaleziono recenzji.", 404);
            if (review.UserId != userId) return (false, "Brak uprawnień.", 403);

            review.Rating = dto.Rating;
            review.Comment = dto.Comment;
            review.CreatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return (true, null, 204);
        }

        public async Task<(bool Success, string? ErrorMessage, int StatusCode)> DeleteReviewAsync(int businessId, int reviewId, ClaimsPrincipal user)
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            var review = await _context.Reviews
                .FirstOrDefaultAsync(r => r.ReviewId == reviewId && r.BusinessId == businessId);

            if (review == null) return (false, "Nie znaleziono recenzji.", 404);
            if (review.UserId != userId) return (false, "Brak uprawnień.", 403);

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();
            return (true, null, 204);
        }

        public async Task<(bool Success, ReviewDto? Data, int BusinessId, string? ErrorMessage, int StatusCode)> PostReviewForReservationAsync(int reservationId, CreateReviewDto dto, ClaimsPrincipal user)
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return (false, null, 0, "Unauthorized", 401);

            var reservation = await _context.Reservations
                .Include(r => r.ServiceVariant)
                    .ThenInclude(v => v.Service)
                .FirstOrDefaultAsync(r => r.ReservationId == reservationId);

            if (reservation == null) return (false, null, 0, "Rezerwacja nie istnieje.", 404);
            if (reservation.CustomerId != userId) return (false, null, 0, "To nie jest Twoja rezerwacja.", 403);

            var endTime = reservation.EndTime;
            bool isEffectivelyCompleted = reservation.Status == ReservationStatus.Completed ||
                                          (reservation.Status == ReservationStatus.Confirmed && endTime < DateTime.UtcNow);

            if (!isEffectivelyCompleted)
            {
                return (false, null, 0, "Możesz ocenić tylko zakończone wizyty.", 400);
            }

            var existingReview = await _context.Reviews.AnyAsync(r => r.ReservationId == reservationId);
            if (existingReview) return (false, null, 0, "Ta wizyta została już oceniona.", 409);

            var dbUser = await _context.Users.FindAsync(userId);
            if (dbUser == null) return (false, null, 0, "Unauthorized", 401);

            var newReview = new Review
            {
                BusinessId = reservation.BusinessId,
                ReservationId = reservationId,
                Rating = dto.Rating,
                Comment = dto.Comment,
                UserId = userId,
                ReviewerName = $"{dbUser.FirstName} {dbUser.LastName}"
            };

            _context.Reviews.Add(newReview);
            await _context.SaveChangesAsync();

            var reviewToReturn = new ReviewDto
            {
                ReviewId = newReview.ReviewId,
                Rating = newReview.Rating,
                Comment = newReview.Comment,
                ReviewerName = newReview.ReviewerName,
                CreatedAt = newReview.CreatedAt,
                ReviewerPhotoUrl = dbUser.PhotoUrl,
                ServiceName = reservation.ServiceVariant != null
                    ? $"{reservation.ServiceVariant.Service.Name} ({reservation.ServiceVariant.Name})"
                    : "Usługa",
            };

            return (true, reviewToReturn, newReview.BusinessId, null, 201);
        }

        public async Task<(bool Success, object? Data, string? ErrorMessage)> CanUserReviewAsync(int businessId, ClaimsPrincipal user)
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return (false, null, "Unauthorized");

            var hasAlreadyReviewed = await _context.Reviews
                .AnyAsync(r => r.BusinessId == businessId && r.UserId == userId);

            return (true, new { canReview = !hasAlreadyReviewed }, null);
        }
    }
}
