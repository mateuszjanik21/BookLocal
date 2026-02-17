namespace BookLocal.API.DTOs
{
    public class EmployeeUpsertDto
    {
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Position { get; set; }
        public required DateOnly DateOfBirth { get; set; }
        public string? Bio { get; set; }
        public string? Specialization { get; set; }
        public string? InstagramProfileUrl { get; set; }
        public string? PortfolioUrl { get; set; }
        public bool IsStudent { get; set; }
    }

    public class EmployeeDto
    {
        public int Id { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Position { get; set; }
        public string? PhotoUrl { get; set; }
        public DateOnly DateOfBirth { get; set; }
        public string? Specialization { get; set; }
        public string? InstagramProfileUrl { get; set; }
        public string? PortfolioUrl { get; set; }

        public bool IsStudent { get; set; }
        public bool IsArchived { get; set; }
        public int AssignedServicesCount { get; set; }
        public int CompletedReservationsCount { get; set; }
        public string? ActiveContractType { get; set; }
        public decimal EstimatedMonthlyRevenue { get; set; }
    }

    public class EmployeeDetailDto
    {
        public int Id { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Position { get; set; }
        public string? PhotoUrl { get; set; }
        public DateOnly DateOfBirth { get; set; }
        public bool IsArchived { get; set; }
        public string? Bio { get; set; }
        public string? Specialization { get; set; }
        public string? Hobbies { get; set; }
        public string? InstagramProfileUrl { get; set; }
        public string? PortfolioUrl { get; set; }
        public bool IsStudent { get; set; }
        public decimal EstimatedRevenue { get; set; }
        public int CompletedReservationsCount { get; set; }
        public required ICollection<ServiceDto> AssignedServices { get; set; }
        public required ICollection<WorkScheduleDto> WorkSchedules { get; set; }
        public ICollection<EmployeeCertificateDto> Certificates { get; set; } = new List<EmployeeCertificateDto>();
        public ICollection<EmploymentContractDto> Contracts { get; set; } = new List<EmploymentContractDto>();
        public ICollection<EmployeePayrollDto> Payrolls { get; set; } = new List<EmployeePayrollDto>();
        public ICollection<ScheduleExceptionDto> ScheduleExceptions { get; set; } = new List<ScheduleExceptionDto>();
        public ICollection<EmployeeReservationDto> UpcomingReservations { get; set; } = new List<EmployeeReservationDto>();
        public FinanceSettingsDto? FinanceSettings { get; set; }
    }

    public class EmployeeCertificateDto
    {
        public int CertificateId { get; set; }
        public string Name { get; set; } = "";
        public string? Institution { get; set; }
        public DateOnly DateObtained { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsVisibleToClient { get; set; }
    }

    public class ScheduleExceptionDto
    {
        public int ExceptionId { get; set; }
        public DateOnly DateFrom { get; set; }
        public DateOnly DateTo { get; set; }
        public string Type { get; set; } = "Other";
        public string? Reason { get; set; }
        public bool IsApproved { get; set; }
        public bool BlocksCalendar { get; set; }
    }

    public class CreateCertificateDto
    {
        public required string Name { get; set; }
        public string? Institution { get; set; }
        public DateOnly DateObtained { get; set; }
        public bool IsVisibleToClient { get; set; } = true;
    }

    public class CreateAbsenceDto
    {
        public DateOnly DateFrom { get; set; }
        public DateOnly DateTo { get; set; }
        public string Type { get; set; } = "Other";
        public string? Reason { get; set; }
        public bool BlocksCalendar { get; set; } = true;
    }

    public class EmployeeReservationDto
    {
        public int ReservationId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string ServiceName { get; set; } = "";
        public string VariantName { get; set; } = "";
        public string? CustomerName { get; set; }
        public decimal AgreedPrice { get; set; }
        public string Status { get; set; } = "Confirmed";
    }
}