using System.ComponentModel.DataAnnotations;

namespace Mo_Entities.ModelRequest;

public class CreateShopRequest
{
    [Required(ErrorMessage = "Tên shop là bắt buộc ")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Tên shop phải từ 3 - 100 Ký tự ")]
    public string Name { get; set; } = string.Empty;

    [StringLength(100, ErrorMessage = "Mô tả không được quá 100 từ ")]
    public string? Description { get; set; }
}