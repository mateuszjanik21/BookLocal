using System.ComponentModel.DataAnnotations;
using BookLocal.Data.Models;

namespace BookLocal.API.DTOs
{
    public class DiscountDto
    {
        public int DiscountId { get; set; }
        public string Code { get; set; }
        public string Type { get; set; }
        public decimal Value { get; set; }
        public int? MaxUses { get; set; }
        public int UsedCount { get; set; }
        public DateOnly? ValidFrom { get; set; }
        public DateOnly? ValidTo { get; set; }
        public bool IsActive { get; set; }
        public int? ServiceId { get; set; }
    }

    public class CreateDiscountDto
    {
        [Required]
        [MaxLength(20)]
        public string Code { get; set; }

        [Required]
        public DiscountType Type { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Value { get; set; }

        public int? MaxUses { get; set; }
        public DateOnly? ValidFrom { get; set; }
        public DateOnly? ValidTo { get; set; }
        public int? ServiceId { get; set; }
    }

    public class VerifyDiscountRequest
    {
        public string Code { get; set; }
        public int? ServiceId { get; set; }
        public decimal OriginalPrice { get; set; }
    }

    public class VerifyDiscountResult
    {
        public bool IsValid { get; set; }
        public string? Message { get; set; }
        public int? DiscountId { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal FinalPrice { get; set; }
    }
}
