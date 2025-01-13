namespace Rentifyx.BLL.Dto.Payment
{
    public class CreatePaymentIntentRequest
    {
        public int ReservationId { get; set; } 
        public decimal Amount { get; set; }   
    }
}
