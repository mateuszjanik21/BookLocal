using Microsoft.EntityFrameworkCore;

[PrimaryKey("EmployeeId", "ServiceId")]
public class EmployeeService
{
    public int EmployeeId { get; set; }
    public int ServiceId { get; set; }

    public virtual Employee Employee { get; set; }
    public virtual Service Service { get; set; }
}