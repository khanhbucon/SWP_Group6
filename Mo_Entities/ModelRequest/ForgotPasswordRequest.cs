using System.ComponentModel.DataAnnotations;

namespace Mo_Entities.ModelRequest
{
    public class ForgotPasswordRequest
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;
    }
}
