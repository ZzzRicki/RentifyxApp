using Rentifyx.BLL.Core;
using Rentifyx.BLL.Dto.Auth;

namespace Rentifyx.BLL.Contract
{
    public interface IAuthService
    {
        Task<ServiceResult<object>> RegisterAsync(RegisterRequest registerRequest);
        Task<ServiceResult<object>> LoginAsync(LoginRequest loginRequest);
        Task<ServiceResult<object>> ForgotPasswordAsync(ForgotPasswordRequest forgotPasswordRequest);
        Task<ServiceResult<object>> ConfirmEmailAsync(string userId, string token);
        Task<ServiceResult<object>> ResetPasswordAsync(ResetPasswordRequest resetPasswordRequest);
    }

}
