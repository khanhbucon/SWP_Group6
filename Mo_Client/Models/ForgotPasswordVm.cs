using System.ComponentModel.DataAnnotations;

namespace Mo_Client.Models
{
    public class ForgotPasswordVm
    {
        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        public string? Success { get; set; }
        public string? Error { get; set; }
    }
}
