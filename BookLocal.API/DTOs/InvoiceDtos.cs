using BookLocal.Data.Models;
using System;
using System.Collections.Generic;

namespace BookLocal.API.DTOs
{
    public class InvoiceDto
    {
        public int InvoiceId { get; set; }
        public string InvoiceNumber { get; set; }
        public DateTime IssueDate { get; set; }
        public DateTime SaleDate { get; set; }
        public string CustomerName { get; set; }
        public string CustomerNip { get; set; }
        public decimal TotalGross { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public List<InvoiceItemDto> Items { get; set; }
    }

    public class InvoiceItemDto
    {
        public string Name { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPriceNet { get; set; }
        public decimal VatRate { get; set; }
        public decimal NetValue { get; set; }
        public decimal GrossValue { get; set; }
    }

    public class CreateReservationInvoiceDto
    {
        public int ReservationId { get; set; }
        public string? CustomerNip { get; set; }
    }
}
