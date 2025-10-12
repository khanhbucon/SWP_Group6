namespace Mo_Entities.ModelResponse;

public class ShopResponse
{
    public long Id { get; set; }
    public long AccountId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? ReportCount { get; set; }
    public bool? IsActive { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int TotalProducts { get; set; }
}