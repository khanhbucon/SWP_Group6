using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mo_Entities.ModelResponse
{
    public class SepayTransactionResponse
    {
        public int Status { get; set; }
        public SepayTransactionDataWrapper? Data { get; set; }
        public string? Message { get; set; }
    }

    public class SepayTransactionDataWrapper
    {
        public List<SepayTransactionData>? Transactions { get; set; }
    }

    public class SepayTransactionData
    {
        public long Id { get; set; }
        public string? TransactionDate { get; set; }
        public string? AccountNumber { get; set; }
        public decimal? AmountIn { get; set; }
        public decimal? AmountOut { get; set; }
        public string? TransactionContent { get; set; }
        public string? ReferenceCode { get; set; }
        public string? BankBrandName { get; set; }
        public string? Gate { get; set; }
    }

    // Request check transaction status
    public class CheckTransactionRequest
    {
        public long PaymentTransactionId { get; set; }
    }

}
