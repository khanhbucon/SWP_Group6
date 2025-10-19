using System;
using System.Collections.Generic;

namespace Mo_Entities.ModelResponse
{
    public class ProfileResponse
    {
        public long Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public decimal? Balance { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<string> Roles { get; set; } = new();
        public bool IsEKYCVerified { get; set; }

        public int TotalOrders { get; set; }
        public int TotalShops { get; set; }
        public int TotalProductsSold { get; set; }
    }
}
