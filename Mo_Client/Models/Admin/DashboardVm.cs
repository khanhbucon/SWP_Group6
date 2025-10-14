using System;
using System.Collections.Generic;

namespace Mo_Client.Models.Admin
{
    public class DashboardVm
    {
        public int TotalUsers { get; set; }
        public int TotalShops { get; set; }
        public int TotalProducts { get; set; }
        public int PendingShops { get; set; }
        public int PendingProducts { get; set; }
        public int BannedUsers { get; set; }
        public List<RecentUserVm> RecentUsers { get; set; } = new List<RecentUserVm>();
    }

    public class RecentUserVm
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
