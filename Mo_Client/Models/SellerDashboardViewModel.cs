using Mo_Client.Services;

namespace Mo_Client.Models
{
    public class SellerDashboardViewModel
    {
        public ProfileResponse? Profile { get; set; }
        public AuthApiClient.ShopStatisticsResponse? Stats { get; set; }
    }
}
