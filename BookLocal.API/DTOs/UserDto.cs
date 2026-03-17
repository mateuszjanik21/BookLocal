using System.ComponentModel.DataAnnotations;

namespace BookLocal.API.DTOs
{
    public class UserDto
    {
        public required string Id { get; set; }
        public required string Email { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public string? PhotoUrl { get; set; }
        public string? PhoneNumber { get; set; }
        public required IList<string> Roles { get; set; }
    }

    public class UserUpdateDto
    {
        [Required]
        public required string FirstName { get; set; }

        [Required]
        public required string LastName { get; set; }

        [RegularExpression(@"^\d{9}$", ErrorMessage = "Numer telefonu musi zawierać 9 cyfr.")]
        public string? PhoneNumber { get; set; }
    }
}
