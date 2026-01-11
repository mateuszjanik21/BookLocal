using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookLocal.Data.Models
{
    public enum WorkCommuteType
    {
        Local,
        Commuting
    }

    public class EmployeeFinanceSettings
    {
        [Key]
        public int SettingId { get; set; }

        [Required]
        public int EmployeeId { get; set; }
        [ForeignKey("EmployeeId")]
        public virtual Employee Employee { get; set; }

        public bool IsStudent { get; set; } = false;

        // --- (PIT) ---

        public bool HasPit2Filed { get; set; } = true;
        public WorkCommuteType CommuteType { get; set; } = WorkCommuteType.Local;
        public bool UseMiddleClassRelief { get; set; } = false;
        [Column(TypeName = "decimal(18,2)")]
        public decimal HourlyRate { get; set; }

        // --- ZUS ---
        public bool IsPensionRetired { get; set; } = false;
        public bool VoluntarySicknessInsurance { get; set; } = true;

        // --- PPK ---
        public bool ParticipatesInPPK { get; set; } = false;

        [Range(0.5, 4.0)]
        public double PPKEmployeeRate { get; set; } = 2.0;

        [Range(1.5, 4.0)]
        public double PPKEmployerRate { get; set; } = 1.5;
    }
}