namespace Mo_Client.Models
{
    public class RegisterVm
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
        public string? Error { get; set; }
        public string? Success { get; set; }
    }
}
