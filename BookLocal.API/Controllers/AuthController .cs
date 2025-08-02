using BookLocal.API.DTOs;
using BookLocal.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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

        var roles = await _userManager.GetRolesAsync(user);
        var userDto = new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Roles = roles
        };

        return Ok(new AuthResponseDto
        {
            Token = await _tokenService.GenerateToken(user),
            User = userDto
        });
    }

    [HttpPost("register-customer")]
    [AllowAnonymous]
    public async Task<IActionResult> RegisterCustomer(RegisterDto registerDto)
    {
        if (await _userManager.FindByEmailAsync(registerDto.Email) != null)
        {
            return BadRequest("Użytkownik o podanym adresie email już istnieje.");
        }

        var user = new User
        {
            UserName = registerDto.Email,
            Email = registerDto.Email,
            FirstName = registerDto.FirstName,
            LastName = registerDto.LastName
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
        {
            return BadRequest("Użytkownik o podanym adresie email już istnieje.");
        }

        var user = new User
        {
            UserName = dto.Email,
            Email = dto.Email,
            FirstName = dto.FirstName,
            LastName = dto.LastName
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
            OwnerId = user.Id
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

        var roles = await _userManager.GetRolesAsync(user);
        var userDto = new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Roles = roles
        };

        return Ok(new AuthResponseDto
        {
            Token = await _tokenService.GenerateToken(user),
            User = userDto
        });
    }

    [Authorize]
    [HttpGet("currentuser")]
    public async Task<ActionResult<UserDto>> GetCurrentUser()
    {
        var email = User.FindFirstValue(ClaimTypes.Email);
        var user = await _userManager.FindByEmailAsync(email);
        var roles = await _userManager.GetRolesAsync(user);

        return new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Roles = roles
        };
    }
}