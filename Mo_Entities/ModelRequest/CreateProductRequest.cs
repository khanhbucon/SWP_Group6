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

    [StringLength(255)]
    public string? DetailedDescription { get; set; }

    [Required]
    public long SubCategoryId { get; set; }

    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }

    [Range(0, int.MaxValue)]
    public int Stock { get; set; }

    // For image upload handling on client side
    public string? ImageUrl { get; set; }
}
