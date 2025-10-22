using System.ComponentModel.DataAnnotations;

namespace Mo_Client.Models
{
    public class ClientDepositRequest
    {
        [Required]
        [Range(10000, 100000000, ErrorMessage = "Số tiền phải từ 10,000 VNĐ đến 100,000,000 VNĐ")]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(500, ErrorMessage = "Nội dung không được vượt quá 500 ký tự")]
        public string Content { get; set; } = string.Empty;

        public string BankCode { get; set; } = "BIDV";
    }

    public class ClientVerifyRequest
    {
        [Required]
        public string TransactionId { get; set; } = string.Empty;

        [Required]
        [Range(1000, 100000000)]
        public decimal Amount { get; set; }
    }
}
