using System.ComponentModel.DataAnnotations;

namespace Mo_Client.Models
{
    public class DepositVm
    {
        [Required(ErrorMessage = "Vui lòng nhập số tiền")]
        [Range(10000, 100000000, ErrorMessage = "Số tiền phải từ 10,000 VNĐ đến 100,000,000 VNĐ")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập nội dung chuyển khoản")]
        [StringLength(500, ErrorMessage = "Nội dung không được vượt quá 500 ký tự")]
        public string Content { get; set; } = string.Empty;

        public VnpayDepositData? DepositData { get; set; }
        public string? Error { get; set; }
        public string? Success { get; set; }
    }

    public class VnpayDepositData
    {
        public string TransactionId { get; set; } = string.Empty;
        public string BankAccount { get; set; } = string.Empty;
        public string BankName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime ExpiredAt { get; set; }
        public string QrCode { get; set; } = string.Empty;
    }
}
