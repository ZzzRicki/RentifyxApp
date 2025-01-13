using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Rentifyx.BLL.Contract;
using Rentifyx.BLL.Core;
using Rentifyx.BLL.Dto.Car;
using Rentifyx.BLL.Dto.Vehicle;
using Rentifyx.DAL.Context;
using Rentifyx.DAL.Core;
using Rentifyx.DAL.Entities.Vehicle;
using Rentifyx.DAL.Enums.Vehicle;
using Rentifyx.DAL.Enums.Reservation;

namespace Rentifyx.BLL.Services
{
    public class VehicleService : IVehicleService
    {
        private readonly ILogger _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFileService _fileService;
        private readonly RentifyxContext _context;

        public VehicleService(RentifyxContext rentifyxContext, IUnitOfWork unitOfWork, ILogger<VehicleService> logger, IFileService fileService)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _fileService = fileService;
            _context = rentifyxContext;
        }

        public async Task<ServiceResult<List<VehicleDto>>> GetAllVehicles()
        {
            var result = new ServiceResult<List<VehicleDto>>();

            try
            {
                var carsFromDB = await _context.Vehicles!
                    .Include(x => x.Characteristics)
                    .ToListAsync();
                var cars = carsFromDB.FromVehicleToVehicleDto();
                result.Data = cars;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = "Error getting available cars.";
                _logger.LogError(ex, result.Message);
            }

            return result;
        }

        public async Task<ServiceResult<List<VehicleDto>>> GetAvailableVehicles(
            string? brand = null,
            string? model = null,
            int? year = null,
            TransmissionType? transmissionType = null,
            int? passengerCapacity = null,
            decimal? dailyPrice = null,
            string? location = null,
            DateTime? pickupDate = null,
            DateTime? returnDate = null)
        {
            var result = new ServiceResult<List<VehicleDto>>();

            try
            {
                // Step 1: Fetch all available vehicles
                var carsFromDB = await _unitOfWork.VehicleRepository.GetEntitiesAsync();
                var availableCars = carsFromDB.Where(x => x.Status == VehicleStatus.AVAILABLE);

                // Step 2: Apply filters for brand, model, etc.
                if (!string.IsNullOrEmpty(brand))
                    availableCars = availableCars.Where(x => x.Brand.Contains(brand, StringComparison.OrdinalIgnoreCase));

                if (!string.IsNullOrEmpty(model))
                    availableCars = availableCars.Where(x => x.Model.Contains(model, StringComparison.OrdinalIgnoreCase));

                if (year.HasValue)
                    availableCars = availableCars.Where(x => x.Year == year.Value);

                if (transmissionType.HasValue)
                    availableCars = availableCars.Where(x => x.TransmissionType == transmissionType.Value);

                if (passengerCapacity.HasValue)
                    availableCars = availableCars.Where(x => x.PassengerCapacity == passengerCapacity.Value);

                if (dailyPrice.HasValue)
                    availableCars = availableCars.Where(x => x.DailyPrice <= dailyPrice.Value);

                if (!string.IsNullOrEmpty(location))
                    availableCars = availableCars.Where(x => x.Location.Contains(location, StringComparison.OrdinalIgnoreCase));

                if (pickupDate.HasValue && returnDate.HasValue)
                {
                    var vehicleIds = availableCars.Select(x => x.Id).ToList();
                    var reservations = await _context.Reservations!
                        .Where(r => vehicleIds
                        .Contains(r.VehicleId) && r.Status != ReservationStatus.CANCELED)
                        .ToListAsync();

                    availableCars = availableCars.Where(car =>
                        !reservations.Any(reservation =>
                            reservation.VehicleId == car.Id &&
                            reservation.PickupDate < returnDate.Value &&
                            reservation.ReturnDate > pickupDate.Value && 
                            reservation.Status == ReservationStatus.CONFIRMED
                        )
                    );
                }

                var cars = availableCars.ToList().FromVehicleToVehicleDto();
                result.Data = cars;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = "Error getting available cars.";
                _logger.LogError(ex, result.Message);
            }

            return result;
        }

