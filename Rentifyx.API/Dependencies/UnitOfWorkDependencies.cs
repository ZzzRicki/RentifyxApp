using Rentifyx.DAL.Core;

namespace Rentifyx.API.Dependencies
{
    public static class UnitOfWorkDependencies
    {
        public static void AddUnitOfWorkDependencies(this WebApplicationBuilder builder)
        {
            builder.AddFileDependencies();
            builder.AddRentifyxUserDependencies();
            builder.AddCarDependencies();
            builder.AddAuthDependencies();
            builder.AddPaymentDependencies();

            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
        }
    }

}
