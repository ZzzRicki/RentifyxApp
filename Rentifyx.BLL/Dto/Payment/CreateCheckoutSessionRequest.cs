namespace Rentifyx.BLL.Dto.Payment
{
    public class CreateCheckoutSessionRequest
    {
        public int ReservationId { get; set; }
        public decimal Amount { get; set; }
        public string? ClientUrl { get; set; }
    }
}
