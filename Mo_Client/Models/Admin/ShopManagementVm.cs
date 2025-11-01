using System;
using System.Collections.Generic;

namespace Mo_Client.Models.Admin
{
    public class ShopManagementVm
    {
        public List<ShopVm> Shops { get; set; } = new List<ShopVm>();
        public string? SearchTerm { get; set; }
        public string? Error { get; set; }
        public string? Success { get; set; }
        public int TotalCount { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class ShopVm
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Owner { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int ProductCount { get; set; }
        public int ReportCount { get; set; }
        public string? Description { get; set; }
    }
}
