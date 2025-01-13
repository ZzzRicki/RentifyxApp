using Rentifyx.BLL.Contract;
using Rentifyx.BLL.Services;

namespace Rentifyx.API.Dependencies
{
    public static class FileDependencies
    {
        public static void AddFileDependencies(this WebApplicationBuilder builder)
        {
            builder.Services.AddScoped<IFileService, FileService>();
        }

    }
}
