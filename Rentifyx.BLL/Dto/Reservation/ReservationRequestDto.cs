using Rentifyx.DAL.Enums.Reservation;

namespace Rentifyx.BLL.Dto.Reservation
{
    public class ReservationRequestDto
    {
        public int VehicleId { get; set; }  
        public DateTime PickupDate { get; set; } 
        public DateTime ReturnDate { get; set; } 
    }
}
