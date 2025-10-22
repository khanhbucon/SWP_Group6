using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mo_Entities.ModelResponse
{
    public class VnpayDepositResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public VnpayDepositData? Data { get; set; }
    }
    public class VnpayDepositData
    {
        public string TransactionId { get; set; } = string.Empty;
        public string BankAccount { get; set; } = string.Empty;
        public string BankName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime ExpiredAt { get; set; }
        public string QrCode { get; set; } = string.Empty; // QR code để scan
    }
}

