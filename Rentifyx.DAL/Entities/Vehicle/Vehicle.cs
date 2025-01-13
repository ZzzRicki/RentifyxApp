using Rentifyx.DAL.Core;
using Rentifyx.DAL.Enums.Vehicle;

namespace Rentifyx.DAL.Entities.Vehicle
{
    public class Vehicle : BaseEntity
    {
        public string Brand { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int Year { get; set; }
        public TransmissionType TransmissionType { get; set; }
        public int PassengerCapacity { get; set; }
        public decimal DailyPrice { get; set; }
        public VehicleStatus Status { get; set; }
        public string Location { get; set; } = string.Empty;
        public List<string> ImageUrls { get; set; } = new List<string>();
        public List<VehicleCharacteristic> Characteristics { get; set; } = new List<VehicleCharacteristic>();
    }
}
