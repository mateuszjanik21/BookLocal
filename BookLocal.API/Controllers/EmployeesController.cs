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
        var business = await _context.Businesses.FindAsync(businessId);
        if (business == null) return NotFound("Firma nie istnieje.");

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (business.OwnerId != userId) return Forbid();

        var employee = await _context.Employees
            .Include(e => e.EmployeeServices)
            .FirstOrDefaultAsync(e => e.EmployeeId == employeeId && e.BusinessId == businessId);

        if (employee == null) return NotFound("Pracownik nie istnieje w tej firmie.");

        employee.EmployeeServices.Clear();

        foreach (var serviceId in assignDto.ServiceIds)
        {
            var serviceExists = await _context.Services.AnyAsync(s => s.ServiceId == serviceId && s.BusinessId == businessId);
            if (serviceExists)
            {
                employee.EmployeeServices.Add(new EmployeeService { ServiceId = serviceId });
            }
        }

        await _context.SaveChangesAsync();
        return Ok(new { Message = "Usługi pracownika zostały zaktualizowane." });
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
}