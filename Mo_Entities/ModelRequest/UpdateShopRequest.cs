using System.ComponentModel.DataAnnotations;

namespace Mo_Entities.ModelRequest;

public class UpdateShopRequest
{
    [Required(ErrorMessage = "Tên shop là bắt buộc")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Tên shop phải từ 3-100 ký tự")]
    public string Name { get; set; } = string.Empty;

    [StringLength(100, ErrorMessage = "Mô tả không được quá 100 ký tự")]
    public string? Description { get; set; }
}
