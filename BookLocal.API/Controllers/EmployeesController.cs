using BookLocal.API.DTOs;
using BookLocal.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

[ApiController]
[Route("api/businesses/{businessId}/[controller]")]
public class EmployeesController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly UserManager<User> _userManager;

    public EmployeesController(AppDbContext context, UserManager<User> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<EmployeeDto>>> GetEmployees(int businessId)
    {
        if (!await _context.Businesses.AnyAsync(b => b.BusinessId == businessId))
            return NotFound("Firma nie istnieje.");

        var employees = await _context.Employees
            .Where(e => e.BusinessId == businessId)
            .Select(e => new EmployeeDto
            {
                Id = e.EmployeeId,
                FirstName = e.FirstName,
                LastName = e.LastName,
                Position = e.Position
            })
            .ToListAsync();

        return Ok(employees);
    }

    [HttpPost]
    [Authorize(Roles = "owner")]
    public async Task<IActionResult> AddEmployee(int businessId, EmployeeUpsertDto employeeDto)
    {
        var business = await _context.Businesses.FindAsync(businessId);
        if (business == null) return NotFound("Firma nie istnieje.");

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (business.OwnerId != userId) return Forbid();

        var employee = new Employee
        {
            FirstName = employeeDto.FirstName,
            LastName = employeeDto.LastName,
            Position = employeeDto.Position,
            BusinessId = businessId
        };

        _context.Employees.Add(employee);
        await _context.SaveChangesAsync();

        var employeeToReturn = new EmployeeDto
        {
            Id = employee.EmployeeId,
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            Position = employee.Position
        };

        return Ok(employeeToReturn);
    }

    [HttpPost("{employeeId}/services")]
    [Authorize(Roles = "owner")]
    public async Task<IActionResult> AssignServicesToEmployee(int businessId, int employeeId, AssignServicesDto assignDto)
    {
        var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var employee = await _context.Employees
            .Include(e => e.Business)
            .FirstOrDefaultAsync(e => e.EmployeeId == employeeId && e.BusinessId == businessId && e.Business.OwnerId == ownerId);

        if (employee == null)
        {
            return Forbid();
        }

        var currentServices = await _context.EmployeeServices
            .Where(es => es.EmployeeId == employeeId)
            .ToListAsync();

        _context.EmployeeServices.RemoveRange(currentServices);

        var newServices = assignDto.ServiceIds.Select(serviceId => new EmployeeService
        {
            EmployeeId = employeeId,
            ServiceId = serviceId
        }).ToList();

        await _context.EmployeeServices.AddRangeAsync(newServices);
        await _context.SaveChangesAsync();

        return Ok(new { Message = "Usługi pracownika zostały pomyślnie zaktualizowane." });
    }

    [HttpGet("/api/businesses/{businessId}/services/{serviceId}/employees")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<EmployeeDto>>> GetEmployeesForService(int businessId, int serviceId)
    {
        var employees = await _context.EmployeeServices
            .Where(es => es.Service.BusinessId == businessId && es.ServiceId == serviceId)
            .Select(es => es.Employee)
            .Select(e => new EmployeeDto
            {
                Id = e.EmployeeId,
                FirstName = e.FirstName,
                LastName = e.LastName,
                Position = e.Position
            })
            .ToListAsync();

        return Ok(employees);
    }

    [HttpGet("{employeeId}/services")]
    [Authorize(Roles = "owner")]
    public async Task<ActionResult<IEnumerable<int>>> GetAssignedServiceIdsForEmployee(int businessId, int employeeId)
    {
        var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isEmployeeInBusiness = await _context.Employees
            .AnyAsync(e => e.EmployeeId == employeeId && e.Business.OwnerId == ownerId && e.BusinessId == businessId);

        if (!isEmployeeInBusiness)
        {
            return Forbid();
        }

        var serviceIds = await _context.EmployeeServices
            .Where(es => es.EmployeeId == employeeId)
            .Select(es => es.ServiceId)
            .ToListAsync();

        return Ok(serviceIds);
    }

    [HttpPut("{employeeId}")]
    [Authorize(Roles = "owner")]
    public async Task<IActionResult> UpdateEmployee(int businessId, int employeeId, EmployeeUpsertDto employeeDto)
    {
        var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var employee = await _context.Employees
            .FirstOrDefaultAsync(e => e.EmployeeId == employeeId && e.BusinessId == businessId && e.Business.OwnerId == ownerId);

        if (employee == null)
        {
            return NotFound("Pracownik nie został znaleziony lub nie masz do niego dostępu.");
        }

        employee.FirstName = employeeDto.FirstName;
        employee.LastName = employeeDto.LastName;
        employee.Position = employeeDto.Position;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{employeeId}")]
    [Authorize(Roles = "owner")]
    public async Task<IActionResult> DeleteEmployee(int businessId, int employeeId)
    {
        var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var employee = await _context.Employees
            .FirstOrDefaultAsync(e => e.EmployeeId == employeeId && e.BusinessId == businessId && e.Business.OwnerId == ownerId);

        if (employee == null)
        {
            return NotFound("Pracownik nie został znaleziony lub nie masz do niego dostępu.");
        }

        _context.Employees.Remove(employee);
        await _context.SaveChangesAsync();
        return NoContent();
    }

}