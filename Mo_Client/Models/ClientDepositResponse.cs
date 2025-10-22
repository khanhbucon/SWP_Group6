namespace Mo_Client.Models
{
    public class ClientDepositResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public ClientDepositData? Data { get; set; }
    }

    public class ClientDepositData
    {
        public string TransactionId { get; set; } = string.Empty;
        public string BankAccount { get; set; } = string.Empty;
        public string BankName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime ExpiredAt { get; set; }
        public string QrCode { get; set; } = string.Empty;
    }

    // API Response wrapper
    public class ClientApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
    }
}
