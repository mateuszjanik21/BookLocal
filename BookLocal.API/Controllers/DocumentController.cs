using BookLocal.API.Services;
using BookLocal.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BookLocal.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentController : ControllerBase
    {
        private readonly IWordTemplateService _wordTemplateService;
        private readonly IWebHostEnvironment _env;
        private readonly AppDbContext _context;

        public DocumentController(IWordTemplateService wordTemplateService, IWebHostEnvironment env, AppDbContext context)
        {
            _wordTemplateService = wordTemplateService;
            _env = env;
            _context = context;
        }

        [HttpPost("upload-template")]
        [Authorize(Roles = "owner")]
        public async Task<IActionResult> UploadTemplate([FromForm] BookLocal.API.DTOs.UploadTemplateDto dto)
        {
            if (dto.File == null || dto.File.Length == 0)
                return BadRequest("No file uploaded.");

            if (!dto.File.FileName.EndsWith(".docx"))
                return BadRequest("Only .docx files are supported.");

            var ownerId = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);
            var business = await _context.Businesses.FirstOrDefaultAsync(b => b.OwnerId == ownerId);
            if (business == null) return NotFound("Nie znaleziono firmy dla tego użytkownika.");

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

            return Ok(new { Message = "Template uploaded successfully", Path = filePath });
        }

        [HttpGet("generate-contract/{employeeId}")]
        [Authorize(Roles = "owner")]
        public async Task<IActionResult> GenerateContract(int employeeId)
        {
            var employee = await _context.Employees
                .Include(e => e.Contracts)
                .Include(e => e.FinanceSettings)
                .Include(e => e.EmployeeDetails)
                .FirstOrDefaultAsync(e => e.EmployeeId == employeeId);

            if (employee == null)
                return NotFound("Employee not found");

            var business = await _context.Businesses
                .Include(b => b.Owner)
                .FirstOrDefaultAsync(b => b.BusinessId == employee.BusinessId);

            var templatePath = Path.Combine(_env.WebRootPath, "templates", business.BusinessId.ToString(), "Contract.docx");

            if (!System.IO.File.Exists(templatePath))
            {
                return NotFound("Nie znaleziono szablonu umowy. Wgraj plik 'Contract.docx' w zakładce Szablony.");
            }

            var contract = employee.Contracts.OrderByDescending(c => c.StartDate).FirstOrDefault();

            string contractTypePL = "Nieokreślono";
            if (contract != null)
            {
                contractTypePL = contract.ContractType switch
                {
                    BookLocal.Data.Models.ContractType.EmploymentContract => "Umowa o Pracę",
                    BookLocal.Data.Models.ContractType.MandateContract => "Umowa Zlecenie",
                    BookLocal.Data.Models.ContractType.B2B => "Kontrakt B2B",
                    BookLocal.Data.Models.ContractType.Apprenticeship => "Praktyki",
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
                { "{{DataGenerowania}}", DateTime.Now.ToString("dd.MM.yyyy") },

                { "{{Firma}}", business?.Name ?? "BRAK NAZWY FIRMY" },
                { "{{Reprezentant}}", business?.Owner != null ? $"{business.Owner.FirstName} {business.Owner.LastName}" : "Właściciel" },
                { "{{DaneFirmy}}", $"{business?.Name}, NIP: {business?.NIP ?? "-"}, Adres: {business?.Address ?? "Brak Adresu"}"  },

                { "{{Wynagrodzenie}}", contract?.BaseSalary.ToString("0.00") ?? "0.00" },
                { "{{TypUmowy}}", contractTypePL },
                { "{{DataRozpoczecia}}", contract?.StartDate.ToString("dd.MM.yyyy") ?? "-" },
                { "{{DataZakonczenia}}", contract?.EndDate.HasValue == true ? contract.EndDate.Value.ToString("dd.MM.yyyy") : "Czas nieokreślony" }
            };

            var fileBytes = _wordTemplateService.GenerateDocument(templatePath, replacements);

            return File(fileBytes, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", $"Umowa_{employee.LastName}_{employee.FirstName}.docx");
        }
    }
}
