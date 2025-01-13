using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rentifyx.BLL.Contract;
using Rentifyx.BLL.Dto.Car;
using Rentifyx.BLL.Dto.Vehicle;
using Rentifyx.DAL.Context;
using Rentifyx.DAL.Enums.Reservation;
using Rentifyx.DAL.Enums.Vehicle;

namespace Rentifyx.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VehiclesController : ControllerBase
    {
        private readonly IVehicleService _carService;
        private readonly RentifyxContext _rentifyxContext;

        public VehiclesController(IVehicleService carService, RentifyxContext rentifyxContext)
        {
            _carService = carService;
            _rentifyxContext = rentifyxContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllVehicles()
        {
            var result = await _carService.GetAllVehicles();

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpGet("{Id}")]
        public async Task<IActionResult> GetById(int Id)
        {
            var vehicleFromDB = await _rentifyxContext.Vehicles
                .Include(x => x.Characteristics)
                .FirstOrDefaultAsync(x => x.Id == Id);

            if (vehicleFromDB == null)
            {
                return BadRequest();
            }

            var vehicle = new VehicleDto()
            {
                Id = Id,
                Brand = vehicleFromDB.Brand,
                DailyPrice = vehicleFromDB.DailyPrice,
                ImageUrls = vehicleFromDB.ImageUrls,
                Location = vehicleFromDB.Location,
                Model = vehicleFromDB.Model,
                PassengerCapacity = vehicleFromDB.PassengerCapacity,
                Status = vehicleFromDB.Status,
                TransmissionType = vehicleFromDB.TransmissionType,
                Year = vehicleFromDB.Year,
                Characteristics = vehicleFromDB.Characteristics
                .Select(x => new VehicleCharacteristicDto() { Name = x.Name ?? "" }).ToList(),
            };

            return Ok(vehicle);

        }

        [HttpGet("available")]
        public async Task<IActionResult> GetAvailableVehicles(
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
            var result = await _carService.GetAvailableVehicles(
                brand,
                model,
                year,
                transmissionType,
                passengerCapacity,
                dailyPrice,
                location,
                pickupDate,
                returnDate
            );

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Register([FromForm] RegisterVehicleDto registerCarDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _carService.RegisterVehicle(registerCarDto);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpPut]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Update([FromForm] UpdateVehicleDto updateVehicleDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _carService.UpdateVehicle(updateVehicleDto);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _carService.DeleteVehicle(id);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [Authorize(Roles = "Administrator")]
        [HttpGet("GetAvailableVehiclesCount")]
        public async Task<IActionResult> GetAvailableVehiclesCount()
        {
            var availableVehiclesCount = await _rentifyxContext.Vehicles
                .CountAsync(v => v.Status == VehicleStatus.AVAILABLE);

            return Ok(new { count = availableVehiclesCount });
        }

        [HttpGet("GetActiveReservationsCount")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> GetActiveReservationsCount()
        {
            var activeReservationsCount = await _rentifyxContext.Reservations
                .CountAsync(r => r.Status == ReservationStatus.ON_GOING); 

            return Ok(new { count = activeReservationsCount });
        }

        [HttpGet("GetTotalRevenue")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> GetTotalRevenue()
        {
            var totalRevenue = await _rentifyxContext.Reservations
                .Where(r => r.Status == ReservationStatus.COMPLETED) 
                .SumAsync(r => r.TotalCost); 
            return Ok(new { totalRevenue });
        }
    }
}
