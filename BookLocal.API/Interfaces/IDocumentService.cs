using BookLocal.API.DTOs;
using System.Security.Claims;

namespace BookLocal.API.Interfaces
{
    public interface IDocumentService
    {
        Task<(bool Success, string? Path, string? ErrorMessage)> UploadTemplateAsync(UploadTemplateDto dto, ClaimsPrincipal user);
        Task<(bool Success, byte[]? FileBytes, string? FileName, string? ContentType, string? ErrorMessage)> GenerateContractAsync(int employeeId, ClaimsPrincipal user);
    }
}
