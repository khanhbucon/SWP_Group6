using System.ComponentModel.DataAnnotations;

namespace Mo_Client.Models
{
    public class ForgotPasswordVm
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;

        public string? Success { get; set; }
        public string? Error { get; set; }
    }
}
