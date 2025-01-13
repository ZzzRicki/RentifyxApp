using Azure.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Rentifyx.BLL.Contract;
using Rentifyx.BLL.Core;
using Rentifyx.BLL.Dto.Auth;
using Rentifyx.DAL.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Rentifyx.BLL.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<RentifyxUser> _userManager;
        private readonly ILogger<AuthService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailSender;

        public AuthService(UserManager<RentifyxUser> userManager, ILogger<AuthService> logger, IConfiguration configuration, IEmailService emailSender)
        {
            _logger = logger;
            _userManager = userManager;
            _configuration = configuration;
            _emailSender = emailSender;
        }

        public async Task<ServiceResult<object>> RegisterAsync(RegisterRequest registerRequest)
        {
            var email = registerRequest.Email;
            var phoneNumber = registerRequest.PhoneNumber;
            var fullName = registerRequest.FullName;
            var password = registerRequest.Password;
            var passwordConfirmation = registerRequest.PasswordConfirmation;

            var result = new ServiceResult<object>();

            if (password != passwordConfirmation)
            {
                result.Success = false;
                result.Message = "Password and confirmation password do not match.";
                _logger.LogWarning("Password confirmation failed for email: {0}", email);
                return result;
            }

            try
            {
                var user = new RentifyxUser
                {
                    UserName = email,
                    Email = email,
                    PhoneNumber = phoneNumber,
                    Name = fullName
                };

                var identityResult = await _userManager.CreateAsync(user, password);
                if (identityResult.Succeeded)
                {
                    var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    var encodedToken = Uri.EscapeDataString(token);
                    var confirmationLink = $"{_configuration["ClientUrl"]}/auth/confirm-email?userId={user.Id}&token={encodedToken}";

                    await _emailSender.SendEmailAsync(email, "Confirma tu correo", $"Por favor confirma tu correo haciendo click en el siguiente enlace: {confirmationLink}");

                    _logger.LogInformation("User registered and confirmation email sent.");
                    result.Success = true;
                    result.Message = "Registration successful. Please confirm your email.";
                }
                else
                {
                    result.Success = false;
                    result.Message = string.Join(", ", identityResult.Errors.Select(e => e.Description));
                    _logger.LogWarning("Registration failed: {0}", result.Message);
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = "Error during registration.";
                _logger.LogError(ex, result.Message);
            }

            return result;
        }
        public async Task<ServiceResult<object>> LoginAsync(LoginRequest loginRequest)
        {
            var result = new ServiceResult<object>();

            try
            {
                string email = loginRequest.Email;
                string password = loginRequest.Password;

                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    result.Success = false;
                    result.Message = "Credenciales incorrectas";
                    _logger.LogWarning("Login failed. User with email {0} not found.", email);
                    return result;
                }

                var passwordCheck = await _userManager.CheckPasswordAsync(user, password);
                if (!passwordCheck)
                {
                    result.Success = false;
                    result.Message = "Invalid credentials.";
                    _logger.LogWarning("Login failed for user {0}. Incorrect password.", email);
                    return result;
                }

                if (!user.EmailConfirmed)
                {
                    result.Success = false;
                    result.Message = "Email not confirmed.";
                    _logger.LogWarning("Login failed for user {0}. Email not confirmed.", email);
                    return result;
                }

                // Generate JWT token
                var token = await GenerateJwtToken(user);

                result.Success = true;
                result.Message = "Login successful.";
                result.Data = new { Token = token.Item1, Role = token.Item2, Email = token.Item3, Name = token.Item4 };
                _logger.LogInformation("User {0} logged in successfully.", email);
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = "Error during login.";
                _logger.LogError(ex, result.Message);
            }

            return result;
        }
        private async Task<(string, string, string, string)> GenerateJwtToken(RentifyxUser user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!);

            var roles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Email, user.Email!),
        new Claim(ClaimTypes.Name, user.UserName!)
    };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(8),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return (tokenHandler.WriteToken(token), roles.FirstOrDefault() ?? "User", user.Email ?? "", user.Name ?? "");
        }

        public async Task<ServiceResult<object>> ForgotPasswordAsync(ForgotPasswordRequest forgotPasswordRequest)
        {
            var result = new ServiceResult<object>();

            var email = forgotPasswordRequest.Email;

            try
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    result.Success = false;
                    result.Message = "Usuario no encontrado";
                    _logger.LogWarning("Password reset requested for non-existent email {0}.", email);
                    return result; // No se revela que el usuario no existe
                }

                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var encodedToken = Uri.EscapeDataString(token);
                var resetLink = $"{_configuration["ClientUrl"]}/auth/reset-password?userId={user.Id}&token={encodedToken}";

                await _emailSender.SendEmailAsync(email, "Restablecer contraseña", $"Haga click en este enlace para restablecer su contraseña: {resetLink}");

                result.Success = true;
                result.Message = "Correo de reinicio de contraseña enviado";
                _logger.LogInformation("Password reset email sent to {0}.", email);
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = "Error reiniciando contraseña";
                _logger.LogError(ex, result.Message);
            }

            return result;
        }
        public async Task<ServiceResult<object>> ConfirmEmailAsync(string userId, string token)
        {
            var result = new ServiceResult<object>();

            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    result.Success = false;
                    result.Message = "Usuario no encontrado";
                    _logger.LogWarning("Email confirmation failed. User {0} not found.", userId);
                    return result;
                }

                var identityResult = await _userManager.ConfirmEmailAsync(user, token);
                if (identityResult.Succeeded)
                {
                    result.Success = true;
                    result.Message = "Email confirmado satisfactoriamente";
                    _logger.LogInformation("Email confirmed for user {0}.", userId);
                }
                else
                {
                    result.Success = false;
                    result.Message = string.Join(", ", identityResult.Errors.Select(e => e.Description));
                    _logger.LogWarning("Email confirmation failed for user {0}.", userId);
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = "Error durante la confirmacion de correo";
                _logger.LogError(ex, result.Message);
            }

            return result;
        }
        public async Task<ServiceResult<object>> ResetPasswordAsync(ResetPasswordRequest resetPasswordRequest)
        {
            var result = new ServiceResult<object>();

            var userId = resetPasswordRequest.UserId;
            var decodedToken = Uri.UnescapeDataString(resetPasswordRequest.Token);
            var newPassword = resetPasswordRequest.NewPassword;
            var confirmNewPassword = resetPasswordRequest.ConfirmNewPassword;

            if (newPassword != confirmNewPassword)
            {
                result.Success = false;
                result.Message = "Las contraseñas no coinciden";
                _logger.LogWarning("Passwords doesn´t match");
                return result;
            }

            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    result.Success = false;
                    result.Message = "Usuario no encontrado";
                    _logger.LogWarning("Password reset failed. User {0} not found.", userId);
                    return result;
                }

                var identityResult = await _userManager.ResetPasswordAsync(user, decodedToken, newPassword);
                if (identityResult.Succeeded)
                {
                    result.Success = true;
                    result.Message = "Contraseña reiniciada exitosamente";
                    _logger.LogInformation("Password reset successful for user {0}.", userId);
                }
                else
                {
                    result.Success = false;
                    result.Message = string.Join(", ", identityResult.Errors.Select(e => e.Description));
                    _logger.LogWarning("Password reset failed for user {0}.", userId);
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = "Error al reiniciar la contraseña";
                _logger.LogError(ex, result.Message);
            }

            return result;
        }
    }
}
