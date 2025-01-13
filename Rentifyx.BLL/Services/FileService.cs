using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Rentifyx.BLL.Contract;

namespace Rentifyx.BLL.Services
{
    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _env;

        public FileService(IWebHostEnvironment env)
        {
            _env = env;
        }

        public async Task<string> UploadFileAsync(IFormFile file, string subFolder)
        {
            var originalFileName = Path.GetFileNameWithoutExtension(file.FileName);
            var sanitizedFileName = originalFileName.Replace(" ", "-").Replace(",", "").Replace(";", "").ToLower();
            var uniqueFileName = $"{sanitizedFileName}_{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";

            var uploadPath = Path.Combine(_env.ContentRootPath, "uploads", subFolder);

            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            var filePath = Path.Combine(uploadPath, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/uploads/{subFolder}/{uniqueFileName}";
        }
    }
}
