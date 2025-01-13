using Rentifyx.BLL.Core;
using Rentifyx.BLL.Dto.Car;
using Rentifyx.BLL.Dto.Vehicle;
using Rentifyx.DAL.Enums.Vehicle;

namespace Rentifyx.BLL.Contract
{
    public interface IVehicleService
    {
        Task<ServiceResult<VehicleDto>> RegisterVehicle(RegisterVehicleDto registerCarDto);
        Task<ServiceResult<VehicleDto>> UpdateVehicle(UpdateVehicleDto updateVehicleDto);
        Task<ServiceResult<string>> DeleteVehicle(int id);
        Task<ServiceResult<List<VehicleDto>>> GetAvailableVehicles(string? brand = null,
            string? model = null,
            int? year = null,
            TransmissionType? transmissionType = null,
            int? passengerCapacity = null,
            decimal? dailyPrice = null,
            string? location = null,
            DateTime ? pickupDate = null,
            DateTime? returnDate = null);
        Task<ServiceResult<List<VehicleDto>>> GetAllVehicles();
    }
}
