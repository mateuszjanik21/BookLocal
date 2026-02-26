namespace BookLocal.API.DTOs
{
    public class FinanceSettingsDto
    {
        public decimal? CommissionPercentage { get; set; }
        public decimal HourlyRate { get; set; }
        public bool IsStudent { get; set; }
        public bool HasPit2Filed { get; set; }
        public bool UseMiddleClassRelief { get; set; }
        public bool IsPensionRetired { get; set; }
        public bool VoluntarySicknessInsurance { get; set; }
        public bool ParticipatesInPPK { get; set; }
        public double PPKEmployeeRate { get; set; }
        public double PPKEmployerRate { get; set; }
        public int CommuteType { get; set; }
    }
}
