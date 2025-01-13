using Rentifyx.DAL.Core;
using Rentifyx.DAL.Enums.Reservation;
using Rentifyx.DAL.Entities.Vehicle;

namespace Rentifyx.DAL.Entities
{
    public class Reservation : BaseEntity
    {
        public string? UserId { get; set; }
        public RentifyxUser? User { get; set; } 

        public int VehicleId { get; set; }
        public Vehicle.Vehicle? Vehicle { get; set; } 

        public DateTime PickupDate { get; set; }
        public DateTime ReturnDate { get; set; }

        public ReservationStatus Status { get; set; }
        public decimal TotalCost { get; set; }

        public Payment? Payment { get; set; } 
    }
}
