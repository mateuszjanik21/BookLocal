using BookLocal.API.DTOs;
using BookLocal.API.Interfaces;
using BookLocal.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BookLocal.API.Services
{
    public class EmployeesService : IEmployeesService
    {
        private readonly AppDbContext _context;

        public EmployeesService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<(bool Success, IEnumerable<EmployeeDto>? Data, string? ErrorMessage)> GetEmployeesAsync(int businessId)
        {
            if (!await _context.Businesses.AnyAsync(b => b.BusinessId == businessId))
                return (false, null, "Firma nie istnieje.");

            var employees = await _context.Employees
                .AsNoTracking()
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

            return (true, employees, null);
        }

        public async Task<(bool Success, EmployeeDto? Data, string? ErrorMessage)> AddEmployeeAsync(int businessId, EmployeeUpsertDto dto, ClaimsPrincipal user)
        {
            var business = await _context.Businesses.FindAsync(businessId);
            if (business == null) return (false, null, "Firma nie istnieje.");

            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (business.OwnerId != userId) return (false, null, "Brak uprawnień.");

            var employee = new Employee
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Position = dto.Position,
                BusinessId = businessId,
                DateOfBirth = dto.DateOfBirth
            };

            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

            var details = new EmployeeDetails
            {
                EmployeeId = employee.EmployeeId,
                Bio = dto.Bio,
                Specialization = dto.Specialization,
                InstagramProfileUrl = dto.InstagramProfileUrl,
                PortfolioUrl = dto.PortfolioUrl
            };
            _context.EmployeeDetails.Add(details);

            var finance = new EmployeeFinanceSettings
            {
                EmployeeId = employee.EmployeeId,
                CommuteType = WorkCommuteType.Local,
                HasPit2Filed = true,
                IsStudent = dto.IsStudent
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

            return (true, employeeToReturn, null);
        }

        public async Task<(bool Success, string? Message, string? ErrorMessage)> AssignServicesToEmployeeAsync(int businessId, int employeeId, AssignServicesDto assignDto, ClaimsPrincipal user)
        {
            var ownerId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            var employee = await _context.Employees
                .Include(e => e.Business)
                .FirstOrDefaultAsync(e => e.EmployeeId == employeeId && e.BusinessId == businessId && e.Business.OwnerId == ownerId);

            if (employee == null) return (false, null, "Brak dostępu lub pracownik nie istnieje.");

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

            return (true, "Usługi pracownika zostały pomyślnie zaktualizowane.", null);
        }

        public async Task<(bool Success, IEnumerable<EmployeeDto>? Data)> GetEmployeesForServiceAsync(int businessId, int serviceId)
        {
            var employees = await _context.EmployeeServices
                .AsNoTracking()
                .Where(es => es.Service.BusinessId == businessId && es.ServiceId == serviceId)
                .Select(es => es.Employee)
                .Where(e => !e.IsArchived)
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

            return (true, employees);
        }

        public async Task<(bool Success, IEnumerable<int>? Data)> GetAssignedServiceIdsForEmployeeAsync(int businessId, int employeeId, ClaimsPrincipal user)
        {
            var ownerId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            var isEmployeeInBusiness = await _context.Employees
                .AsNoTracking()
                .AnyAsync(e => e.EmployeeId == employeeId && e.Business.OwnerId == ownerId && e.BusinessId == businessId);

            if (!isEmployeeInBusiness) return (false, null);

            var serviceIds = await _context.EmployeeServices
                .AsNoTracking()
                .Where(es => es.EmployeeId == employeeId)
                .Select(es => es.ServiceId)
                .ToListAsync();

            return (true, serviceIds);
        }

        public async Task<(bool Success, string? ErrorMessage)> UpdateEmployeeAsync(int businessId, int employeeId, EmployeeUpsertDto dto, ClaimsPrincipal user)
        {
            var ownerId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            var employee = await _context.Employees
                .Include(e => e.EmployeeDetails)
                .Include(e => e.FinanceSettings)
                .FirstOrDefaultAsync(e => e.EmployeeId == employeeId && e.BusinessId == businessId && e.Business.OwnerId == ownerId);

            if (employee == null) return (false, "Pracownik nie został znaleziony lub nie masz do niego dostępu.");

            employee.FirstName = dto.FirstName;
            employee.LastName = dto.LastName;
            employee.Position = dto.Position;
            employee.DateOfBirth = dto.DateOfBirth;

            if (employee.EmployeeDetails == null)
            {
                employee.EmployeeDetails = new EmployeeDetails
                {
                    EmployeeId = employee.EmployeeId
                };
                _context.EmployeeDetails.Add(employee.EmployeeDetails);
            }

            employee.EmployeeDetails.Bio = dto.Bio;
            employee.EmployeeDetails.Specialization = dto.Specialization;
            employee.EmployeeDetails.InstagramProfileUrl = dto.InstagramProfileUrl;
            employee.EmployeeDetails.PortfolioUrl = dto.PortfolioUrl;

            if (employee.FinanceSettings == null)
            {
                employee.FinanceSettings = new EmployeeFinanceSettings
                {
                    EmployeeId = employee.EmployeeId,
                    IsStudent = dto.IsStudent
                };
                _context.EmployeeFinanceSettings.Add(employee.FinanceSettings);
            }
            else
            {
                employee.FinanceSettings.IsStudent = dto.IsStudent;
            }

            await _context.SaveChangesAsync();
            return (true, null);
        }

        public async Task<(bool Success, string? Message, string? ErrorMessage)> ArchiveEmployeeAsync(int businessId, int employeeId, ClaimsPrincipal user)
        {
            var ownerId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.EmployeeId == employeeId && e.BusinessId == businessId && e.Business.OwnerId == ownerId);

            if (employee == null) return (false, null, "Pracownik nie został znaleziony lub nie masz do niego dostępu.");

            employee.IsArchived = true;
            var futureReservations = await _context.Reservations
                .Where(r => r.EmployeeId == employeeId && r.StartTime > DateTime.UtcNow && r.Status != ReservationStatus.Cancelled && r.Status != ReservationStatus.Completed)
                .ToListAsync();

            foreach (var res in futureReservations)
            {
                res.Status = ReservationStatus.Cancelled;
            }

            await _context.SaveChangesAsync();
            return (true, $"Pracownik zarchiwizowany. Anulowano {futureReservations.Count} nadchodzących rezerwacji.", null);
        }

        public async Task<(bool Success, EmployeeDetailDto? Data, string? ErrorMessage)> GetEmployeeDetailsAsync(int businessId, int employeeId, ClaimsPrincipal user)
        {
            var ownerId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            var employee = await _context.Employees
                .AsNoTracking()
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
                .FirstOrDefaultAsync(e => e.EmployeeId == employeeId && e.BusinessId == businessId && e.Business.OwnerId == ownerId);

            if (employee == null) return (false, null, "Pracownik nie został znaleziony lub nie masz do niego dostępu.");

            var completedReservations = await _context.Reservations
                .AsNoTracking()
                .Where(r => r.EmployeeId == employeeId && r.Status == ReservationStatus.Completed)
                .ToListAsync();

            var estimatedRevenue = completedReservations.Sum(r => r.AgreedPrice);

            var certificates = await _context.EmployeeCertificates
                .AsNoTracking()
                .Where(c => c.EmployeeId == employeeId)
                .ToListAsync();

            var now = DateTime.UtcNow;
            var weekEnd = now.AddDays(7);
            var upcomingReservations = await _context.Reservations
                .AsNoTracking()
                .Include(r => r.ServiceVariant)
                    .ThenInclude(sv => sv.Service)
                .Include(r => r.Customer)
                .Where(r => r.EmployeeId == employeeId
                    && r.Status == ReservationStatus.Confirmed
                    && r.StartTime >= now
                    && r.StartTime <= weekEnd)
                .OrderBy(r => r.StartTime)
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
                    CustomerName = r.Customer != null ? $"{r.Customer.FirstName} {r.Customer.LastName}" : r.GuestName ?? "",
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

            return (true, employeeDetails, null);
        }

        public async Task<(bool Success, string? ErrorMessage)> UpdateFinanceSettingsAsync(int businessId, int employeeId, FinanceSettingsDto dto, ClaimsPrincipal user)
        {
            var ownerId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            var employee = await _context.Employees
                .Include(e => e.FinanceSettings)
                .FirstOrDefaultAsync(e => e.EmployeeId == employeeId && e.BusinessId == businessId && e.Business.OwnerId == ownerId);

            if (employee == null) return (false, "Pracownik nie został znaleziony lub nie masz do niego dostępu.");

            var activeContract = await _context.EmploymentContracts
                .Where(c => c.EmployeeId == employeeId && c.IsActive)
                .OrderByDescending(c => c.StartDate)
                .FirstOrDefaultAsync();

            var age = 0;
            if (employee.DateOfBirth != DateOnly.MinValue)
            {
                var today = DateTime.UtcNow;
                age = today.Year - employee.DateOfBirth.Year;
                if (today.Month < employee.DateOfBirth.Month || (today.Month == employee.DateOfBirth.Month && today.Day < employee.DateOfBirth.Day))
                {
                    age--;
                }
            }
            bool isUnder26 = age < 26;

            if (activeContract?.ContractType == ContractType.EmploymentContract)
            {
                dto.VoluntarySicknessInsurance = true;
            }

            if (dto.IsStudent && isUnder26 && activeContract?.ContractType == ContractType.MandateContract)
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
            employee.FinanceSettings.IsStudent = dto.IsStudent;
            employee.FinanceSettings.HasPit2Filed = dto.HasPit2Filed;
            employee.FinanceSettings.UseMiddleClassRelief = dto.UseMiddleClassRelief;
            employee.FinanceSettings.IsPensionRetired = dto.IsPensionRetired;
            employee.FinanceSettings.VoluntarySicknessInsurance = dto.VoluntarySicknessInsurance;
            employee.FinanceSettings.ParticipatesInPPK = dto.ParticipatesInPPK;
            employee.FinanceSettings.PPKEmployeeRate = dto.PPKEmployeeRate;
            employee.FinanceSettings.PPKEmployerRate = dto.PPKEmployerRate;
            employee.FinanceSettings.CommuteType = (WorkCommuteType)dto.CommuteType;

            await _context.SaveChangesAsync();
            return (true, null);
        }

        public async Task<(bool Success, EmployeeCertificateDto? Data, string? ErrorMessage)> AddCertificateAsync(int businessId, int employeeId, CreateCertificateDto dto, ClaimsPrincipal user)
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            var business = await _context.Businesses.FindAsync(businessId);
            if (business == null || business.OwnerId != userId) return (false, null, "Brak uprawnień.");

            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.EmployeeId == employeeId && e.BusinessId == businessId);
            if (employee == null) return (false, null, "Nie znaleziono pracownika.");

            if (dto.DateObtained > DateOnly.FromDateTime(DateTime.UtcNow))
                return (false, null, "Data uzyskania certyfikatu nie może być w przyszłości.");

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

            return (true, new EmployeeCertificateDto
            {
                CertificateId = certificate.CertificateId,
                Name = certificate.Name,
                Institution = certificate.Institution,
                DateObtained = certificate.DateObtained,
                IsVisibleToClient = certificate.IsVisibleToClient
            }, null);
        }

        public async Task<(bool Success, string? Message, string? ErrorMessage)> DeleteCertificateAsync(int businessId, int employeeId, int certId, ClaimsPrincipal user)
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            var business = await _context.Businesses.FindAsync(businessId);
            if (business == null || business.OwnerId != userId) return (false, null, "Brak uprawnień");

            var certificate = await _context.EmployeeCertificates
                .FirstOrDefaultAsync(c => c.CertificateId == certId && c.EmployeeId == employeeId);
            if (certificate == null) return (false, null, "Nie znaleziono certyfikatu.");

            _context.EmployeeCertificates.Remove(certificate);
            await _context.SaveChangesAsync();

            return (true, "Certyfikat został usunięty.", null);
        }

        public async Task<(bool Success, object? Data, string? ErrorMessage)> AddAbsenceAsync(int businessId, int employeeId, CreateAbsenceDto dto, ClaimsPrincipal user)
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            var business = await _context.Businesses.FindAsync(businessId);
            if (business == null || business.OwnerId != userId) return (false, null, "Brak uprawnień");

            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.EmployeeId == employeeId && e.BusinessId == businessId);
            if (employee == null) return (false, null, "Nie znaleziono pracownika.");

            if (!Enum.TryParse<AbsenceType>(dto.Type, out var absenceType))
                return (false, null, "Nieprawidłowy typ nieobecności.");

            var hasOverlap = await _context.ScheduleExceptions
                .AnyAsync(se => se.EmployeeId == employeeId
                    && se.DateFrom <= dto.DateTo
                    && se.DateTo >= dto.DateFrom);

            if (hasOverlap)
                return (false, null, "W wybranym okresie istnieje już inna nieobecność.");

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

            return (true, new
            {
                absence.ExceptionId,
                absence.DateFrom,
                absence.DateTo,
                Type = absence.Type.ToString(),
                absence.Reason,
                absence.IsApproved,
                absence.BlocksCalendar,
                CancelledReservations = cancelledCount
            }, null);
        }

        public async Task<(bool Success, object? Data, string? ErrorMessage)> ToggleAbsenceApprovalAsync(int businessId, int employeeId, int absenceId, ClaimsPrincipal user)
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            var business = await _context.Businesses.FindAsync(businessId);
            if (business == null || business.OwnerId != userId) return (false, null, "Brak uprawnień");

            var absence = await _context.ScheduleExceptions
                .FirstOrDefaultAsync(se => se.ExceptionId == absenceId && se.EmployeeId == employeeId);
            if (absence == null) return (false, null, "Nie znaleziono nieobecności.");

            absence.IsApproved = !absence.IsApproved;
            await _context.SaveChangesAsync();

            return (true, new { Message = absence.IsApproved ? "Nieobecność zatwierdzona." : "Zatwierdzenie cofnięte.", absence.IsApproved }, null);
        }

        public async Task<(bool Success, string? Message, string? ErrorMessage)> DeleteAbsenceAsync(int businessId, int employeeId, int absenceId, ClaimsPrincipal user)
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            var business = await _context.Businesses.FindAsync(businessId);
            if (business == null || business.OwnerId != userId) return (false, null, "Brak uprawnień.");

            var absence = await _context.ScheduleExceptions
                .FirstOrDefaultAsync(se => se.ExceptionId == absenceId && se.EmployeeId == employeeId);
            if (absence == null) return (false, null, "Nie znaleziono nieobecności.");

            _context.ScheduleExceptions.Remove(absence);
            await _context.SaveChangesAsync();

            return (true, "Nieobecność została usunięta.", null);
        }
    }
}
