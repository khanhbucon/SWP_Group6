using System.ComponentModel.DataAnnotations;


public class RegisterRequest
{
    [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
    [MinLength(3, ErrorMessage = "Tên đăng nhập tối thiểu 3 ký tự")]
    public string Username { get; set; } = string.Empty;


     [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
    [MinLength(6, ErrorMessage = "Mật khẩu tối thiểu 6 ký tự")]
    public string Password { get; set; } = string.Empty;


    [Required(ErrorMessage = "Vui lòng nhập email")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    public string Email { get; set; } = string.Empty;

   

    public string? Phone { get; set; }
}


