namespace Mo_Client.Models
{
    public class LoginVm
    {
        public string Identifier { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool RememberMe { get; set; }
        public string? ReturnUrl { get; set; }
        public string? Error { get; set; }
        public string? Success { get; set; }
    }

}
