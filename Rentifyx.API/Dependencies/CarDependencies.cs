using Rentifyx.BLL.Contract;
using Rentifyx.BLL.Services;
using Rentifyx.DAL.Interfaces;
using Rentifyx.DAL.Repositories;

namespace Rentifyx.API.Dependencies
{
    public static class CarDependencies
    {
        public static void AddCarDependencies(this WebApplicationBuilder builder)
        {
            builder.Services.AddScoped<IVehicleRepository, VehicleRepository>();
            builder.Services.AddScoped<IVehicleService, VehicleService>();
        }

    }
}
