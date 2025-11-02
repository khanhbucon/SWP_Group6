namespace Mo_Entities.ModelResponse;

public class AdminProductListItem
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ShopName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int SoldCount { get; set; }
    // Status: "Active", "Inactive", "Pending"
    public string Status { get; set; } = string.Empty;
    public DateTime? CreatedAt { get; set; }
    public string? Description { get; set; }
}
