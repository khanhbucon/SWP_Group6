using System.ComponentModel.DataAnnotations;

namespace Mo_Entities.ModelRequest;

public class UpdateProductRequest
{
    [Required]
    public long Id { get; set; }

    [StringLength(100)]
    public string? Name { get; set; }

    [StringLength(200)]
    public string? ShortDescription { get; set; }

    [StringLength(500)]
    public string? DetailedDescription { get; set; }

    [Range(typeof(decimal), "0", "999.99", ErrorMessage = "Phí sàn phải từ 0 đến 999.99")]
    public decimal? Fee { get; set; }

    public bool? IsActive { get; set; }

    // Optional: base64 data URL (or plain base64) to update product image
    // If null => do not change. If provided => replace image. If RemoveImage == true => clear image.
    public string? ImageUrl { get; set; }

    public bool? RemoveImage { get; set; }
}
