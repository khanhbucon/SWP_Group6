namespace Mo_Entities.ModelResponse;

public class ShopListResponse
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int ProductCount { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string OwnerName { get; set; } = string.Empty;
}




