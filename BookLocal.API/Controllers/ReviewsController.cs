using BookLocal.API.DTOs;
using BookLocal.API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookLocal.API.Controllers
{
    [ApiController]
    [Route("api/businesses/{businessId}/reviews")]
    public class ReviewsController : ControllerBase
    {
        private readonly IReviewsService _reviewsService;

        public ReviewsController(IReviewsService reviewsService)
        {
            _reviewsService = reviewsService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<PagedResultDto<ReviewDto>>> GetReviews(int businessId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 5, [FromQuery] int? rating = null, [FromQuery] string? search = null, [FromQuery] string? sortBy = "newest")
        {
            var result = await _reviewsService.GetReviewsAsync(businessId, pageNumber, pageSize, rating, search, sortBy, User);

            if (!result.Success) return NotFound(result.ErrorMessage);

            return Ok(result.Data);
        }

        [HttpPut("{reviewId}")]
        [Authorize(Roles = "customer")]
        public async Task<IActionResult> UpdateReview(int businessId, int reviewId, UpdateReviewDto updateReviewDto)
        {
            var result = await _reviewsService.UpdateReviewAsync(businessId, reviewId, updateReviewDto, User);

            if (!result.Success)
            {
                if (result.ErrorMessage == "Brak uprawnień.") return Forbid();
                return NotFound(result.ErrorMessage);
            }

            return NoContent();
        }

        [HttpDelete("{reviewId}")]
        [Authorize(Roles = "customer")]
        public async Task<IActionResult> DeleteReview(int businessId, int reviewId)
        {
            var result = await _reviewsService.DeleteReviewAsync(businessId, reviewId, User);

            if (!result.Success)
            {
                if (result.ErrorMessage == "Brak uprawnień.") return Forbid();
                return NotFound(result.ErrorMessage);
            }

            return NoContent();
        }

        [HttpPost("/api/reservations/{reservationId}/reviews")]
        [Authorize(Roles = "customer")]
        public async Task<ActionResult<ReviewDto>> PostReviewForReservation(int reservationId, CreateReviewDto createReviewDto)
        {
            var result = await _reviewsService.PostReviewForReservationAsync(reservationId, createReviewDto, User);

            if (!result.Success)
            {
                if (result.ErrorMessage == "Brak uprawnień." || result.ErrorMessage == "To nie jest Twoja rezerwacja.") return Forbid();
                if (result.ErrorMessage == "Unauthorized") return Unauthorized();
                if (result.ErrorMessage!.Contains("już oceniona")) return Conflict(result.ErrorMessage);
                if (result.ErrorMessage.Contains("nie istnieje")) return NotFound(result.ErrorMessage);
                return BadRequest(result.ErrorMessage);
            }

            return CreatedAtAction(nameof(GetReviews), new { businessId = result.BusinessId }, result.Data);
        }

        [HttpGet("can-review")]
        [Authorize(Roles = "customer")]
        public async Task<ActionResult<object>> CanUserReview(int businessId)
        {
            var result = await _reviewsService.CanUserReviewAsync(businessId, User);

            if (!result.Success) return Unauthorized();

            return Ok(result.Data);
        }
    }
}