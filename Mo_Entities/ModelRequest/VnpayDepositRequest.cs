using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mo_Entities.ModelRequest
{
    public class VnpayDepositRequest
    {
        [Required]
        [Range(10000, 100000000, ErrorMessage = "Số tiền nạp phải từ 10,000 VNĐ đến 100,000,000 VNĐ")]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(500, ErrorMessage = "Nội dung chuyển khoản không được vượt quá 500 ký tự")]
        public string Content { get; set; } = string.Empty;

        [Required]
        public string BankCode { get; set; } = "BIDV"; // Mặc định BIDV
    }
}
