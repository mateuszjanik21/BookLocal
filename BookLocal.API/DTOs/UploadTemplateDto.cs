namespace BookLocal.API.DTOs
{
    public class UploadTemplateDto
    {
        public required IFormFile File { get; set; }
        public required string TemplateName { get; set; }
    }
}
