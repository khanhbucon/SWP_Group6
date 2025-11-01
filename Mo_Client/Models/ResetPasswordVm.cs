using System.ComponentModel.DataAnnotations;

namespace Mo_Client.Models
{
    public class ResetPasswordVm
    {
        [Required(ErrorMessage = "Token is required")]
        public string Token { get; set; } = string.Empty;

        [Required(ErrorMessage = "New password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 100 characters")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirm password is required")]
        [Compare("NewPassword", ErrorMessage = "Password and confirm password do not match")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = string.Empty;

        public string? Success { get; set; }
        public string? Error { get; set; }
    }
}
