using System.ComponentModel.DataAnnotations;

namespace Mo_Entities.ModelRequest;

public class CreateProductRequest
{
    [Required]
    public long ShopId { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(200)]
    public string? ShortDescription { get; set; }

    // Details column allows up to 500 chars
    [StringLength(500)]
    public string? DetailedDescription { get; set; }

    [Required]
    public long SubCategoryId { get; set; }

    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }

    [Range(0, int.MaxValue)]
    public int Stock { get; set; }

    // For image upload handling on client side
    public string? ImageUrl { get; set; }

    // Optional per-product platform fee override (e.g., percentage like 5%)
    [Range(typeof(decimal), "0", "5", ErrorMessage = "Phí sàn phải từ 0 đến 5")]
    public decimal? Fee { get; set; }

    // Tên biến thể để dễ quản lý
    [StringLength(100)]
    public string? VariantName { get; set; }
}
