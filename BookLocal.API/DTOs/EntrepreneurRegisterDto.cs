namespace BookLocal.API.DTOs
{
    public class EntrepreneurRegisterDto
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }

        public required string BusinessName { get; set; }
        public required string NIP { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Description { get; set; }
    }
}
