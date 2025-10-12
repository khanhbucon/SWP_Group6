using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mo_Entities.ModelResponse
{
    public class ListAccountResponse
    {
        public long UserId { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
        public decimal? Balance { get; set; }
        public string? Phone { get; set; }
        public bool? IsActive { get; set; }
        public List<string> Roles { get; set; } = new();     
        public DateTime? CreatedAt { get; set; } 
        public int TotalOrders { get; set; }
        public int TotalShops { get; set; } 
        public int TotalProductsSold { get; set; } 
        public bool IsEKYCVerified { get; set; } 

    }
}
