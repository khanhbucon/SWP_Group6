using System;

namespace Mo_Client.Models.Admin
{
    public class DashboardStatsVm
    {
        public int TotalUsers { get; set; }
        public int TotalShops { get; set; }
        public int TotalProducts { get; set; }
        public int PendingShops { get; set; }
        public int PendingProducts { get; set; }
        public int BannedUsers { get; set; }
        public List<RecentUserVm> RecentUsers { get; set; } = new List<RecentUserVm>();
    }

  
}
