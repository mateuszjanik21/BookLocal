using BookLocal.API.DTOs;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace BookLocal.API.Interfaces
{
    public interface IAuthService
    {
        Task<(bool Success, AuthResponseDto? Data, string? ErrorMessage)> LoginAsync(LoginDto loginDto);
        Task<(bool Success, IEnumerable<IdentityError>? Errors, string? ErrorMessage)> RegisterCustomerAsync(RegisterDto dto);
        Task<(bool Success, AuthResponseDto? Data, IEnumerable<IdentityError>? Errors, string? ErrorMessage)> RegisterOwnerAsync(EntrepreneurRegisterDto dto);
        Task<(bool Success, string? UserId, string? Email, IEnumerable<IdentityError>? Errors, string? ErrorMessage)> CreateUserByOwnerAsync(RegisterDto dto);
        Task<(bool Success, IEnumerable<IdentityError>? Errors)> ChangePasswordAsync(ClaimsPrincipal userClaims, ChangePasswordDto dto);
        Task<UserDto?> GetCurrentUserAsync(ClaimsPrincipal userClaims);
        Task<(bool Success, UserDto? Data)> UpdateProfileAsync(ClaimsPrincipal userClaims, UserUpdateDto dto);
    }
}
