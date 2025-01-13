
using Microsoft.Extensions.Configuration;
using Stripe;

public class PaymentService
{
    private readonly IConfiguration _config;

    public PaymentService(IConfiguration config)
    {
        _config = config;
        StripeConfiguration.ApiKey = _config["Stripe:SecretKey"];
    }

    public async Task<PaymentIntent> CreatePaymentIntent(decimal amount)
    {
        var options = new PaymentIntentCreateOptions
        {
            Amount = (long)(amount * 100),
            Currency = "usd",
            PaymentMethodTypes = new List<string> { "card" }
        };

        var service = new PaymentIntentService();
        return await service.CreateAsync(options);
    }

    public async Task<PaymentIntent> ConfirmPaymentIntent(string paymentIntentId)
    {
        var service = new PaymentIntentService();
        var options = new PaymentIntentConfirmOptions();
        return await service.ConfirmAsync(paymentIntentId, options);
    }
}

