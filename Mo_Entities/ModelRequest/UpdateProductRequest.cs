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
}
