using Rentifyx.DAL.Entities.Vehicle;

namespace Rentifyx.BLL.Contract
{
    public interface IVehicleCharacteristicService
    {
        Task<List<VehicleCharacteristic>> GetCharacteristicsByVehicleIdAsync(int vehicleId);
        Task<VehicleCharacteristic> AddCharacteristicAsync(int vehicleId, string name);
        Task<VehicleCharacteristic> UpdateCharacteristicAsync(int characteristicId, string name);
        Task<bool> DeleteCharacteristicAsync(int characteristicId);
    }
}
