using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mo_Entities.ModelResponse
{
    public class CreateDepositResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public long? PaymentTransactionId { get; set; }
        public string? QrCodeUrl { get; set; }
        public string? BankAccount { get; set; }
        public string? BankName { get; set; }
        public string? AccountName { get; set; }
        public decimal Amount { get; set; }
        public string? TransferContent { get; set; }
    }
}
