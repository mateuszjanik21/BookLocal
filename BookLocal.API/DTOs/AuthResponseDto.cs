namespace BookLocal.API.DTOs
{
    public class AuthResponseDto
    {
        public required string Token { get; set; }
        public required UserDto User { get; set; }
    }
}
