using System.ComponentModel.DataAnnotations;

namespace BookLocal.Data.Models
{
    public class PaymentMethod
    {
        [Key]
        public int PaymentMethodId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; }

        public bool IsActive { get; set; } = true;
    }
}