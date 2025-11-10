namespace Mo_Client.Models
{
    public class PaymentTransactionVm
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public string Type { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string PaymentDescription { get; set; } = string.Empty;
        public DateTime? CreatedAt { get; set; }
        public string? Status { get; set; }

        // Display properties
        public string TypeDisplay => Type switch
        {
            "NapTien" => "Nạp tiền",
            "MuaHang" => "Mua hàng",
            "RutTien" => "Rút tiền",
            "HoanTien" => "Hoàn tiền",
            "ChuyenKhoan" => "Chuyển khoản",
            _ => Type
        };

        public string AmountDisplay => $"{Amount:N0} VNĐ";
        
        public bool IsIncome => Type switch
        {
            "NapTien" => true,
            "HoanTien" => true,
            _ => false
        };
        
        public bool IsExpense => Type switch
        {
            "MuaHang" => true,
            "RutTien" => true,
            _ => false
        };

        public string StatusDisplay => Status switch
        {
            "Success" => "Thành công",
            "Pending" => "Đang xử lý",
            "Failed" => "Thất bại",
            "Cancelled" => "Đã hủy",
            _ => Status ?? "Không xác định"
        };

        public string StatusClass => Status switch
        {
            "Success" => "success",
            "Pending" => "warning",
            "Failed" => "danger",
            "Cancelled" => "secondary",
            _ => "secondary"
        };

        public string TypeClass => Type switch
        {
            "Deposit" => "success",
            "Purchase" => "primary",
            "Withdraw" => "warning",
            "Refund" => "info",
            "Transfer" => "secondary",
            _ => "secondary"
        };
    }
}