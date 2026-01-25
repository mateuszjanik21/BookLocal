using BookLocal.API.DTOs;
using BookLocal.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BookLocal.API.Controllers
{
    [ApiController]
    [Route("api/businesses/{businessId}/invoices")]
    [Authorize(Roles = "owner")]
    public class InvoicesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public InvoicesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("generate")]
        public async Task<ActionResult<InvoiceDto>> GenerateInvoice(int businessId, [FromBody] CreateReservationInvoiceDto dto)
        {
            var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!await _context.Businesses.AnyAsync(b => b.BusinessId == businessId && b.OwnerId == ownerId))
                return Forbid();

            var reservation = await _context.Reservations
                .Include(r => r.ServiceVariant)
                .Include(r => r.Customer)
                .FirstOrDefaultAsync(r => r.ReservationId == dto.ReservationId && r.BusinessId == businessId);

            if (reservation == null) return NotFound("Rezerwacja nie istnieje.");
            if (reservation.Status != ReservationStatus.Completed) return BadRequest("Fakturę można wystawić tylko do zakończonej rezerwacji.");

            var existingInvoice = await _context.Invoices.AnyAsync(i => i.ReservationId == dto.ReservationId);
            if (existingInvoice) return Conflict("Faktura do tej rezerwacji została już wystawiona.");

            var now = DateTime.Now;
            var startOfMonth = new DateTime(now.Year, now.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddTicks(-1);

            var countThisMonth = await _context.Invoices
                .CountAsync(i => i.BusinessId == businessId && i.IssueDate >= startOfMonth && i.IssueDate <= endOfMonth);

            var seqNumber = countThisMonth + 1;
            var invoiceNumber = $"FV/{now.Year}/{now.Month:D2}/{seqNumber:D3}";

            while (await _context.Invoices.AnyAsync(i => i.BusinessId == businessId && i.InvoiceNumber == invoiceNumber))
            {
                seqNumber++;
                invoiceNumber = $"FV/{now.Year}/{now.Month:D2}/{seqNumber:D3}";
            }

            decimal vatRate = 0.23m;
            decimal grossAmount = reservation.AgreedPrice;

            decimal netAmount = Math.Round(grossAmount / (1 + vatRate), 2);
            decimal vatAmount = grossAmount - netAmount;

            var invoice = new Invoice
            {
                BusinessId = businessId,
                ReservationId = reservation.ReservationId,
                CustomerId = reservation.CustomerId ?? "",

                InvoiceNumber = invoiceNumber,
                IssueDate = now,
                SaleDate = reservation.EndTime,
                PaymentMethod = reservation.PaymentMethod,
                TotalNet = netAmount,
                TotalTax = vatAmount,
                TotalGross = grossAmount,
                Items = new List<InvoiceItem>
                {
                    new InvoiceItem
                    {
                        Name = reservation.ServiceVariant.Name,
                        Quantity = 1,
                        UnitPriceNet = netAmount,
                        VatRate = vatRate,
                        NetValue = netAmount,
                        TaxValue = vatAmount,
                        GrossValue = grossAmount
                    }
                }
            };

            if (string.IsNullOrEmpty(reservation.CustomerId))
            {
                return BadRequest("Faktury są obecnie dostępne tylko dla zarejestrowanych klientów (wymagane konto klienta).");
            }
            invoice.CustomerId = reservation.CustomerId;

            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync();

            return Ok(MapToDto(invoice, reservation.Customer?.FirstName + " " + reservation.Customer?.LastName));
        }

        [HttpGet]
        public async Task<ActionResult<PagedResultDto<InvoiceDto>>> GetInvoices(
            int businessId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!await _context.Businesses.AnyAsync(b => b.BusinessId == businessId && b.OwnerId == ownerId))
                return Forbid();

            var query = _context.Invoices
               .Include(i => i.Items)
               .Include(i => i.Customer)
               .Where(i => i.BusinessId == businessId);

            var totalCount = await query.CountAsync();

            var invoices = await query
               .OrderByDescending(i => i.IssueDate)
               .Skip((page - 1) * pageSize)
               .Take(pageSize)
               .ToListAsync();

            var dtos = invoices.Select(i => MapToDto(i, i.Customer.FirstName + " " + i.Customer.LastName)).ToList();

            return Ok(new PagedResultDto<InvoiceDto>
            {
                Items = dtos,
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = pageSize
            });
        }

        private InvoiceDto MapToDto(Invoice i, string customerName)
        {
            return new InvoiceDto
            {
                InvoiceId = i.InvoiceId,
                InvoiceNumber = i.InvoiceNumber,
                IssueDate = i.IssueDate,
                SaleDate = i.SaleDate,
                CustomerName = customerName,
                TotalGross = i.TotalGross,
                PaymentMethod = i.PaymentMethod,
                Items = i.Items.Select(item => new InvoiceItemDto
                {
                    Name = item.Name,
                    Quantity = item.Quantity,
                    UnitPriceNet = item.UnitPriceNet,
                    VatRate = item.VatRate,
                    NetValue = item.NetValue,
                    GrossValue = item.GrossValue
                }).ToList()
            };
        }
    }
}
