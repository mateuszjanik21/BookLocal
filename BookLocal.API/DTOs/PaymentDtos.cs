using System.ComponentModel.DataAnnotations;
using BookLocal.Data.Models;

namespace BookLocal.API.DTOs
{
    public class PaymentDto
    {
        public int PaymentId { get; set; }
        public int ReservationId { get; set; }
        public int BusinessId { get; set; }
        public string Method { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public DateTime TransactionDate { get; set; }
        public string Status { get; set; }
    }

    public class CreatePaymentDto
    {
        [Required]
        public int ReservationId { get; set; }

        [Required]
        public PaymentMethod Method { get; set; }

        [Required]
        public decimal Amount { get; set; }
    }
}
