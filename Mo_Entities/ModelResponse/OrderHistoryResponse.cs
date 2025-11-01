namespace Mo_Entities.ModelResponse
{
    public class OrderHistoryResponse
    {
        // Order Information
        public long OrderId { get; set; }
        public string Status { get; set; } = string.Empty;
        public string StatusDisplay { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal TotalAmount { get; set; }
        public string TotalAmountDisplay { get; set; } = string.Empty;
        public DateTime? CreatedAt { get; set; }
        public string CreatedAtDisplay { get; set; } = string.Empty;

        // Product Information
        public long ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? ProductDescription { get; set; }
        public byte[]? ProductImage { get; set; }
        public string ProductImageBase64 { get; set; } = string.Empty;

        // Product Variant Information
        public long ProductVariantId { get; set; }
        public string VariantName { get; set; } = string.Empty;
        public decimal VariantPrice { get; set; }
        public string VariantPriceDisplay { get; set; } = string.Empty;

        // Shop Information
        public long ShopId { get; set; }
        public string ShopName { get; set; } = string.Empty;
        public string? ShopDescription { get; set; }
        public string? ShopEmail { get; set; }
        public string? ShopPhone { get; set; }

        // Product Store Codes (mã để sử dụng)
        public List<ProductStoreInfo> ProductCodes { get; set; } = new List<ProductStoreInfo>();

        // Additional calculated fields
        public string StatusClass { get; set; } = string.Empty;
        public bool HasCodes { get; set; }
        public bool CanCancel { get; set; }
    }

    public class ProductStoreInfo
    {
        public string Content { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string StatusDisplay { get; set; } = string.Empty;
    }

    public class OrderHistoryListResponse
    {
        public List<OrderHistoryResponse> Orders { get; set; } = new List<OrderHistoryResponse>();
        public int TotalCount { get; set; }
        
        // Summary statistics
        public decimal TotalSpent { get; set; }
        public int TotalOrders { get; set; }
        public int CompletedOrders { get; set; }
        public int PendingOrders { get; set; }
        public int ConfirmedOrders { get; set; }
        public int CancelledOrders { get; set; }
    }
}

