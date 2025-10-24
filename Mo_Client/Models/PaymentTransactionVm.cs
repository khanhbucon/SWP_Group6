namespace Mo_Client.Models
{
    public class PaymentTransactionVm
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public string Type { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime? CreatedAt { get; set; }
        public string? Status { get; set; }

        // Display properties
        public string TypeDisplay => Type switch
        {
            "NapTien" => "Nạp tiền",
            "MuaHang" => "Mua hàng",
            "RutTien" => "Rút tiền",
            "BanHang" => "Bán hàng",
            "HoaHong" => "Hoa hồng",
            "ChiaSe" => "Chia sẻ",
            _ => Type
        };

        public string AmountDisplay => $"{Amount:N0} VNĐ";
        
        public bool IsIncome => Type switch
        {
            "NapTien" => true,
            "BanHang" => true,
            "HoaHong" => true,
            _ => false
        };
        
        public bool IsExpense => Type switch
        {
            "MuaHang" => true,
            "RutTien" => true,
            "ChiaSe" => true,
            _ => false
        };

        public string StatusDisplay => Status switch
        {
            "COMPLETED" => "Thành công",
            "PENDING" => "Đang xử lý",
            "FAILED" => "Thất bại",
            "CANCELLED" => "Đã hủy",
            _ => Status ?? "Không xác định"
        };

        public string StatusClass => Status switch
        {
            "COMPLETED" => "success",
            "PENDING" => "warning",
            "FAILED" => "danger",
            "CANCELLED" => "secondary",
            _ => "secondary"
        };

        public string TypeClass => Type switch
        {
            "NapTien" => "success",
            "MuaHang" => "primary",
            "RutTien" => "warning",
            "BanHang" => "info",
            "HoaHong" => "success",
            "ChiaSe" => "secondary",
            _ => "secondary"
        };

        public string TypeIcon => Type switch
        {
            "NapTien" => "fas fa-plus-circle",
            "MuaHang" => "fas fa-shopping-cart",
            "RutTien" => "fas fa-minus-circle",
            "BanHang" => "fas fa-store",
            "HoaHong" => "fas fa-gift",
            "ChiaSe" => "fas fa-share-alt",
            _ => "fas fa-circle"
        };
    }
}