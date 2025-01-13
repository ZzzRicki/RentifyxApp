using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
public class FileController : ControllerBase
{
    private readonly IWebHostEnvironment _env;

    public FileController(IWebHostEnvironment env)
    {
        _env = env;
    }

    [HttpGet("uploads/{subFolder}/{filename}")]
    public IActionResult GetImageFile(string subFolder, string filename)
    {
        var fullFilePath = Path.Combine(_env.ContentRootPath, "uploads", subFolder, filename);

        if (!System.IO.File.Exists(fullFilePath))
        {
            return NotFound();
        }

        var contentType = GetImageContentType(fullFilePath);
        if (contentType == null)
        {
            return StatusCode(StatusCodes.Status415UnsupportedMediaType, "File type not supported. Only images are allowed.");
        }

        var fileStream = new FileStream(fullFilePath, FileMode.Open, FileAccess.Read);
        return File(fileStream, contentType);
    }

    private string? GetImageContentType(string path)
    {
        var extension = Path.GetExtension(path).ToLowerInvariant();
        return extension switch
        {
            ".jpg" => "image/jpeg",
            ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            _ => null,
        };
    }
}