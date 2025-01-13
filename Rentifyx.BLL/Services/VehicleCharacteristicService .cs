using Microsoft.EntityFrameworkCore;
using Rentifyx.BLL.Contract;
using Rentifyx.DAL.Context;
using Rentifyx.DAL.Entities.Vehicle;

public class VehicleCharacteristicService : IVehicleCharacteristicService
{
    private readonly RentifyxContext _context;

    public VehicleCharacteristicService(RentifyxContext context)
    {
        _context = context;
    }

    public async Task<List<VehicleCharacteristic>> GetCharacteristicsByVehicleIdAsync(int vehicleId)
    {
        return await _context.VehicleCharacteristics
            .Where(vc => vc.VehicleId == vehicleId)
            .ToListAsync();
    }

    public async Task<VehicleCharacteristic> AddCharacteristicAsync(int vehicleId, string name)
    {
        var characteristic = new VehicleCharacteristic
        {
            VehicleId = vehicleId,
            Name = name
        };

        _context.VehicleCharacteristics.Add(characteristic);
        await _context.SaveChangesAsync();

        return characteristic;
    }

    public async Task<VehicleCharacteristic> UpdateCharacteristicAsync(int characteristicId, string name)
    {
        var characteristic = await _context.VehicleCharacteristics.FindAsync(characteristicId);

        if (characteristic == null)
            throw new Exception("Characteristic not found");

        characteristic.Name = name;
        await _context.SaveChangesAsync();

        return characteristic;
    }

    public async Task<bool> DeleteCharacteristicAsync(int characteristicId)
    {
        var characteristic = await _context.VehicleCharacteristics.FindAsync(characteristicId);

        if (characteristic == null)
            return false;

        _context.VehicleCharacteristics.Remove(characteristic);
        await _context.SaveChangesAsync();

        return true;
    }
}
