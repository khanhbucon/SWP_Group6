using System;

namespace Mo_Entities.ModelResponse
{
    public class DashboardStatsResponse
    {
        public int TotalUsers { get; set; }
        public int TotalShops { get; set; }
        public int TotalProducts { get; set; }
        public int PendingShops { get; set; }
        public int PendingProducts { get; set; }
        public int BannedUsers { get; set; }
        public List<RecentUserResponse> RecentUsers { get; set; } = new List<RecentUserResponse>();
    }

    public class RecentUserResponse
    {
        public long UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
    }
}
