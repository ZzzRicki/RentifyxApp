using Microsoft.AspNetCore.Http;
using Rentifyx.DAL.Enums.Vehicle;

namespace Rentifyx.BLL.Dto.Car
{
    public class RegisterVehicleDto
    {
        public string Brand { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int Year { get; set; }
        public TransmissionType TransmissionType { get; set; }
        public int PassengerCapacity { get; set; }
        public decimal DailyPrice { get; set; }
        public VehicleStatus Status { get; set; }
        public string Location { get; set; } = string.Empty;
        public ICollection<IFormFile> Images { get; set; } = [];
        public ICollection<string> Characteristics { get; set; } = [];
    }
}
