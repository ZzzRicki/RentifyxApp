using Rentifyx.BLL.Dto.Car;
using Rentifyx.BLL.Dto.Vehicle;
using Rentifyx.DAL.Entities.Vehicle;
using System.Reflection.PortableExecutable;

public static class VehicleExtension
{
    public static List<VehicleDto> FromVehicleToVehicleDto(this List<Vehicle> vehicles)
    {
        var vehiclesDto = new List<VehicleDto>();

        foreach (var vehicle in vehicles)
        {
            vehiclesDto.Add(new VehicleDto
            {
                Id = vehicle.Id,
                Brand = vehicle.Brand,
                Model = vehicle.Model,
                Year = vehicle.Year,
                TransmissionType = vehicle.TransmissionType,
                PassengerCapacity = vehicle.PassengerCapacity,
                DailyPrice = vehicle.DailyPrice,
                Status = vehicle.Status,
                Location = vehicle.Location,
                ImageUrls = vehicle.ImageUrls,
                Characteristics = vehicle.Characteristics
                    .Select(c => new VehicleCharacteristicDto { Name = c.Name ?? "" })
                    .ToList()
            });
        }

        return vehiclesDto;
    }
}