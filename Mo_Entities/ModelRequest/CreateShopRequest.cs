using System.ComponentModel.DataAnnotations;

namespace Mo_Entities.ModelRequest;

public class CreateShopRequest
{
    [Required(ErrorMessage = "Tên shop là bắt buộc ")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Tên shop phải từ 3 - 100 Ký tự ")]
    public string Name { get; set; } = string.Empty;

    //Description của phânf tạo shopp Tùy chọn; được cắt thành null trên máy chủ. Độ dài tối đa 100 ký tự.
    [StringLength(100, ErrorMessage = "Mô tả không được quá 100 ký tự")]
    public string? Description { get; set; }
}
