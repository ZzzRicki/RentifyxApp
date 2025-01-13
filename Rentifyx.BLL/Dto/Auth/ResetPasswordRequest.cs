namespace Rentifyx.BLL.Dto.Auth
{
    public class ResetPasswordRequest
    {
        public string UserId { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }
}
