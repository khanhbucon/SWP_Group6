namespace Mo_Entities.ModelResponse;

public class ShopStatisticsResponse
{
    public long ShopId { get; set; }
    public string ShopName { get; set; } = string.Empty;
    public int TotalProducts { get; set; }
    public int TotalProductsSold { get; set; }
    public decimal TotalRevenue { get; set; }
    public int TotalOrders { get; set; }
    public decimal AverageRating { get; set; }
    public int TotalFeedbacks { get; set; }
}