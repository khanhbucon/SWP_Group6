namespace Mo_Client.Models
{
    public class ClientPaymentTransactionVm
    {
        public long Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string PaymentDescription { get; set; } = string.Empty;
        public DateTime? CreatedAt { get; set; }
        public string? Status { get; set; }
    }
}
