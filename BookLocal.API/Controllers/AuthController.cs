using BookLocal.API.DTOs;
using BookLocal.API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookLocal.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<AuthResponseDto>> Login(LoginDto loginDto)
        {
            var result = await _authService.LoginAsync(loginDto);
            if (!result.Success) return Unauthorized(result.ErrorMessage);

            return Ok(result.Data);
        }

        [HttpPost("register-customer")]
        [AllowAnonymous]
        public async Task<IActionResult> RegisterCustomer(RegisterDto registerDto)
        {
            var result = await _authService.RegisterCustomerAsync(registerDto);
            if (!result.Success)
            {
                if (result.ErrorMessage != null)
                    return BadRequest(new { title = result.ErrorMessage });
                return BadRequest(result.Errors);
            }

            return Ok(new { Message = "Rejestracja klienta pomyślna." });
        }

        [HttpPost("register-owner")]
        [AllowAnonymous]
        public async Task<ActionResult<AuthResponseDto>> RegisterOwner([FromBody] EntrepreneurRegisterDto dto)
        {
            var result = await _authService.RegisterOwnerAsync(dto);
            if (!result.Success)
            {
                if (result.ErrorMessage != null)
                    return BadRequest(new { title = result.ErrorMessage });
                return BadRequest(result.Errors);
            }

            return Ok(result.Data);
        }

        [HttpPost("create-user")]
        [Authorize(Roles = "owner")]
        public async Task<IActionResult> CreateUserByOwner(RegisterDto registerDto)
        {
            var result = await _authService.CreateUserByOwnerAsync(registerDto);
            if (!result.Success)
            {
                if (result.ErrorMessage != null)
                    return BadRequest(result.ErrorMessage);
                return BadRequest(result.Errors);
            }

            return Ok(new { Id = result.UserId, Email = result.Email });
        }

        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto changePasswordDto)
        {
            var result = await _authService.ChangePasswordAsync(User, changePasswordDto);
            if (!result.Success)
            {
                if (result.Errors != null) return BadRequest(result.Errors);
                return Unauthorized();
            }

            return Ok(new { Message = "Hasło zostało pomyślnie zmienione." });
        }

        [HttpGet("currentuser")]
        [Authorize]
        public async Task<ActionResult<UserDto>> GetCurrentUser()
        {
            var user = await _authService.GetCurrentUserAsync(User);
            if (user == null) return Unauthorized();

            return Ok(user);
        }

        [HttpPut("profile")]
        [Authorize]
        public async Task<ActionResult<UserDto>> UpdateProfile(UserUpdateDto updateDto)
        {
            var result = await _authService.UpdateProfileAsync(User, updateDto);
            if (!result.Success || result.Data == null)
            {
                if (result.Data == null && !result.Success) return Unauthorized();
                return BadRequest("Nie udało się zaktualizować profilu.");
            }

            return Ok(result.Data);
        }
    }
}