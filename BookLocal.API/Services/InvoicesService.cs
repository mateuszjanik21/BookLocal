using BookLocal.API.DTOs;
using BookLocal.API.Interfaces;
using BookLocal.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BookLocal.API.Services
{
    public class InvoicesService : IInvoicesService
    {
        private readonly AppDbContext _context;

        public InvoicesService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<(bool Success, InvoiceDto? Data, string? ErrorMessage, int StatusCode)> GenerateInvoiceAsync(int businessId, CreateReservationInvoiceDto dto, ClaimsPrincipal user)
        {
            var ownerId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!await _context.Businesses.AnyAsync(b => b.BusinessId == businessId && b.OwnerId == ownerId))
                return (false, null, "Brak uprawnień.", 403);

            var reservation = await _context.Reservations
                .Include(r => r.ServiceVariant)
                    .ThenInclude(sv => sv.Service)
                .Include(r => r.Customer)
                .FirstOrDefaultAsync(r => r.ReservationId == dto.ReservationId && r.BusinessId == businessId);

            if (reservation == null) return (false, null, "Rezerwacja nie istnieje.", 404);
            if (reservation.Status != ReservationStatus.Completed) return (false, null, "Fakturę można wystawić tylko do zakończonej rezerwacji.", 400);

            var existingInvoice = await _context.Invoices.AnyAsync(i => i.ReservationId == dto.ReservationId);
            if (existingInvoice) return (false, null, "Faktura do tej rezerwacji została już wystawiona.", 409);

            if (string.IsNullOrEmpty(reservation.CustomerId))
            {
                return (false, null, "Faktury są obecnie dostępne tylko dla zarejestrowanych klientów (wymagane konto klienta).", 400);
            }

            using var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
            try
            {
                var now = DateTime.UtcNow;
                var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
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
                    CustomerId = reservation.CustomerId,
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
                            Name = $"{reservation.ServiceVariant.Service.Name} - {reservation.ServiceVariant.Name}",
                            Quantity = 1,
                            UnitPriceNet = netAmount,
                            VatRate = vatRate,
                            NetValue = netAmount,
                            TaxValue = vatAmount,
                            GrossValue = grossAmount
                        }
                    }
                };

                _context.Invoices.Add(invoice);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                var customerName = reservation.Customer != null ? $"{reservation.Customer.FirstName} {reservation.Customer.LastName}" : "";
                return (true, MapToDto(invoice, customerName), null, 200);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return (false, null, "Wystąpił błąd podczas generowania faktury. Spróbuj ponownie.", 500);
            }
        }

        public async Task<(bool Success, object? Data, string? ErrorMessage)> GetInvoicesAsync(int businessId, int page, int pageSize, string? search, string? month, ClaimsPrincipal user)
        {
            var ownerId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!await _context.Businesses.AnyAsync(b => b.BusinessId == businessId && b.OwnerId == ownerId))
                return (false, null, "Brak uprawnień.");

            var query = _context.Invoices
               .AsNoTracking()
               .Include(i => i.Items)
               .Include(i => i.Customer)
               .Where(i => i.BusinessId == businessId);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                query = query.Where(i => i.InvoiceNumber.ToLower().Contains(s)
                    || (i.Customer.FirstName + " " + i.Customer.LastName).ToLower().Contains(s));
            }

            if (!string.IsNullOrWhiteSpace(month) && month.Length == 7)
            {
                if (int.TryParse(month.Substring(0, 4), out int year) && int.TryParse(month.Substring(5, 2), out int mon))
                {
                    var startOfMonth = new DateTime(year, mon, 1);
                    var endOfMonth = startOfMonth.AddMonths(1);
                    query = query.Where(i => i.IssueDate >= startOfMonth && i.IssueDate < endOfMonth);
                }
            }

            var totalCount = await query.CountAsync();
            var totalGrossSum = await query.SumAsync(i => i.TotalGross);

            var invoices = await query
               .OrderByDescending(i => i.IssueDate)
               .Skip((page - 1) * pageSize)
               .Take(pageSize)
               .ToListAsync();

            var dtos = invoices.Select(i => MapToDto(i, i.Customer != null ? $"{i.Customer.FirstName} {i.Customer.LastName}" : "")).ToList();

            var result = new
            {
                Items = dtos,
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                TotalGrossSum = totalGrossSum
            };

            return (true, result, null);
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
                TotalNet = i.TotalNet,
                TotalTax = i.TotalTax,
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
