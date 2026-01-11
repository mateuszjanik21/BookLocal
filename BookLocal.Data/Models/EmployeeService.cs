using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookLocal.Data.Models
{
    [PrimaryKey("EmployeeId", "ServiceId")]
    public class EmployeeService
    {
        public int EmployeeId { get; set; }
        public int ServiceId { get; set; }

        [ForeignKey("EmployeeId")]
        public virtual Employee Employee { get; set; }

        [ForeignKey("ServiceId")]
        public virtual Service Service { get; set; }
    }
}