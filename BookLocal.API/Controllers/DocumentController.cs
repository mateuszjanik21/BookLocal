using BookLocal.API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookLocal.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentController : ControllerBase
    {
        private readonly IDocumentService _documentService;

        public DocumentController(IDocumentService documentService)
        {
            _documentService = documentService;
        }

        [HttpPost("upload-template")]
        [Authorize(Roles = "owner")]
        public async Task<IActionResult> UploadTemplate([FromForm] BookLocal.API.DTOs.UploadTemplateDto dto)
        {
            var result = await _documentService.UploadTemplateAsync(dto, User);

            if (!result.Success && result.Path == null)
            {
                if (result.ErrorMessage == "Nie znaleziono firmy dla tego użytkownika.") return NotFound(result.ErrorMessage);
                return BadRequest(result.ErrorMessage);
            }

            return Ok(new { Message = "Template uploaded successfully", result.Path });
        }

        [HttpGet("generate-contract/{employeeId}")]
        [Authorize(Roles = "owner")]
        public async Task<IActionResult> GenerateContract(int employeeId)
        {
            var result = await _documentService.GenerateContractAsync(employeeId, User);

            if (!result.Success)
            {
                return NotFound(result.ErrorMessage);
            }

            return File(result.FileBytes!, result.ContentType!, result.FileName!);
        }
    }
}
