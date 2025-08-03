using System.ComponentModel.DataAnnotations;

namespace BookLocal.API.DTOs
{
    public class ChangePasswordDto
    {
        [Required]
        public required string CurrentPassword { get; set; }

        [Required]
        public required string NewPassword { get; set; }
    }
}
