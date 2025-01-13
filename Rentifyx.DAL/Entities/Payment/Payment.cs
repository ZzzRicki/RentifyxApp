using Rentifyx.DAL.Core;
using Rentifyx.DAL.Enums.Payments;
using Rentifyx.DAL.Entities;

namespace Rentifyx.DAL.Entities
{
    public class Payment : BaseEntity
    {
        public int ReservationId { get; set; } 
        public Reservation? Reservation { get; set; } 

        public decimal Amount { get; set; } 
        public string? StripePaymentIntentId { get; set; } 
        public PaymentStatus PaymentStatus { get; set; } 
        public DateTime PaymentDate { get; set; } 
    }
}