        public async Task<ServiceResult<VehicleDto>> RegisterVehicle(RegisterVehicleDto registerCarDto)
        {
            var result = new ServiceResult<VehicleDto>();

            try
            {
                var car = new Vehicle
                {
                    Brand = registerCarDto.Brand,
                    Model = registerCarDto.Model,
                    Year = registerCarDto.Year,
                    TransmissionType = registerCarDto.TransmissionType,
                    PassengerCapacity = registerCarDto.PassengerCapacity,
                    DailyPrice = registerCarDto.DailyPrice,
                    Status = registerCarDto.Status,
                    Location = registerCarDto.Location,
                    ImageUrls = new List<string>()
                };

                if (registerCarDto.Images != null && registerCarDto.Images.Count > 0)
                {
                    foreach (var image in registerCarDto.Images)
                    {
                        var imageUrl = await _fileService.UploadFileAsync(image, "cars");
                        car.ImageUrls.Add(imageUrl);
                    }
                }

                await _unitOfWork.VehicleRepository.SaveAsync(car);
                await _unitOfWork.Commit();

                if (registerCarDto.Characteristics != null && registerCarDto.Characteristics.Count > 0)
                {
                    foreach (var characteristicName in registerCarDto.Characteristics)
                    {
                        var characteristic = new VehicleCharacteristic
                        {
                            VehicleId = car.Id, 
                            Name = characteristicName
                        };
                        await _context.VehicleCharacteristics.AddAsync(characteristic);
                    }
                    await _unitOfWork.Commit();
                }

                var carDto = new VehicleDto
                {
                    Brand = car.Brand,
                    Model = car.Model,
                    Year = car.Year,
                    TransmissionType = car.TransmissionType,
                    PassengerCapacity = car.PassengerCapacity,
                    DailyPrice = car.DailyPrice,
                    Status = car.Status,
                    Location = car.Location,
                    ImageUrls = car.ImageUrls,
                    Characteristics = registerCarDto.Characteristics!
                        .Select(name => new VehicleCharacteristicDto { Name = name })
                        .ToList()
                };

                result.Data = carDto;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = "Error saving car.";
                _logger.LogError(ex, result.Message);
            }

            return result;
        }

        public async Task<ServiceResult<VehicleDto>> UpdateVehicle(UpdateVehicleDto updateVehicleDto)
        {
            var result = new ServiceResult<VehicleDto>();

            try
            {
                var car = await _unitOfWork.VehicleRepository.GetEntityAsync(updateVehicleDto.Id);

                if (car == null)
                {
                    result.Success = false;
                    result.Message = "Vehicle not found.";
                    return result;
                }

                // Update basic vehicle properties
                car.Brand = updateVehicleDto.Brand!;
                car.Model = updateVehicleDto.Model!;
                car.Year = updateVehicleDto.Year;
                car.TransmissionType = updateVehicleDto.TransmissionType;
                car.PassengerCapacity = updateVehicleDto.PassengerCapacity;
                car.DailyPrice = updateVehicleDto.DailyPrice;
                car.Status = updateVehicleDto.Status;
                car.Location = updateVehicleDto.Location!;

                // Update images if new ones are provided
                if (updateVehicleDto.Images != null && updateVehicleDto.Images.Count > 0)
                {
                    car.ImageUrls.Clear();

                    foreach (var image in updateVehicleDto.Images)
                    {
                        var imageUrl = await _fileService.UploadFileAsync(image, "cars");
                        car.ImageUrls.Add(imageUrl);
                    }
                }

                var existingCharacteristics = await _context
                    .VehicleCharacteristics
                    .Where(c => c.VehicleId == car.Id)
                    .ToListAsync();

                foreach (var characteristic in existingCharacteristics)
                {
                    _context.VehicleCharacteristics.Remove(characteristic);
                }

                if (updateVehicleDto.Characteristics != null && updateVehicleDto.Characteristics.Count > 0)
                {
                    foreach (var characteristicName in updateVehicleDto.Characteristics)
                    {
                        var characteristic = new VehicleCharacteristic
                        {
                            VehicleId = car.Id,
                            Name = characteristicName
                        };
                        await _context.VehicleCharacteristics.AddAsync(characteristic);
                    }
                }

                await _unitOfWork.VehicleRepository.UpdateAsync(car);
                await _unitOfWork.Commit();

                var carDto = new VehicleDto
                {
                    Brand = car.Brand,
                    Model = car.Model,
                    Year = car.Year,
                    TransmissionType = car.TransmissionType,
                    PassengerCapacity = car.PassengerCapacity,
                    DailyPrice = car.DailyPrice,
                    Status = car.Status,
                    Location = car.Location,
                    ImageUrls = car.ImageUrls,
                    Characteristics = updateVehicleDto.Characteristics!
                        .Select(name => new VehicleCharacteristicDto { Name = name })
                        .ToList()
                };

                result.Data = carDto;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = "Error updating vehicle.";
                _logger.LogError(ex, result.Message);
            }

            return result;
        }

        public async Task<ServiceResult<string>> DeleteVehicle(int id)
        {
            var result = new ServiceResult<string>();

            try
            {
                var car = await _unitOfWork.VehicleRepository.GetEntityAsync(id);

                if (car == null)
                {
                    result.Success = false;
                    result.Message = "Vehicle not found.";
                    return result;
                }

                await _unitOfWork.VehicleRepository.RemoveAsync(car);
                await _unitOfWork.Commit();

                result.Success = true;
                result.Message = "Vehicle deleted successfully.";
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = "Error deleting vehicle.";
                _logger.LogError(ex, result.Message);
            }

            return result;
        }


    }
}
