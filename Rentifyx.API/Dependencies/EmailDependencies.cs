using Rentifyx.BLL.Contract;

namespace Rentifyx.API.Dependencies
{
    public static class EmailDependencies
    {
        public static void AddEmailDependencies(this WebApplicationBuilder builder)
        {
            string acsConnectionString = builder.Configuration["AzureCommunicationServices:ConnectionString"]!;

            builder.Services.AddScoped<IEmailService>(provider => new EmailService(acsConnectionString));

        }
    }
}
