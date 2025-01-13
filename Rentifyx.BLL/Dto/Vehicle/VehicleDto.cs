using Rentifyx.BLL.Dto.Vehicle;
using Rentifyx.DAL.Enums.Vehicle;

namespace Rentifyx.BLL.Dto.Car
{
    public class VehicleDto
    {
        public int Id { get; set; }
        public string Brand { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int Year { get; set; }
        public TransmissionType TransmissionType { get; set; }
        public int PassengerCapacity { get; set; }
        public decimal DailyPrice { get; set; }
        public VehicleStatus Status { get; set; }
        public string Location { get; set; } = string.Empty;
        public ICollection<string> ImageUrls { get; set; } = [];
        public ICollection<VehicleCharacteristicDto> Characteristics { get; set; } = [];
    }
}
