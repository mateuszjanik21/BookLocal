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
    public async Task<ActionResult<IEnumerable<ReviewDto>>> GetReviews(int businessId)
    {
        if (!await _context.Businesses.AnyAsync(b => b.BusinessId == businessId))
            return NotFound("Firma nie istnieje.");

        var reviews = await _context.Reviews
            .Include(r => r.User)
            .Where(r => r.BusinessId == businessId)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new ReviewDto
            {
                ReviewId = r.ReviewId,
                Rating = r.Rating,
                Comment = r.Comment,
                ReviewerName = r.ReviewerName,
                CreatedAt = r.CreatedAt,
                UserId = r.UserId,
                ReviewerPhotoUrl = r.User.PhotoUrl
            })
            .ToListAsync();

        return Ok(reviews);
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


    [HttpPost]
    [Authorize(Roles = "customer")]
    public async Task<ActionResult<ReviewDto>> PostReview(int businessId, CreateReviewDto createReviewDto)
    {
        if (!await _context.Businesses.AnyAsync(b => b.BusinessId == businessId))
            return NotFound("Firma nie istnieje.");

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        var existingReview = await _context.Reviews
            .AnyAsync(r => r.BusinessId == businessId && r.UserId == userId);

        if (existingReview)
        {
            return Conflict("You have already submitted a review for this business.");
        }

        var user = await _context.Users.FindAsync(userId);
        if (user == null) return Unauthorized();

        var newReview = new Review
        {
            BusinessId = businessId,
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

        return CreatedAtAction(nameof(GetReviews), new { businessId = businessId }, reviewToReturn);
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