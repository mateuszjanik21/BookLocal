using BookLocal.API.DTOs;
using BookLocal.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BookLocal.API.Controllers
{
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
                .Include(e => e.EmployeeDetails)
                .Include(e => e.FinanceSettings)
                .Where(e => e.BusinessId == businessId)
                .Select(e => new EmployeeDto
                {
                    Id = e.EmployeeId,
                    FirstName = e.FirstName,
                    LastName = e.LastName,
                    Position = e.Position,
                    PhotoUrl = e.PhotoUrl,
                    DateOfBirth = e.DateOfBirth,
                    Specialization = e.EmployeeDetails != null ? e.EmployeeDetails.Specialization : null,
                    InstagramProfileUrl = e.EmployeeDetails != null ? e.EmployeeDetails.InstagramProfileUrl : null,
                    PortfolioUrl = e.EmployeeDetails != null ? e.EmployeeDetails.PortfolioUrl : null,
                    IsStudent = e.FinanceSettings != null ? e.FinanceSettings.IsStudent : false
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
                BusinessId = businessId,
                DateOfBirth = employeeDto.DateOfBirth
            };

            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

            var details = new EmployeeDetails
            {
                EmployeeId = employee.EmployeeId,
                Bio = employeeDto.Bio,
                Specialization = employeeDto.Specialization,
                InstagramProfileUrl = employeeDto.InstagramProfileUrl,
                PortfolioUrl = employeeDto.PortfolioUrl
            };
            _context.EmployeeDetails.Add(details);

            var finance = new EmployeeFinanceSettings
            {
                EmployeeId = employee.EmployeeId,
                CommuteType = WorkCommuteType.Local,
                HasPit2Filed = true,
                IsStudent = employeeDto.IsStudent
            };
            _context.EmployeeFinanceSettings.Add(finance);

            await _context.SaveChangesAsync();

            var employeeToReturn = new EmployeeDto
            {
                Id = employee.EmployeeId,
                FirstName = employee.FirstName,
                LastName = employee.LastName,
                Position = employee.Position,
                DateOfBirth = employee.DateOfBirth,
                Specialization = details.Specialization,
                IsStudent = finance.IsStudent
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
                    Position = e.Position,
                    PhotoUrl = e.PhotoUrl,
                    DateOfBirth = e.DateOfBirth,
                    Specialization = e.EmployeeDetails != null ? e.EmployeeDetails.Specialization : null,
                    InstagramProfileUrl = e.EmployeeDetails != null ? e.EmployeeDetails.InstagramProfileUrl : null,
                    PortfolioUrl = e.EmployeeDetails != null ? e.EmployeeDetails.PortfolioUrl : null
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
                .Include(e => e.EmployeeDetails)
                .Include(e => e.FinanceSettings)
                .FirstOrDefaultAsync(e => e.EmployeeId == employeeId && e.BusinessId == businessId && e.Business.OwnerId == ownerId);

            if (employee == null)
            {
                return NotFound("Pracownik nie został znaleziony lub nie masz do niego dostępu.");
            }

            employee.FirstName = employeeDto.FirstName;
            employee.LastName = employeeDto.LastName;
            employee.Position = employeeDto.Position;
            employee.DateOfBirth = employeeDto.DateOfBirth;

            if (employee.EmployeeDetails == null)
            {
                employee.EmployeeDetails = new EmployeeDetails();
            }

            employee.EmployeeDetails.Bio = employeeDto.Bio;
            employee.EmployeeDetails.Specialization = employeeDto.Specialization;
            employee.EmployeeDetails.InstagramProfileUrl = employeeDto.InstagramProfileUrl;
            employee.EmployeeDetails.PortfolioUrl = employeeDto.PortfolioUrl;

            if (employee.FinanceSettings == null)
            {
                employee.FinanceSettings = new EmployeeFinanceSettings
                {
                    EmployeeId = employee.EmployeeId,
                    IsStudent = employeeDto.IsStudent
                };
                _context.EmployeeFinanceSettings.Add(employee.FinanceSettings);
            }
            else
            {
                employee.FinanceSettings.IsStudent = employeeDto.IsStudent;
            }

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

        [HttpGet("{employeeId}/details")]
        [Authorize(Roles = "owner")]
        public async Task<ActionResult<EmployeeDetailDto>> GetEmployeeDetails(int businessId, int employeeId)
        {
            var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var employee = await _context.Employees
                .Include(e => e.Business)
                .Include(e => e.WorkSchedules)
                .Include(e => e.EmployeeDetails)
                .Include(e => e.FinanceSettings)
                .Include(e => e.EmployeeServices)
                    .ThenInclude(es => es.Service)
                        .ThenInclude(s => s.Variants)
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.EmployeeId == employeeId && e.BusinessId == businessId && e.Business.OwnerId == ownerId);

            if (employee == null)
            {
                return NotFound("Pracownik nie został znaleziony lub nie masz do niego dostępu.");
            }

            var estimatedRevenue = await _context.Reservations
                .Where(r => r.EmployeeId == employeeId && r.Status == ReservationStatus.Completed)
                .SumAsync(r => r.AgreedPrice);

            var employeeDetails = new EmployeeDetailDto
            {
                Id = employee.EmployeeId,
                FirstName = employee.FirstName,
                LastName = employee.LastName,
                Position = employee.Position,
                PhotoUrl = employee.PhotoUrl,
                Bio = employee.EmployeeDetails?.Bio,
                Specialization = employee.EmployeeDetails?.Specialization,
                InstagramProfileUrl = employee.EmployeeDetails?.InstagramProfileUrl,
                PortfolioUrl = employee.EmployeeDetails?.PortfolioUrl,
                IsStudent = employee.FinanceSettings != null ? employee.FinanceSettings.IsStudent : false,

                EstimatedRevenue = estimatedRevenue,

                WorkSchedules = employee.WorkSchedules.Select(ws => new WorkScheduleDto
                {
                    DayOfWeek = ws.DayOfWeek,
                    StartTime = ws.StartTime?.ToString(@"hh\:mm"),
                    EndTime = ws.EndTime?.ToString(@"hh\:mm"),
                    IsDayOff = ws.IsDayOff
                }).OrderBy(ws => ws.DayOfWeek).ToList(),

                AssignedServices = employee.EmployeeServices.Select(es => new ServiceDto
                {
                    Id = es.Service.ServiceId,
                    Name = es.Service.Name,
                    Description = es.Service.Description,
                    ServiceCategoryId = es.Service.ServiceCategoryId,
                    IsArchived = es.Service.IsArchived,
                    Variants = es.Service.Variants.Select(v => new ServiceVariantDto
                    {
                        ServiceVariantId = v.ServiceVariantId,
                        Name = v.Name,
                        Price = v.Price,
                        DurationMinutes = v.DurationMinutes,
                        IsDefault = v.IsDefault
                    }).ToList()
                }).ToList()
            };

            return Ok(employeeDetails);
        }
    }
}