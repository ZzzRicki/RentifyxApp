using Microsoft.AspNetCore.Http;
using Rentifyx.DAL.Enums.Vehicle;

namespace Rentifyx.BLL.Dto.Vehicle
{
    public class UpdateVehicleDto
    {
        public int Id { get; set; }
        public string? Brand { get; set; }
        public string? Model { get; set; }
        public int Year { get; set; }
        public TransmissionType TransmissionType { get; set; }
        public int PassengerCapacity { get; set; }
        public decimal DailyPrice { get; set; }
        public VehicleStatus Status { get; set; }
        public string? Location { get; set; }
        public ICollection<IFormFile>? Images { get; set; }
        public ICollection<string> Characteristics { get; set; } = [];
    }
}
