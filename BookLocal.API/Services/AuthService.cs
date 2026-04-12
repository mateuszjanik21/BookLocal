using BookLocal.API.DTOs;
using BookLocal.API.Interfaces;
using BookLocal.Data.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace BookLocal.API.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly AppDbContext _context;
        private readonly TokenService _tokenService;
        private readonly ILazyStateService _lazyStateService;

        public AuthService(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            AppDbContext context,
            TokenService tokenService,
            ILazyStateService lazyStateService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _tokenService = tokenService;
            _lazyStateService = lazyStateService;
        }

        public async Task<(bool Success, AuthResponseDto? Data, string? ErrorMessage)> LoginAsync(LoginDto loginDto)
        {
            var user = await _userManager.FindByEmailAsync(loginDto.Email);
            if (user == null) return (false, null, "Nieprawidłowy email lub hasło.");

            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);
            if (!result.Succeeded) return (false, null, "Nieprawidłowy email lub hasło.");

            var authResponse = await CreateAuthResponse(user);
            return (true, authResponse, null);
        }

        public async Task<(bool Success, IEnumerable<IdentityError>? Errors, string? ErrorMessage)> RegisterCustomerAsync(RegisterDto dto)
        {
            if (await _userManager.FindByEmailAsync(dto.Email) != null)
                return (false, null, "Użytkownik o podanym adresie email już istnieje.");

            var user = new User
            {
                UserName = dto.Email,
                Email = dto.Email,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                PhotoUrl = "https://api.dicebear.com/8.x/initials/svg?seed=" + dto.FirstName + " " + dto.LastName
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded) return (false, result.Errors, null);

            await _userManager.AddToRoleAsync(user, "customer");
            return (true, null, null);
        }

        public async Task<(bool Success, AuthResponseDto? Data, IEnumerable<IdentityError>? Errors, string? ErrorMessage)> RegisterOwnerAsync(EntrepreneurRegisterDto dto)
        {
            if (await _userManager.FindByEmailAsync(dto.Email) != null)
                return (false, null, null, "Użytkownik o podanym adresie email już istnieje.");

            var user = new User
            {
                UserName = dto.Email,
                Email = dto.Email,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                PhoneNumber = dto.PhoneNumber,
                PhotoUrl = "https://api.dicebear.com/8.x/initials/svg?seed=" + dto.FirstName + " " + dto.LastName
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded) return (false, null, result.Errors, null);

            await _userManager.AddToRoleAsync(user, "owner");

            var business = new Business
            {
                Name = dto.BusinessName,
                NIP = dto.NIP,
                Address = dto.Address,
                City = dto.City,
                Description = dto.Description,
                Owner = user
            };
            _context.Businesses.Add(business);

            var employee = new Employee
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Position = "Właściciel",
                Business = business
            };
            _context.Employees.Add(employee);

            await _context.SaveChangesAsync();

            var authResponse = await CreateAuthResponse(user);
            return (true, authResponse, null, null);
        }

        public async Task<(bool Success, string? UserId, string? Email, IEnumerable<IdentityError>? Errors, string? ErrorMessage)> CreateUserByOwnerAsync(RegisterDto dto)
        {
            if (await _userManager.FindByEmailAsync(dto.Email) != null)
                return (false, null, null, null, "Użytkownik o podanym adresie email już istnieje.");

            var user = new User
            {
                UserName = dto.Email,
                Email = dto.Email,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                PhotoUrl = "https://api.dicebear.com/8.x/initials/svg?seed=" + dto.FirstName + " " + dto.LastName
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded) return (false, null, null, result.Errors, null);

            await _userManager.AddToRoleAsync(user, "customer");

            return (true, user.Id, user.Email, null, null);
        }

        public async Task<(bool Success, IEnumerable<IdentityError>? Errors)> ChangePasswordAsync(ClaimsPrincipal userClaims, ChangePasswordDto dto)
        {
            var user = await _userManager.GetUserAsync(userClaims);
            if (user == null) return (false, null);

            var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
            if (!result.Succeeded) return (false, result.Errors);

            return (true, null);
        }

        public async Task<UserDto?> GetCurrentUserAsync(ClaimsPrincipal userClaims)
        {
            var user = await _userManager.GetUserAsync(userClaims);
            if (user == null) return null;

            var roles = await _userManager.GetRolesAsync(user);

            await _lazyStateService.SyncUserStateAsync(user.Id, roles.FirstOrDefault() ?? "customer");

            return new UserDto
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                FirstName = user.FirstName ?? string.Empty,
                LastName = user.LastName ?? string.Empty,
                PhotoUrl = user.PhotoUrl,
                PhoneNumber = user.PhoneNumber,
                Roles = (List<string>)roles
            };
        }

        public async Task<(bool Success, UserDto? Data)> UpdateProfileAsync(ClaimsPrincipal userClaims, UserUpdateDto dto)
        {
            var user = await _userManager.GetUserAsync(userClaims);
            if (user == null) return (false, null);

            user.FirstName = dto.FirstName;
            user.LastName = dto.LastName;
            user.PhoneNumber = dto.PhoneNumber;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var userDto = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email ?? string.Empty,
                    FirstName = user.FirstName ?? string.Empty,
                    LastName = user.LastName ?? string.Empty,
                    PhotoUrl = user.PhotoUrl,
                    PhoneNumber = user.PhoneNumber,
                    Roles = (List<string>)roles
                };
                return (true, userDto);
            }

            return (false, null);
        }

        private async Task<AuthResponseDto> CreateAuthResponse(User user)
        {
            var roles = await _userManager.GetRolesAsync(user);

            await _lazyStateService.SyncUserStateAsync(user.Id, roles.FirstOrDefault() ?? "customer");

            var userDto = new UserDto
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                FirstName = user.FirstName ?? string.Empty,
                LastName = user.LastName ?? string.Empty,
                PhotoUrl = user.PhotoUrl,
                PhoneNumber = user.PhoneNumber,
                Roles = (List<string>)roles
            };

            return new AuthResponseDto
            {
                Token = await _tokenService.GenerateToken(user),
                User = userDto
            };
        }
    }
}
