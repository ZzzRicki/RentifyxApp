using Microsoft.AspNetCore.Http;

namespace Rentifyx.BLL.Contract
{
    public interface IFileService
    {
        Task<string> UploadFileAsync(IFormFile file, string subFolder);
    }
}
