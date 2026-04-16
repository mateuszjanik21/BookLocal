using BookLocal.API.DTOs;
using BookLocal.API.Interfaces;
using BookLocal.API.Services;
using BookLocal.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BookLocal.API.Services
{
    public class DocumentService : IDocumentService
    {
        private readonly IWordTemplateService _wordTemplateService;
        private readonly IWebHostEnvironment _env;
        private readonly AppDbContext _context;

        public DocumentService(IWordTemplateService wordTemplateService, IWebHostEnvironment env, AppDbContext context)
        {
            _wordTemplateService = wordTemplateService;
            _env = env;
            _context = context;
        }

        public async Task<(bool Success, string? Path, string? ErrorMessage)> UploadTemplateAsync(UploadTemplateDto dto, ClaimsPrincipal user)
        {
            if (dto.File == null || dto.File.Length == 0)
                return (false, null, "No file uploaded.");

            if (!dto.File.FileName.EndsWith(".docx"))
                return (false, null, "Only .docx files are supported.");

            var ownerId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            var business = await _context.Businesses.FirstOrDefaultAsync(b => b.OwnerId == ownerId);

            if (business == null)
                return (false, null, "Nie znaleziono firmy dla tego użytkownika.");

            var templatesDir = Path.Combine(_env.WebRootPath, "templates", business.BusinessId.ToString());
            if (!Directory.Exists(templatesDir))
            {
                Directory.CreateDirectory(templatesDir);
            }

            var filePath = Path.Combine(templatesDir, $"{dto.TemplateName}.docx");

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await dto.File.CopyToAsync(stream);
            }

            return (true, filePath, null);
        }

        public async Task<(bool Success, byte[]? FileBytes, string? FileName, string? ContentType, string? ErrorMessage)> GenerateContractAsync(int employeeId, ClaimsPrincipal user)
        {
            var ownerId = user.FindFirstValue(ClaimTypes.NameIdentifier);

            var employee = await _context.Employees
                .AsNoTracking()
                .Include(e => e.Contracts)
                .Include(e => e.FinanceSettings)
                .Include(e => e.EmployeeDetails)
                .FirstOrDefaultAsync(e => e.EmployeeId == employeeId);

            if (employee == null)
                return (false, null, null, null, "Employee not found");

            var business = await _context.Businesses
                .AsNoTracking()
                .Include(b => b.Owner)
                .FirstOrDefaultAsync(b => b.BusinessId == employee.BusinessId && b.OwnerId == ownerId);

            if (business == null)
                return (false, null, null, null, "Nie masz dostępu do tej firmy lub firma nie istnieje");

            var templatePath = Path.Combine(_env.WebRootPath, "templates", business.BusinessId.ToString(), "Contract.docx");

            if (!System.IO.File.Exists(templatePath))
            {
                return (false, null, null, null, "Nie znaleziono szablonu umowy. Wgraj plik 'Contract.docx' w zakładce Szablony.");
            }

            var contract = employee.Contracts.OrderByDescending(c => c.StartDate).FirstOrDefault();

            string contractTypePL = "Nieokreślono";
            if (contract != null)
            {
                contractTypePL = contract.ContractType switch
                {
                    ContractType.EmploymentContract => "Umowa o Pracę",
                    ContractType.MandateContract => "Umowa Zlecenie",
                    ContractType.B2B => "Kontrakt B2B",
                    ContractType.Apprenticeship => "Praktyki",
                    _ => contract.ContractType.ToString()
                };
            }

            var replacements = new Dictionary<string, string>
            {
                { "{{Imie}}", employee.FirstName },
                { "{{Nazwisko}}", employee.LastName },
                { "{{Pracownik}}", $"{employee.FirstName} {employee.LastName}" },
                { "{{Stanowisko}}", employee.Position ?? "Pracownik" },
                { "{{DataUrodzenia}}", employee.DateOfBirth.ToString("dd.MM.yyyy") },
                { "{{DataGenerowania}}", DateTime.UtcNow.ToString("dd.MM.yyyy") },

                { "{{Firma}}", business.Name ?? "BRAK NAZWY FIRMY" },
                { "{{Reprezentant}}", business.Owner != null ? $"{business.Owner.FirstName} {business.Owner.LastName}" : "Właściciel" },
                { "{{DaneFirmy}}", $"{business.Name}, NIP: {business.NIP ?? "-"}, Adres: {business.Address ?? "Brak Adresu"}"  },

                { "{{Wynagrodzenie}}", contract?.BaseSalary.ToString("0.00") ?? "0.00" },
                { "{{TypUmowy}}", contractTypePL },
                { "{{DataRozpoczecia}}", contract?.StartDate.ToString("dd.MM.yyyy") ?? "-" },
                { "{{DataZakonczenia}}", contract?.EndDate.HasValue == true ? contract.EndDate.Value.ToString("dd.MM.yyyy") : "Czas nieokreślony" }
            };

            var fileBytes = _wordTemplateService.GenerateDocument(templatePath, replacements);
            var fileName = $"Umowa_{employee.LastName}_{employee.FirstName}.docx";
            var contentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";

            return (true, fileBytes, fileName, contentType, null);
        }
    }
}
