using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mo_Entities.ModelResponse
{
    public class PurchaseResponse
    {
        public bool Success { get; set; }
        public long OrderId { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string TotalAmountDisplay { get; set; } = string.Empty;
        public List<ProductCodeInfo> ProductCodes { get; set; } = new List<ProductCodeInfo>();
        public string Message { get; set; } = string.Empty;
    }

    public class ProductCodeInfo
    {
        public string Content { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string StatusDisplay { get; set; } = string.Empty;
    }
}
