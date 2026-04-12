using BookLocal.API.DTOs;
using BookLocal.API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookLocal.API.Controllers
{
    [ApiController]
    [Route("api/businesses/{businessId}/[controller]")]
    public class EmployeesController : ControllerBase
    {
        private readonly IEmployeesService _employeesService;

        public EmployeesController(IEmployeesService employeesService)
        {
            _employeesService = employeesService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<EmployeeDto>>> GetEmployees(int businessId)
        {
            var result = await _employeesService.GetEmployeesAsync(businessId);

            if (!result.Success) return NotFound(result.ErrorMessage);

            return Ok(result.Data);
        }

        [HttpPost]
        [Authorize(Roles = "owner")]
        public async Task<IActionResult> AddEmployee(int businessId, EmployeeUpsertDto employeeDto)
        {
            var result = await _employeesService.AddEmployeeAsync(businessId, employeeDto, User);

            if (!result.Success)
            {
                if (result.ErrorMessage == "Brak uprawnień.") return Forbid();
                return NotFound(result.ErrorMessage);
            }

            return Ok(result.Data);
        }

        [HttpPost("{employeeId}/services")]
        [Authorize(Roles = "owner")]
        public async Task<IActionResult> AssignServicesToEmployee(int businessId, int employeeId, AssignServicesDto assignDto)
        {
            var result = await _employeesService.AssignServicesToEmployeeAsync(businessId, employeeId, assignDto, User);

            if (!result.Success) return Forbid();

            return Ok(new { result.Message });
        }

        [HttpGet("/api/businesses/{businessId}/services/{serviceId}/employees")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<EmployeeDto>>> GetEmployeesForService(int businessId, int serviceId)
        {
            var result = await _employeesService.GetEmployeesForServiceAsync(businessId, serviceId);

            return Ok(result.Data);
        }

        [HttpGet("{employeeId}/services")]
        [Authorize(Roles = "owner")]
        public async Task<ActionResult<IEnumerable<int>>> GetAssignedServiceIdsForEmployee(int businessId, int employeeId)
        {
            var result = await _employeesService.GetAssignedServiceIdsForEmployeeAsync(businessId, employeeId, User);

            if (!result.Success) return Forbid();

            return Ok(result.Data);
        }

        [HttpPut("{employeeId}")]
        [Authorize(Roles = "owner")]
        public async Task<IActionResult> UpdateEmployee(int businessId, int employeeId, EmployeeUpsertDto employeeDto)
        {
            var result = await _employeesService.UpdateEmployeeAsync(businessId, employeeId, employeeDto, User);

            if (!result.Success) return NotFound(result.ErrorMessage);

            return NoContent();
        }

        [HttpDelete("{employeeId}")]
        [Authorize(Roles = "owner")]
        public async Task<IActionResult> ArchiveEmployee(int businessId, int employeeId)
        {
            var result = await _employeesService.ArchiveEmployeeAsync(businessId, employeeId, User);

            if (!result.Success) return NotFound(result.ErrorMessage);

            return Ok(new { message = result.Message });
        }

        [HttpGet("{employeeId}/details")]
        [Authorize(Roles = "owner")]
        public async Task<ActionResult<EmployeeDetailDto>> GetEmployeeDetails(int businessId, int employeeId)
        {
            var result = await _employeesService.GetEmployeeDetailsAsync(businessId, employeeId, User);

            if (!result.Success) return NotFound(result.ErrorMessage);

            return Ok(result.Data);
        }

        [HttpPut("{employeeId}/finance-settings")]
        [Authorize(Roles = "owner")]
        public async Task<IActionResult> UpdateFinanceSettings(int businessId, int employeeId, FinanceSettingsDto dto)
        {
            var result = await _employeesService.UpdateFinanceSettingsAsync(businessId, employeeId, dto, User);

            if (!result.Success) return NotFound(result.ErrorMessage);

            return NoContent();
        }

        [HttpPost("{employeeId}/certificates")]
        [Authorize(Roles = "owner")]
        public async Task<IActionResult> AddCertificate(int businessId, int employeeId, [FromBody] CreateCertificateDto dto)
        {
            var result = await _employeesService.AddCertificateAsync(businessId, employeeId, dto, User);

            if (!result.Success)
            {
                if (result.ErrorMessage == "Brak uprawnień.") return Forbid();
                if (result.ErrorMessage == "Nie znaleziono pracownika.") return NotFound(result.ErrorMessage);
                return BadRequest(new { message = result.ErrorMessage });
            }

            return Ok(result.Data);
        }

        [HttpDelete("{employeeId}/certificates/{certId}")]
        [Authorize(Roles = "owner")]
        public async Task<IActionResult> DeleteCertificate(int businessId, int employeeId, int certId)
        {
            var result = await _employeesService.DeleteCertificateAsync(businessId, employeeId, certId, User);

            if (!result.Success)
            {
                if (result.ErrorMessage == "Brak uprawnień") return Forbid();
                return NotFound(result.ErrorMessage);
            }

            return Ok(new { result.Message });
        }

        [HttpPost("{employeeId}/absences")]
        [Authorize(Roles = "owner")]
        public async Task<IActionResult> AddAbsence(int businessId, int employeeId, [FromBody] CreateAbsenceDto dto)
        {
            var result = await _employeesService.AddAbsenceAsync(businessId, employeeId, dto, User);

            if (!result.Success)
            {
                if (result.ErrorMessage == "Brak uprawnień") return Forbid();
                if (result.ErrorMessage == "Nie znaleziono pracownika.") return NotFound(result.ErrorMessage);
                if (result.ErrorMessage == "W wybranym okresie istnieje już inna nieobecność.") return BadRequest(new { message = result.ErrorMessage });
                return BadRequest(result.ErrorMessage);
            }

            return Ok(result.Data);
        }

        [HttpPatch("{employeeId}/absences/{absenceId}/approve")]
        [Authorize(Roles = "owner")]
        public async Task<IActionResult> ToggleAbsenceApproval(int businessId, int employeeId, int absenceId)
        {
            var result = await _employeesService.ToggleAbsenceApprovalAsync(businessId, employeeId, absenceId, User);

            if (!result.Success)
            {
                if (result.ErrorMessage == "Brak uprawnień") return Forbid();
                return NotFound(result.ErrorMessage);
            }

            return Ok(result.Data);
        }

        [HttpDelete("{employeeId}/absences/{absenceId}")]
        [Authorize(Roles = "owner")]
        public async Task<IActionResult> DeleteAbsence(int businessId, int employeeId, int absenceId)
        {
            var result = await _employeesService.DeleteAbsenceAsync(businessId, employeeId, absenceId, User);

            if (!result.Success)
            {
                if (result.ErrorMessage == "Brak uprawnień.") return Forbid();
                return NotFound(result.ErrorMessage);
            }

            return Ok(new { result.Message });
        }
    }
}