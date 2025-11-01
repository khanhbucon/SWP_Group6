namespace Mo_Entities.ModelResponse;

public class AdminShopListItem
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Owner { get; set; } = string.Empty;
    // Status: "Active", "Inactive", "Pending"
    public string Status { get; set; } = string.Empty;
    public int ProductCount { get; set; }
    public int ReportCount { get; set; }
    public DateTime? CreatedAt { get; set; }
}
