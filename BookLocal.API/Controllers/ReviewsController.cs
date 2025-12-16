using BookLocal.API.DTOs;
using BookLocal.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

[ApiController]
[Route("api/businesses/{businessId}/reviews")]
public class ReviewsController : ControllerBase
{
    private readonly AppDbContext _context;

    public ReviewsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<PagedResultDto<ReviewDto>>> GetReviews(int businessId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 5)
    {
        if (!await _context.Businesses.AnyAsync(b => b.BusinessId == businessId))
            return NotFound("Firma nie istnieje.");

        var query = _context.Reviews
            .Where(r => r.BusinessId == businessId)
            .OrderByDescending(r => r.CreatedAt)
            .AsQueryable();

        var totalCount = await query.CountAsync();

        var reviewsData = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Include(r => r.User)
            .Include(r => r.Reservation)
                .ThenInclude(res => res.Service)
            .Include(r => r.Reservation)
                .ThenInclude(res => res.Employee)
            .Select(r => new ReviewDto
            {
                ReviewId = r.ReviewId,
                Rating = r.Rating,
                Comment = r.Comment,
                ReviewerName = r.ReviewerName,
                CreatedAt = r.CreatedAt,
                UserId = r.UserId,
                ReviewerPhotoUrl = r.User.PhotoUrl,
                ServiceName = r.Reservation != null ? r.Reservation.Service.Name : null,
                EmployeeFullName = r.Reservation != null ? $"{r.Reservation.Employee.FirstName} {r.Reservation.Employee.LastName}" : null
            })
            .ToListAsync();

        var paginatedResult = new PagedResultDto<ReviewDto>
        {
            Items = reviewsData,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };

        return Ok(paginatedResult);
    }

    [HttpPut("{reviewId}")]
    [Authorize(Roles = "customer")]
    public async Task<IActionResult> UpdateReview(int businessId, int reviewId, UpdateReviewDto updateReviewDto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var review = await _context.Reviews
            .FirstOrDefaultAsync(r => r.ReviewId == reviewId && r.BusinessId == businessId);

        if (review == null) return NotFound();
        if (review.UserId != userId) return Forbid();

        review.Rating = updateReviewDto.Rating;
        review.Comment = updateReviewDto.Comment;
        review.CreatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{reviewId}")]
    [Authorize(Roles = "customer")]
    public async Task<IActionResult> DeleteReview(int businessId, int reviewId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var review = await _context.Reviews
            .FirstOrDefaultAsync(r => r.ReviewId == reviewId && r.BusinessId == businessId);

        if (review == null) return NotFound();
        if (review.UserId != userId) return Forbid();

        _context.Reviews.Remove(review);
        await _context.SaveChangesAsync();
        return NoContent();
    }


    [HttpPost("/api/reservations/{reservationId}/reviews")]
    [Authorize(Roles = "customer")]
    public async Task<ActionResult<ReviewDto>> PostReviewForReservation(int reservationId, CreateReviewDto createReviewDto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        var reservation = await _context.Reservations
            .Include(r => r.Service) 
            .FirstOrDefaultAsync(r => r.ReservationId == reservationId);

        if (reservation == null) return NotFound("Rezerwacja nie istnieje.");
        if (reservation.CustomerId != userId) return Forbid("To nie jest Twoja rezerwacja.");
        var endTime = reservation.StartTime.AddMinutes(reservation.Service.DurationMinutes);

        bool isEffectivelyCompleted = reservation.Status == ReservationStatus.Completed ||
                                      (reservation.Status == ReservationStatus.Confirmed && endTime < DateTime.UtcNow);

        if (!isEffectivelyCompleted)
        {
            return BadRequest("Możesz ocenić tylko zakończone wizyty.");
        }

        var existingReview = await _context.Reviews.AnyAsync(r => r.ReservationId == reservationId);
        if (existingReview) return Conflict("Ta wizyta została już oceniona.");

        var user = await _context.Users.FindAsync(userId);
        if (user == null) return Unauthorized();

        var newReview = new Review
        {
            BusinessId = reservation.Service.BusinessId,
            ReservationId = reservationId,
            Rating = createReviewDto.Rating,
            Comment = createReviewDto.Comment,
            UserId = userId,
            ReviewerName = $"{user.FirstName} {user.LastName}"
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
            ReviewerPhotoUrl = user.PhotoUrl
        };

        return CreatedAtAction(nameof(GetReviews), new { businessId = newReview.BusinessId }, reviewToReturn);
    }

    [HttpGet("can-review")]
    [Authorize(Roles = "customer")]
    public async Task<ActionResult<object>> CanUserReview(int businessId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        var hasAlreadyReviewed = await _context.Reviews
            .AnyAsync(r => r.BusinessId == businessId && r.UserId == userId);

        return Ok(new { canReview = !hasAlreadyReviewed });
    }
}