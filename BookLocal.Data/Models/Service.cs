using BookLocal.Data.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Service
{
    [Key]
    public int ServiceId { get; set; }

    [Required]
    public int ServiceCategoryId { get; set; }
    [ForeignKey("ServiceCategoryId")]
    public virtual ServiceCategory ServiceCategory { get; set; }

    [Required]
    public int BusinessId { get; set; }

    [Required]
    [MaxLength(255)]
    public required string Name { get; set; }

    public string? Description { get; set; }

    [Required]
    [Column(TypeName = "decimal(10, 2)")]
    public decimal Price { get; set; }

    [Required]
    public int DurationMinutes { get; set; }

    public virtual Business Business { get; set; }
    public virtual ICollection<EmployeeService> EmployeeServices { get; set; } = new List<EmployeeService>();
}