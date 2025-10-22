namespace Mo_Client.Models
{
    public class ClientPaymentHistoryVm
    {
        public string TransactionId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string BankName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string Content { get; set; } = string.Empty;
    }
}
