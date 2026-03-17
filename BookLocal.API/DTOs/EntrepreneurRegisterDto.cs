using System.ComponentModel.DataAnnotations;

namespace BookLocal.API.DTOs
{
    public class EntrepreneurRegisterDto
    {
        [Required, EmailAddress]
        public required string Email { get; set; }

        [Required, MinLength(6)]
        public required string Password { get; set; }

        [Required]
        public required string FirstName { get; set; }

        [Required]
        public required string LastName { get; set; }

        [Required, RegularExpression(@"^\d{9}$", ErrorMessage = "Numer telefonu musi zawierać 9 cyfr.")]
        public required string PhoneNumber { get; set; }

        [Required, MaxLength(255)]
        public required string BusinessName { get; set; }

        [Required, RegularExpression(@"^\d{10}$", ErrorMessage = "NIP musi zawierać 10 cyfr.")]
        public required string NIP { get; set; }

        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Description { get; set; }
    }
}
