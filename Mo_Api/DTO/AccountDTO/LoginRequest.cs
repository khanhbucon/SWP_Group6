using System.ComponentModel.DataAnnotations;

namespace Mo_Api.DTO.AccountDTO;

public class LoginRequest
{
    [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập hoặc email")]
    [MinLength(3, ErrorMessage = "Tên đăng nhập/email tối thiểu 3 ký tự")]
    public string Identifier { get; set; } = string.Empty; // username or email

    [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
    [MinLength(6, ErrorMessage = "Mật khẩu tối thiểu 6 ký tự")]
    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; } = false;
}

