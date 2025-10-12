using System.ComponentModel.DataAnnotations;

namespace Mo_Entities.ModelRequest;

public class UpdateShopRequest
{
    [Required(ErrorMessage = "T�n shop l� b?t bu?c")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "T�n shop ph?i t? 3-100 k? t?")]
    public string Name { get; set; } = string.Empty;

    [StringLength(100, ErrorMessage = "M� t? kh�ng ��?c qu� 100 k? t?")]
    public string? Description { get; set; }
}