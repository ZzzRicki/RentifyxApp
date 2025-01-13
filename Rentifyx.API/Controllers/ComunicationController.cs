using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rentifyx.BLL.Contract;
using Rentifyx.DAL.Context;
using Rentifyx.DAL.Entities.Comunication;

namespace Rentifyx.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommunicationsController : ControllerBase
    {
        private readonly RentifyxContext _context;
        private readonly IEmailService _emailSender;

        public CommunicationsController(RentifyxContext context, IEmailService emailSender)
        {
            _context = context;
            _emailSender = emailSender;
        }

        [HttpPost("SendMessage")]
        public async Task<IActionResult> SendMessage([FromBody] Message message)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            message.SentAt = DateTime.UtcNow;
            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Message sent successfully." });
        }

        [HttpGet("GetMessages")]
        public async Task<IActionResult> GetMessages()
        {
            var messages = await _context.Messages
                .OrderBy(m => m.SentAt)
                .ToListAsync();
            return Ok(messages);
        }

        [HttpPost("AnswerEmail/{id}")]
        public async Task<IActionResult> AnswerEmail(int id, [FromBody] string response)
        {
            var message = await _context.Messages.FindAsync(id);

            if (message == null)
            {
                return NotFound(new { success = false, message = "Message not found." });
            }

            if (message.Answered)
            {
                return BadRequest(new { success = false, message = "Message has already been answered." });
            }

            try
            {
                var subject = "Respuesta a tu consulta";
                var emailContent = $@"
                    <p>Hola {message.Name},</p>
                    <p>Gracias por contactarnos. Aquí está nuestra respuesta a tu consulta:</p>
                    <p>{response}</p>
                    <p>Saludos cordiales,<br>Equipo de Soporte de Rentifyx</p>
                ";

                await _emailSender.SendEmailAsync(message.Email, subject, emailContent);

                // Update the message as answered
                message.Answered = true;
                message.Response = response;
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Email response sent and message marked as answered." });
            }
            catch (Exception ex)
            {
                // Log the error and return a server error response
                return StatusCode(500, new { success = false, message = "An error occurred while sending the email.", error = ex.Message });
            }
        }
    }
}
