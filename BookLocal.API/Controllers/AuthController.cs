using BookLocal.API.DTOs;
using BookLocal.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly AppDbContext _context;
    private readonly TokenService _tokenService;

    public AuthController(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        AppDbContext context,
        TokenService tokenService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _context = context;
        _tokenService = tokenService;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginDto loginDto)
    {
        var user = await _userManager.FindByEmailAsync(loginDto.Email);
        if (user == null) return Unauthorized("Nieprawidłowy email lub hasło.");

        var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);
        if (!result.Succeeded) return Unauthorized("Nieprawidłowy email lub hasło.");

        return await CreateAuthResponse(user);
    }

    [HttpPost("register-customer")]
    [AllowAnonymous]
    public async Task<IActionResult> RegisterCustomer(RegisterDto registerDto)
    {
        if (await _userManager.FindByEmailAsync(registerDto.Email) != null)
            return BadRequest(new { title = "Użytkownik o podanym adresie email już istnieje." });

        var user = new User
        {
            UserName = registerDto.Email,
            Email = registerDto.Email,
            FirstName = registerDto.FirstName,
            LastName = registerDto.LastName,
            PhotoUrl = "https://api.dicebear.com/8.x/initials/svg?seed=" + registerDto.FirstName + " " + registerDto.LastName
        };

        var result = await _userManager.CreateAsync(user, registerDto.Password);
        if (!result.Succeeded) return BadRequest(result.Errors);

        await _userManager.AddToRoleAsync(user, "customer");
        return Ok(new { Message = "Rejestracja klienta pomyślna." });
    }

    [HttpPost("register-owner")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponseDto>> RegisterOwner([FromBody] EntrepreneurRegisterDto dto)
    {
        if (await _userManager.FindByEmailAsync(dto.Email) != null)
            return BadRequest(new { title = "Użytkownik o podanym adresie email już istnieje." });

        var user = new User
        {
            UserName = dto.Email,
            Email = dto.Email,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            PhotoUrl = "https://api.dicebear.com/8.x/initials/svg?seed=" + dto.FirstName + " " + dto.LastName
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded) return BadRequest(result.Errors);

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

        return await CreateAuthResponse(user);
    }

    [HttpPost("create-user")]
    [Authorize(Roles = "owner")]
    public async Task<IActionResult> CreateUserByOwner(RegisterDto registerDto)
    {
        if (await _userManager.FindByEmailAsync(registerDto.Email) != null)
            return BadRequest("Użytkownik o podanym adresie email już istnieje.");

        var user = new User
        {
            UserName = registerDto.Email,
            Email = registerDto.Email,
            FirstName = registerDto.FirstName,
            LastName = registerDto.LastName,
            PhotoUrl = "https://api.dicebear.com/8.x/initials/svg?seed=" + registerDto.FirstName + " " + registerDto.LastName
        };

        var result = await _userManager.CreateAsync(user, registerDto.Password);
        if (!result.Succeeded) return BadRequest(result.Errors);

        await _userManager.AddToRoleAsync(user, "customer");

        return Ok(new { user.Id, user.Email });
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword(ChangePasswordDto changePasswordDto)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var result = await _userManager.ChangePasswordAsync(user, changePasswordDto.CurrentPassword, changePasswordDto.NewPassword);
        if (!result.Succeeded) return BadRequest(result.Errors);

        return Ok(new { Message = "Hasło zostało pomyślnie zmienione." });
    }

    [HttpGet("currentuser")]
    [Authorize]
    public async Task<ActionResult<UserDto>> GetCurrentUser()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();
        var roles = await _userManager.GetRolesAsync(user);

        return new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhotoUrl = user.PhotoUrl,
            Roles = (List<string>)roles
        };
    }

    [Authorize]
    [HttpPut("profile")]
    public async Task<ActionResult<UserDto>> UpdateProfile(UserUpdateDto updateDto)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        user.FirstName = updateDto.FirstName;
        user.LastName = updateDto.LastName;

        var result = await _userManager.UpdateAsync(user);

        if (result.Succeeded)
        {
            var roles = await _userManager.GetRolesAsync(user);
            return new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhotoUrl = user.PhotoUrl,
                Roles = (List<string>)roles
            };
        }

        return BadRequest("Nie udało się zaktualizować profilu.");
    }

    private async Task<AuthResponseDto> CreateAuthResponse(User user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        var userDto = new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhotoUrl = user.PhotoUrl,
            Roles = (List<string>)roles
        };

        return new AuthResponseDto
        {
            Token = await _tokenService.GenerateToken(user),
            User = userDto
        };
    }
}