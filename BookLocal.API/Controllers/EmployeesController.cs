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
                    IsStudent = e.FinanceSettings != null ? e.FinanceSettings.IsStudent : false,
                    IsArchived = e.IsArchived
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
        public async Task<IActionResult> ArchiveEmployee(int businessId, int employeeId)
        {
            var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.EmployeeId == employeeId && e.BusinessId == businessId && e.Business.OwnerId == ownerId);

            if (employee == null)
            {
                return NotFound("Pracownik nie został znaleziony lub nie masz do niego dostępu.");
            }

            employee.IsArchived = true;
            var futureReservations = await _context.Reservations
                .Where(r => r.EmployeeId == employeeId && r.StartTime > DateTime.UtcNow && r.Status != ReservationStatus.Cancelled && r.Status != ReservationStatus.Completed)
                .ToListAsync();

            foreach (var res in futureReservations)
            {
                res.Status = ReservationStatus.Cancelled;
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = $"Pracownik zarchiwizowany. Anulowano {futureReservations.Count} nadchodzących rezerwacji." });
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
                .Include(e => e.Contracts)
                .Include(e => e.Payrolls)
                .Include(e => e.ScheduleExceptions)
                .Include(e => e.Reservations)
                    .ThenInclude(r => r.ServiceVariant)
                        .ThenInclude(sv => sv.Service)
                .Include(e => e.Reservations)
                    .ThenInclude(r => r.Customer)
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.EmployeeId == employeeId && e.BusinessId == businessId && e.Business.OwnerId == ownerId);

            if (employee == null)
            {
                return NotFound("Pracownik nie został znaleziony lub nie masz do niego dostępu.");
            }

            var completedReservations = await _context.Reservations
                .Where(r => r.EmployeeId == employeeId && r.Status == ReservationStatus.Completed)
                .ToListAsync();

            var estimatedRevenue = completedReservations.Sum(r => r.AgreedPrice);

            var certificates = await _context.EmployeeCertificates
                .Where(c => c.EmployeeId == employeeId)
                .AsNoTracking()
                .ToListAsync();

            var now = DateTime.Now;
            var weekEnd = now.AddDays(7);
            var upcomingReservations = await _context.Reservations
                .Include(r => r.ServiceVariant)
                    .ThenInclude(sv => sv.Service)
                .Include(r => r.Customer)
                .Where(r => r.EmployeeId == employeeId
                    && r.Status == ReservationStatus.Confirmed
                    && r.StartTime >= now
                    && r.StartTime <= weekEnd)
                .OrderBy(r => r.StartTime)
                .AsNoTracking()
                .ToListAsync();

            var employeeDetails = new EmployeeDetailDto
            {
                Id = employee.EmployeeId,
                FirstName = employee.FirstName,
                LastName = employee.LastName,
                Position = employee.Position,
                PhotoUrl = employee.PhotoUrl,
                DateOfBirth = employee.DateOfBirth,
                IsArchived = employee.IsArchived,

                Bio = employee.EmployeeDetails?.Bio,
                Specialization = employee.EmployeeDetails?.Specialization,
                Hobbies = employee.EmployeeDetails?.Hobbies,
                InstagramProfileUrl = employee.EmployeeDetails?.InstagramProfileUrl,
                PortfolioUrl = employee.EmployeeDetails?.PortfolioUrl,
                IsStudent = employee.FinanceSettings?.IsStudent ?? false,

                EstimatedRevenue = estimatedRevenue,
                CompletedReservationsCount = completedReservations.Count,

                WorkSchedules = employee.WorkSchedules.Select(ws => new WorkScheduleDto
                {
                    DayOfWeek = ws.DayOfWeek,
                    StartTime = ws.StartTime.HasValue ? ws.StartTime.Value.ToString(@"hh\:mm") : null,
                    EndTime = ws.EndTime.HasValue ? ws.EndTime.Value.ToString(@"hh\:mm") : null,
                    IsDayOff = ws.IsDayOff
                }).OrderBy(ws => ws.DayOfWeek).ToList(),

                AssignedServices = employee.EmployeeServices.Select(es => new ServiceDto
                {
                    Id = es.ServiceId,
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
                }).ToList(),

                Certificates = certificates.Select(c => new EmployeeCertificateDto
                {
                    CertificateId = c.CertificateId,
                    Name = c.Name,
                    Institution = c.Institution,
                    DateObtained = c.DateObtained,
                    ImageUrl = c.ImageUrl,
                    IsVisibleToClient = c.IsVisibleToClient
                }).ToList(),

                Contracts = employee.Contracts.Select(c => new EmploymentContractDto
                {
                    ContractId = c.ContractId,
                    EmployeeId = c.EmployeeId,
                    EmployeeName = $"{employee.FirstName} {employee.LastName}",
                    ContractType = c.ContractType,
                    BaseSalary = c.BaseSalary,
                    TaxDeductibleExpenses = c.TaxDeductibleExpenses,
                    StartDate = c.StartDate,
                    EndDate = c.EndDate,
                    IsActive = c.IsActive
                }).OrderByDescending(c => c.StartDate).ToList(),

                Payrolls = employee.Payrolls.Select(p => new EmployeePayrollDto
                {
                    PayrollId = p.PayrollId,
                    EmployeeId = p.EmployeeId,
                    EmployeeName = $"{employee.FirstName} {employee.LastName}",
                    PeriodMonth = p.PeriodMonth,
                    PeriodYear = p.PeriodYear,
                    GrossAmount = p.GrossAmount,
                    NetAmount = p.NetAmount,
                    TotalEmployerCost = p.TotalEmployerCost,
                    Status = p.Status,
                    PaidAt = p.PaidAt,

                    BaseSalary = p.BaseSalaryComponent,
                    CommissionComponent = p.CommissionComponent,
                    BonusComponent = p.BonusComponent,
                    PensionContribution = p.SocialSecurityTax,
                    DisabilityContribution = 0,
                    SicknessContribution = 0,
                    HealthInsuranceContribution = p.HealthInsuranceTax,
                    TaxAdvance = p.IncomeTaxAdvance,
                    PPKAmount = 0
                }).OrderByDescending(p => p.PeriodYear).ThenByDescending(p => p.PeriodMonth).ToList(),

                ScheduleExceptions = employee.ScheduleExceptions.Select(se => new ScheduleExceptionDto
                {
                    ExceptionId = se.ExceptionId,
                    DateFrom = se.DateFrom,
                    DateTo = se.DateTo,
                    Type = se.Type.ToString(),
                    Reason = se.Reason,
                    IsApproved = se.IsApproved,
                    BlocksCalendar = se.BlocksCalendar
                }).OrderByDescending(se => se.DateFrom).ToList(),

                UpcomingReservations = upcomingReservations.Select(r => new EmployeeReservationDto
                {
                    ReservationId = r.ReservationId,
                    StartTime = r.StartTime,
                    EndTime = r.EndTime,
                    ServiceName = r.ServiceVariant?.Service?.Name ?? "",
                    VariantName = r.ServiceVariant?.Name ?? "",
                    CustomerName = r.Customer != null ? $"{r.Customer.FirstName} {r.Customer.LastName}" : r.GuestName,
                    AgreedPrice = r.AgreedPrice,
                    Status = r.Status.ToString()
                }).ToList(),
                FinanceSettings = employee.FinanceSettings != null ? new FinanceSettingsDto
                {
                    CommissionPercentage = employee.FinanceSettings.CommissionPercentage,
                    HourlyRate = employee.FinanceSettings.HourlyRate,
                    HasPit2Filed = employee.FinanceSettings.HasPit2Filed,
                    UseMiddleClassRelief = employee.FinanceSettings.UseMiddleClassRelief,
                    IsPensionRetired = employee.FinanceSettings.IsPensionRetired,
                    VoluntarySicknessInsurance = employee.FinanceSettings.VoluntarySicknessInsurance,
                    ParticipatesInPPK = employee.FinanceSettings.ParticipatesInPPK,
                    PPKEmployeeRate = employee.FinanceSettings.PPKEmployeeRate,
                    PPKEmployerRate = employee.FinanceSettings.PPKEmployerRate,
                    CommuteType = (int)employee.FinanceSettings.CommuteType
                } : null
            };

            return Ok(employeeDetails);
        }

        [HttpPut("{employeeId}/finance-settings")]
        [Authorize(Roles = "owner")]
        public async Task<IActionResult> UpdateFinanceSettings(int businessId, int employeeId, FinanceSettingsDto dto)
        {
            var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var employee = await _context.Employees
                .Include(e => e.FinanceSettings)
                .FirstOrDefaultAsync(e => e.EmployeeId == employeeId && e.BusinessId == businessId && e.Business.OwnerId == ownerId);

            if (employee == null)
            {
                return NotFound("Pracownik nie został znaleziony lub nie masz do niego dostępu.");
            }

            var activeContract = await _context.EmploymentContracts
                .Where(c => c.EmployeeId == employeeId && c.IsActive)
                .OrderByDescending(c => c.StartDate)
                .FirstOrDefaultAsync();

            var age = 0;
            if (employee.DateOfBirth != DateOnly.MinValue)
            {
                age = DateTime.Now.Year - employee.DateOfBirth.Year;
                if (DateTime.Now.DayOfYear < employee.DateOfBirth.DayOfYear) age--;
            }
            bool isUnder26 = age < 26;

            if (activeContract?.ContractType == ContractType.EmploymentContract)
            {
                dto.VoluntarySicknessInsurance = true;
            }

            if ((employee.FinanceSettings?.IsStudent ?? false) && isUnder26 && activeContract?.ContractType == ContractType.MandateContract)
            {
                dto.VoluntarySicknessInsurance = false;
                dto.IsPensionRetired = false;
                dto.ParticipatesInPPK = false;
            }

            if (age > 70)
            {
                dto.ParticipatesInPPK = false;
            }

            if (employee.FinanceSettings == null)
            {
                employee.FinanceSettings = new EmployeeFinanceSettings { EmployeeId = employeeId };
                _context.EmployeeFinanceSettings.Add(employee.FinanceSettings);
            }

            employee.FinanceSettings.CommissionPercentage = dto.CommissionPercentage;
            employee.FinanceSettings.HourlyRate = dto.HourlyRate;
            employee.FinanceSettings.HasPit2Filed = dto.HasPit2Filed;
            employee.FinanceSettings.UseMiddleClassRelief = dto.UseMiddleClassRelief;
            employee.FinanceSettings.IsPensionRetired = dto.IsPensionRetired;
            employee.FinanceSettings.VoluntarySicknessInsurance = dto.VoluntarySicknessInsurance;
            employee.FinanceSettings.ParticipatesInPPK = dto.ParticipatesInPPK;
            employee.FinanceSettings.PPKEmployeeRate = dto.PPKEmployeeRate;
            employee.FinanceSettings.PPKEmployerRate = dto.PPKEmployerRate;
            employee.FinanceSettings.CommuteType = (WorkCommuteType)dto.CommuteType;

            await _context.SaveChangesAsync();
            return NoContent();
        }


        // POST: api/businesses/1/employees/5/certificates
        [HttpPost("{employeeId}/certificates")]
        [Authorize(Roles = "owner")]
        public async Task<IActionResult> AddCertificate(int businessId, int employeeId, [FromBody] CreateCertificateDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var business = await _context.Businesses.FindAsync(businessId);
            if (business == null || business.OwnerId != userId) return Forbid();

            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.EmployeeId == employeeId && e.BusinessId == businessId);
            if (employee == null) return NotFound("Nie znaleziono pracownika.");

            if (dto.DateObtained > DateOnly.FromDateTime(DateTime.Now))
                return BadRequest(new { message = "Data uzyskania certyfikatu nie może być w przyszłości." });

            var certificate = new EmployeeCertificate
            {
                EmployeeId = employeeId,
                Name = dto.Name,
                Institution = dto.Institution,
                DateObtained = dto.DateObtained,
                IsVisibleToClient = dto.IsVisibleToClient
            };

            _context.EmployeeCertificates.Add(certificate);
            await _context.SaveChangesAsync();

            return Ok(new EmployeeCertificateDto
            {
                CertificateId = certificate.CertificateId,
                Name = certificate.Name,
                Institution = certificate.Institution,
                DateObtained = certificate.DateObtained,
                IsVisibleToClient = certificate.IsVisibleToClient
            });
        }

        // DELETE: api/businesses/1/employees/5/certificates/10
        [HttpDelete("{employeeId}/certificates/{certId}")]
        [Authorize(Roles = "owner")]
        public async Task<IActionResult> DeleteCertificate(int businessId, int employeeId, int certId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var business = await _context.Businesses.FindAsync(businessId);
            if (business == null || business.OwnerId != userId) return Forbid();

            var certificate = await _context.EmployeeCertificates
                .FirstOrDefaultAsync(c => c.CertificateId == certId && c.EmployeeId == employeeId);
            if (certificate == null) return NotFound("Nie znaleziono certyfikatu.");

            _context.EmployeeCertificates.Remove(certificate);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Certyfikat został usunięty." });
        }


        // POST: api/businesses/1/employees/5/absences
        [HttpPost("{employeeId}/absences")]
        [Authorize(Roles = "owner")]
        public async Task<IActionResult> AddAbsence(int businessId, int employeeId, [FromBody] CreateAbsenceDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var business = await _context.Businesses.FindAsync(businessId);
            if (business == null || business.OwnerId != userId) return Forbid();

            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.EmployeeId == employeeId && e.BusinessId == businessId);
            if (employee == null) return NotFound("Nie znaleziono pracownika.");

            if (!Enum.TryParse<AbsenceType>(dto.Type, out var absenceType))
                return BadRequest("Nieprawidłowy typ nieobecności.");

            var hasOverlap = await _context.ScheduleExceptions
                .AnyAsync(se => se.EmployeeId == employeeId
                    && se.DateFrom <= dto.DateTo
                    && se.DateTo >= dto.DateFrom);

            if (hasOverlap)
                return BadRequest(new { message = "W wybranym okresie istnieje już inna nieobecność." });

            var absence = new ScheduleException
            {
                EmployeeId = employeeId,
                DateFrom = dto.DateFrom,
                DateTo = dto.DateTo,
                Type = absenceType,
                Reason = dto.Reason,
                BlocksCalendar = dto.BlocksCalendar,
                IsApproved = true
            };

            _context.ScheduleExceptions.Add(absence);

            int cancelledCount = 0;
            if (dto.BlocksCalendar)
            {
                var absenceStart = dto.DateFrom.ToDateTime(TimeOnly.MinValue);
                var absenceEnd = dto.DateTo.ToDateTime(TimeOnly.MaxValue);

                var overlapping = await _context.Reservations
                    .Where(r => r.EmployeeId == employeeId
                        && r.Status == ReservationStatus.Confirmed
                        && r.StartTime < absenceEnd
                        && r.EndTime > absenceStart)
                    .ToListAsync();

                foreach (var res in overlapping)
                {
                    res.Status = ReservationStatus.Cancelled;
                }
                cancelledCount = overlapping.Count;
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                ExceptionId = absence.ExceptionId,
                DateFrom = absence.DateFrom,
                DateTo = absence.DateTo,
                Type = absence.Type.ToString(),
                Reason = absence.Reason,
                IsApproved = absence.IsApproved,
                BlocksCalendar = absence.BlocksCalendar,
                CancelledReservations = cancelledCount
            });
        }

        // PATCH: api/businesses/1/employees/5/absences/10/approve
        [HttpPatch("{employeeId}/absences/{absenceId}/approve")]
        [Authorize(Roles = "owner")]
        public async Task<IActionResult> ToggleAbsenceApproval(int businessId, int employeeId, int absenceId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var business = await _context.Businesses.FindAsync(businessId);
            if (business == null || business.OwnerId != userId) return Forbid();

            var absence = await _context.ScheduleExceptions
                .FirstOrDefaultAsync(se => se.ExceptionId == absenceId && se.EmployeeId == employeeId);
            if (absence == null) return NotFound("Nie znaleziono nieobecności.");

            absence.IsApproved = !absence.IsApproved;
            await _context.SaveChangesAsync();

            return Ok(new { Message = absence.IsApproved ? "Nieobecność zatwierdzona." : "Zatwierdzenie cofnięte.", IsApproved = absence.IsApproved });
        }

        // DELETE: api/businesses/1/employees/5/absences/10
        [HttpDelete("{employeeId}/absences/{absenceId}")]
        [Authorize(Roles = "owner")]
        public async Task<IActionResult> DeleteAbsence(int businessId, int employeeId, int absenceId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var business = await _context.Businesses.FindAsync(businessId);
            if (business == null || business.OwnerId != userId) return Forbid();

            var absence = await _context.ScheduleExceptions
                .FirstOrDefaultAsync(se => se.ExceptionId == absenceId && se.EmployeeId == employeeId);
            if (absence == null) return NotFound("Nie znaleziono nieobecności.");

            _context.ScheduleExceptions.Remove(absence);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Nieobecność została usunięta." });
        }
    }
}