using Rentifyx.BLL.Contract;
using Rentifyx.BLL.Services;
using Rentifyx.DAL.Interfaces;
using Rentifyx.DAL.Repositories;

namespace Rentifyx.API.Dependencies
{
    public static class RentifyUserDependencies
    {
        public static void AddRentifyxUserDependencies(this WebApplicationBuilder builder)
        {
            builder.Services.AddScoped<IRentifyxUserRepository, RentifyxUserRepository>();
            builder.Services.AddScoped<IRentifyxUserService, RentifyxUserService>();
        }

    }
}
