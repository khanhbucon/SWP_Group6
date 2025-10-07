using System.ComponentModel.DataAnnotations;


public class ResetPasswordRequest
{
    [Required]
    public string Token { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới")]
    [MinLength(6, ErrorMessage = "Mật khẩu tối thiểu 6 ký tự")]
    public string NewPassword { get; set; } = string.Empty;

    [MinLength(6, ErrorMessage = "Mật khẩu tối thiểu 6 ký tự")]
    public string ConfirmNewPassword { get; set; } = string.Empty;
}


