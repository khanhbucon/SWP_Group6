using System;
using System.Collections.Generic;

namespace Mo_Client.Models.Admin
{
    public class ProductManagementVm
    {
        public List<ProductVm> Products { get; set; } = new List<ProductVm>();
        public string? SearchTerm { get; set; }
        public string? Error { get; set; }
        public string? Success { get; set; }
        public int TotalCount { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class ProductVm
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ShopName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public DateTime CreatedAt { get; set; }
        public int SoldCount { get; set; }
        public string? Description { get; set; }
    }
}
