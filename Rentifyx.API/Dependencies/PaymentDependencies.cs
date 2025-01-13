namespace Rentifyx.API.Dependencies
{
    public static class PaymentDependencies
    {
        public static void AddPaymentDependencies(this WebApplicationBuilder builder)
        {
            builder.Services.AddScoped<PaymentService>();
        }
    }
}
