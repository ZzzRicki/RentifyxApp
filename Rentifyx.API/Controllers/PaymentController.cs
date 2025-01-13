using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rentifyx.BLL.Dto.Payment;
using Rentifyx.DAL.Context;
using Rentifyx.DAL.Entities;
using Rentifyx.DAL.Enums.Payments;
using Rentifyx.DAL.Enums.Reservation;
using Stripe;
using Stripe.Checkout;
using Stripe.TestHelpers;
using System;
using System.Security.Claims;

namespace Rentifyx.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly PaymentService _paymentService;
        private readonly RentifyxContext _dbContext;

        public PaymentController(PaymentService paymentService, RentifyxContext dbContext)
        {
            _paymentService = paymentService;
            _dbContext = dbContext;
        }

        [Authorize]
        [HttpPost("create-checkout-session")]
        public async Task<IActionResult> CreateCheckoutSession([FromBody] CreateCheckoutSessionRequest request)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            try
            {
                var reservation = await _dbContext.Reservations
                    .FirstOrDefaultAsync(r => r.Id == request.ReservationId && r.UserId == userId);

                if (reservation == null)
                {
                    return NotFound("Reservation not found.");
                }

                var vehicle = await _dbContext.Vehicles
                    .FirstOrDefaultAsync(v => v.Id == reservation.VehicleId);

                if (vehicle == null)
                {
                    return NotFound("Vehicle not found.");
                }

                // Create a Checkout Session
                var options = new SessionCreateOptions
                {
                    PaymentMethodTypes = new List<string> { "card" },
                    LineItems = new List<SessionLineItemOptions>
            {
                new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(reservation.TotalCost * 100), // Amount in cents
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = $"Reservacion de para {vehicle.Brand} {vehicle.Model} ({vehicle.Year})",
                            Description = $"Desde {reservation.PickupDate.ToString("dd-MM-yyyy")} hasta {reservation.ReturnDate.ToString("dd-MM-yyyy")}"

                        },
                    },
                    Quantity = 1,
                },
            },
                    Mode = "payment",
                    SuccessUrl = $"{request.ClientUrl}/payment-success?session_id={{CHECKOUT_SESSION_ID}}",
                    CancelUrl = $"{request.ClientUrl}/payment-cancel",
                    Metadata = new Dictionary<string, string>
                    {
                        { "reservationId", reservation.Id.ToString() }
                    }
                };

                var service = new SessionService();
                var session = await service.CreateAsync(options);

                return Ok(new { sessionId = session.Id });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating checkout session: {ex.Message}");
                return StatusCode(500, "An error occurred while creating the checkout session.");
            }
        }

        [Authorize]
        [HttpPost("create-payment-intent")]
        public async Task<IActionResult> CreatePaymentIntent([FromBody] CreatePaymentIntentRequest request)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            try
            {
                var reservation = await _dbContext.Reservations
                    .FirstOrDefaultAsync(r => r.Id == request.ReservationId && r.UserId == userId);

                if (reservation == null)
                {
                    return NotFound("Reservation not found.");
                }

                if (reservation.TotalCost != request.Amount)
                {
                    return BadRequest("Payment amount mismatch.");
                }

                // Create a Stripe payment intent
                var paymentIntent = await _paymentService.CreatePaymentIntent(request.Amount);

                // Create payment record in database
                var payment = new Payment
                {
                    ReservationId = request.ReservationId,
                    Amount = request.Amount,
                    StripePaymentIntentId = paymentIntent.Id,
                    PaymentStatus = PaymentStatus.Pending,
                    PaymentDate = DateTime.UtcNow
                };

                _dbContext.Payments.Add(payment);
                await _dbContext.SaveChangesAsync();

                return Ok(new { clientSecret = paymentIntent.ClientSecret });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating payment intent: {ex.Message}");
                return StatusCode(500, "An error occurred while creating the payment intent.");
            }
        }

        [Authorize]
        [HttpPost("confirm-payment")]
        public async Task<IActionResult> ConfirmPayment([FromBody] string paymentIntentId)
        {
            try
            {
                // Confirm the payment intent via Stripe
                var paymentIntent = await _paymentService.ConfirmPaymentIntent(paymentIntentId);

                if (paymentIntent.Status == "succeeded")
                {
                    // Update the payment status in the database
                    var payment = await _dbContext.Payments
                        .FirstOrDefaultAsync(p => p.StripePaymentIntentId == paymentIntentId);

                    if (payment == null)
                    {
                        return NotFound("Payment not found.");
                    }

                    payment.PaymentStatus = PaymentStatus.Paid;
                    await _dbContext.SaveChangesAsync();

                    return Ok(new { status = "Payment successful" });
                }

                return BadRequest(new { status = "Payment failed" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error confirming payment: {ex.Message}");
                return StatusCode(500, "An error occurred while confirming the payment.");
            }
        }

        [Authorize]
        [HttpPost("confirm-payment-session")]
        public async Task<IActionResult> ConfirmPayment([FromBody] ConfirmPaymentRequest request)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            try
            {
                var user = await _dbContext.RentifyxUsers.FirstOrDefaultAsync(x => x.Id == userId);

                if (user is null) return BadRequest();

                var service = new SessionService();
                var session = await service.GetAsync(request.SessionId);

                if (session.PaymentStatus == "paid")
                {
                    // Find the payment by Stripe Payment Intent ID
                    var payment = await _dbContext.Payments
                        .FirstOrDefaultAsync(p => p.StripePaymentIntentId == session.PaymentIntentId);

                    if (payment != null)
                    {
                        // Update the payment status to "Paid"
                        payment.PaymentStatus = PaymentStatus.Paid;
                        await _dbContext.SaveChangesAsync();
                    }
                    else
                    {
                        // Create a new payment record if it does not exist
                        var reservationId = session.Metadata["reservationId"];
                        var amount = (decimal)session.AmountTotal / 100;

                        var newPayment = new Payment
                        {
                            ReservationId = int.Parse(reservationId),
                            Amount = amount,
                            StripePaymentIntentId = session.PaymentIntentId,
                            PaymentStatus = PaymentStatus.Paid,
                            PaymentDate = DateTime.UtcNow,
                            CreationUser = user.Email
                        };

                        _dbContext.Payments.Add(newPayment);
                        await _dbContext.SaveChangesAsync();

                        payment = newPayment;
                    }

                    var reservation = await _dbContext.Reservations
                        .FirstOrDefaultAsync(r => r.Id == payment.ReservationId);

                    if (reservation != null)
                    {
                        reservation.Status = ReservationStatus.CONFIRMED;
                        await _dbContext.SaveChangesAsync();
                    }

                    return Ok(new { status = "Payment successful" });
                }

                return BadRequest(new { status = "Payment failed" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error confirming payment: {ex.Message}");
                return StatusCode(500, "An error occurred while confirming the payment.");
            }
        }

        [Authorize]
        [HttpGet("transactions")]
        public async Task<IActionResult> GetTransactions(int page = 1, int pageSize = 10)
        {
            try
            {
                var paymentIntentService = new PaymentIntentService();
                var chargeService = new ChargeService();
                var refundService = new Stripe.RefundService();

                var options = new PaymentIntentListOptions
                {
                    Limit = pageSize,
                    StartingAfter = page > 1 ? GetStartingAfterId(page, pageSize) : null
                };

                StripeList<PaymentIntent> paymentIntents = await paymentIntentService.ListAsync(options);

                var transactions = new List<object>();

                foreach (var paymentIntent in paymentIntents.Data.OrderByDescending(pi => pi.Created))
                {
                    string email = null;
                    bool isRefunded = false;  

                    if (string.IsNullOrEmpty(paymentIntent.CustomerId) && paymentIntent.LatestChargeId != null)
                    {
                        var charge = await chargeService.GetAsync(paymentIntent.LatestChargeId);
                        email = charge.BillingDetails?.Email;

                        if (charge.AmountRefunded > 0)
                        {
                            isRefunded = true;
                        }
                    }
                    else if (!string.IsNullOrEmpty(paymentIntent.CustomerId))
                    {
                        var customerService = new Stripe.CustomerService();
                        var customer = await customerService.GetAsync(paymentIntent.CustomerId);
                        email = customer?.Email;

                        if (paymentIntent.LatestChargeId != null)
                        {
                            var charge = await chargeService.GetAsync(paymentIntent.LatestChargeId);
                            if (charge.AmountRefunded > 0)
                            {
                                isRefunded = true;
                            }
                        }
                    }

                    transactions.Add(new
                    {
                        PaymentIntentId = paymentIntent.Id,
                        Amount = paymentIntent.Amount,
                        Currency = paymentIntent.Currency,
                        Status = isRefunded ? "refunded" : paymentIntent.Status, 
                        Created = paymentIntent.Created,
                        Customer = new
                        {
                            Email = email
                        },
                        RefundStatus = isRefunded ? "refunded" : "not_refunded"  
                    });
                }

                return Ok(new
                {
                    HasMore = paymentIntents.HasMore,
                    Page = page,
                    PageSize = pageSize,
                    Transactions = transactions
                });
            }
            catch (StripeException ex)
            {
                Console.WriteLine($"Error fetching transactions: {ex.Message}");
                return StatusCode(500, "An error occurred while fetching transactions.");
            }
        }

        private string? GetStartingAfterId(int page, int pageSize)
        {
            return null;
        }

        [Authorize]
        [HttpPost("refund/{paymentIntentId}")]
        public async Task<IActionResult> RefundPayment(string paymentIntentId)
        {
            

            try
            {
                var refundService = new Stripe.RefundService();
                var chargeService = new ChargeService();
                var chargeList = await chargeService.ListAsync(new ChargeListOptions
                {
                    PaymentIntent = paymentIntentId,
                    Limit = 1 
                });

                if (chargeList.Data.Count == 0)
                {
                    return NotFound("No charge associated with this PaymentIntent.");
                }

                var chargeId = chargeList.Data[0].Id;

                var refundOptions = new RefundCreateOptions
                {
                    Charge = chargeId
                };

                var refund = await refundService.CreateAsync(refundOptions);

                return Ok(new
                {
                    Success = true,
                    Message = "Refund successful",
                    RefundId = refund.Id
                });
            }
            catch (StripeException ex)
            {
                Console.WriteLine($"Stripe error: {ex.Message}");
                return StatusCode(500, new { Success = false, Message = "An error occurred while processing the refund." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, new { Success = false, Message = "An error occurred while processing the refund." });
            }
        }

    }
}
