namespace Mo_Entities.ModelResponse;

public class ProductListResponse
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Image { get; set; }
    public string ShopName { get; set; } = string.Empty;
    public long ShopId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string SubCategoryName { get; set; } = string.Empty;
    public decimal MinPrice { get; set; }
    public decimal MaxPrice { get; set; }
    public DateTime? CreatedAt { get; set; }
}




