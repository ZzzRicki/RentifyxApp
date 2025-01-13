using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rentifyx.BLL.Core;
using Rentifyx.BLL.Dto.Reservation;
using Rentifyx.DAL.Context;
using Rentifyx.DAL.Entities;
using Rentifyx.DAL.Enums.Vehicle;
using Rentifyx.DAL.Enums.Reservation;
using System.Security.Claims;

namespace Rentifyx.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReservationsController : ControllerBase
    {
        private readonly RentifyxContext _dbContext;

        public ReservationsController(RentifyxContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> GetReservations()
        {
            var serviceResult = new ServiceResult<List<Reservation>>
            {
                Success = true,
                Data = null,
                Message = string.Empty
            };

            try
            {
                var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                var reservations = await _dbContext.Reservations
                    .Include(r => r.User)
                    .Include(r => r.Vehicle)
                    .ToListAsync();

                serviceResult.Data = reservations;

                if (reservations.Count == 0)
                {
                    serviceResult.Success = true;
                    serviceResult.Message = "No se encontraron reservas para el usuario.";
                }
                else
                {
                    serviceResult.Success = true;
                    serviceResult.Message = "Reservas recuperadas con éxito.";
                }
            }
            catch (Exception ex)
            {
                serviceResult.Success = false;
                serviceResult.Message = $"Error al obtener las reservas: {ex.Message}";
            }

            return Ok(serviceResult);
        }

        [HttpGet("User")]
        [Authorize]
        public async Task<IActionResult> GetUserReservations()
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var reservations = await _dbContext.Reservations
                .Include(r => r.User)
                .Include(r => r.Vehicle)
                .Where(r => r.UserId == userId)
                .ToListAsync();

            return Ok(reservations);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetReservation(int id)
        {
            var reservation = await _dbContext.Reservations
                .Include(r => r.User)
                .Include(r => r.Vehicle)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reservation == null)
                return NotFound("Reservation not found.");

            return Ok(reservation);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateReservation([FromBody] ReservationRequestDto reservationDto)
        {
            var serviceResult = new ServiceResult<string>();

            // Validate the input data
            if (reservationDto == null)
            {
                serviceResult.Success = false;
                serviceResult.Message = "Datos de reservación inválidos.";
                return BadRequest(serviceResult);
            }

            // Retrieve the user ID from token claims
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            // Find the vehicle and check if it's available
            var vehicle = await _dbContext.Vehicles
                .FirstOrDefaultAsync(x => x.Id == reservationDto.VehicleId && x.Status == VehicleStatus.AVAILABLE);

            if (vehicle == null)
            {
                serviceResult.Success = false;
                serviceResult.Message = "El vehículo no está disponible o no existe.";
                return BadRequest(serviceResult);
            }

            // Find the user
            var user = await _dbContext.RentifyxUsers
                .FirstOrDefaultAsync(x => x.Id == userId);

            if (user == null)
            {
                serviceResult.Success = false;
                serviceResult.Message = "Usuario no válido.";
                return BadRequest(serviceResult);
            }

            // Check for existing reservations for this vehicle within the given date range
            var existingReservation = await _dbContext.Reservations
                .FirstOrDefaultAsync(r => r.VehicleId == reservationDto.VehicleId
                                          && r.UserId == userId
                                          && (r.Status == ReservationStatus.PENDING_PAYMENT || r.Status == ReservationStatus.CONFIRMED)
                                          && ((reservationDto.PickupDate >= r.PickupDate && reservationDto.PickupDate <= r.ReturnDate)
                                              || (reservationDto.ReturnDate >= r.PickupDate && reservationDto.ReturnDate <= r.ReturnDate)
                                              || (reservationDto.PickupDate <= r.PickupDate && reservationDto.ReturnDate >= r.ReturnDate)));

            if (existingReservation != null)
            {
                serviceResult.Success = false;
                serviceResult.Message = "Ya tiene una reservación pendiente o confirmada para este vehículo dentro de las fechas seleccionadas.";
                return BadRequest(serviceResult);
            }

            // Calculate rental days
            var rentalDays = (reservationDto.ReturnDate - reservationDto.PickupDate).Days;

            if (rentalDays <= 0)
            {
                serviceResult.Success = false;
                serviceResult.Message = "Período de renta inválido.";
                return BadRequest(serviceResult);
            }

            // Create the reservation
            var reservation = new Reservation()
            {
                TotalCost = rentalDays * vehicle.DailyPrice,
                Status = ReservationStatus.PENDING_PAYMENT,
                UserId = user.Id,
                VehicleId = reservationDto.VehicleId,
                PickupDate = reservationDto.PickupDate,
                ReturnDate = reservationDto.ReturnDate,
                CreationDate = DateTime.Now,
                CreationUser = user.Name
            };

            // Save the reservation to the database
            _dbContext.Reservations.Add(reservation);
            await _dbContext.SaveChangesAsync();

            // Return success result with reservation information
            var successResult = new ServiceResult<Reservation>
            {
                Success = true,
                Message = "¡Reservación creada exitosamente! Proceda a realizar el pago.",
                Data = reservation
            };

            return Ok(successResult);
        }

        [Authorize]
        [HttpPost("cancel/{id}")]
        public async Task<IActionResult> CancelReservation(int id)
        {
            var reservation = await _dbContext.Reservations!.FindAsync(id);

            if (reservation == null)
                return NotFound("Reservation not found.");

            if (reservation.Status == ReservationStatus.CONFIRMED)
                return BadRequest("Cannot cancel a confirmed reservation.");

            reservation.Status = ReservationStatus.CANCELED;
            await _dbContext.SaveChangesAsync();

            return Ok(new { message = "Reservation canceled successfully" });
        }

        [HttpPost("{reservationId}/pagar")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> PagarReserva(int reservationId)
        {
            var serviceResult = new ServiceResult<Reservation>();

            var reservation = await _dbContext.Reservations.FindAsync(reservationId);

            if (reservation == null)
            {
                serviceResult.Success = false;
                serviceResult.Message = "Reserva no encontrada";
                return NotFound(serviceResult);
            }

            reservation.Status = ReservationStatus.CONFIRMED;
            _dbContext.Reservations.Update(reservation);
            await _dbContext.SaveChangesAsync();

            serviceResult.Success = true;
            serviceResult.Data = reservation;
            serviceResult.Message = "Reserva pagada correctamente";
            return Ok(serviceResult);
        }

        [HttpPost("{reservationId}/entregar")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> EntregarVehiculo(int reservationId)
        {
            var serviceResult = new ServiceResult<Reservation>();

            var reservation = await _dbContext.Reservations.FindAsync(reservationId);

            if (reservation == null)
            {
                serviceResult.Success = false;
                serviceResult.Message = "Reserva no encontrada";
                return NotFound(serviceResult);
            }

            reservation.Status = ReservationStatus.ON_GOING;
            _dbContext.Reservations.Update(reservation);
            await _dbContext.SaveChangesAsync();

            serviceResult.Success = true;
            serviceResult.Data = reservation;
            serviceResult.Message = "Vehículo entregado correctamente";
            return Ok(serviceResult);
        }

        [HttpPost("{reservationId}/recibir")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> RecibirVehiculo(int reservationId)
        {
            var serviceResult = new ServiceResult<Reservation>();

            var reservation = await _dbContext.Reservations.FindAsync(reservationId);

            if (reservation == null)
            {
                serviceResult.Success = false;
                serviceResult.Message = "Reserva no encontrada";
                return NotFound(serviceResult);
            }

            reservation.Status = ReservationStatus.COMPLETED;
            _dbContext.Reservations.Update(reservation);
            await _dbContext.SaveChangesAsync();

            serviceResult.Success = true;
            serviceResult.Data = reservation;
            serviceResult.Message = "Vehículo recibido correctamente";
            return Ok(serviceResult);
        }

        [HttpPost("{reservationId}/cancelar")]
        public async Task<IActionResult> CancelarReserva(int reservationId)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            var serviceResult = new ServiceResult<Reservation>();

            var reservation = await _dbContext.Reservations.FindAsync(reservationId);

            if (reservation == null)
            {
                serviceResult.Success = false;
                serviceResult.Message = "Reserva no encontrada";
                return NotFound(serviceResult);
            }

            if (reservation.UserId != userId)
            {
                serviceResult.Success = false;
                serviceResult.Message = "No tienes permiso para cancelar esta reserva";
                return Unauthorized(serviceResult);
            }

            reservation.Status = ReservationStatus.CANCELED;
            _dbContext.Reservations.Update(reservation);
            await _dbContext.SaveChangesAsync();

            serviceResult.Success = true;
            serviceResult.Data = reservation;
            serviceResult.Message = "Reserva cancelada correctamente";
            return Ok(serviceResult);
        }
    }
}
