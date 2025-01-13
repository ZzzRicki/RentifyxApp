using Rentifyx.BLL.Contract;
using Rentifyx.BLL.Services;

namespace Rentifyx.API.Dependencies
{
    public static class AuthDependencies
    {
        public static void AddAuthDependencies(this WebApplicationBuilder builder)
        {
            builder.Services.AddScoped<IAuthService, AuthService>();
        }
    }
}
