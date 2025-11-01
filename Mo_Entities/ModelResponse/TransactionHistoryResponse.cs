namespace Mo_Entities.ModelResponse
{
    public class TransactionHistoryResponse
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public string Type { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime? CreatedAt { get; set; }
        public string? Status { get; set; }
        
        // Additional info for display
        public string TypeDisplay { get; set; } = string.Empty;
        public string StatusDisplay { get; set; } = string.Empty;
        public string AmountDisplay { get; set; } = string.Empty;
        public bool IsIncome { get; set; }
        public bool IsExpense { get; set; }
    }

    public class TransactionHistoryListResponse
    {
        public List<TransactionHistoryResponse> Transactions { get; set; } = new List<TransactionHistoryResponse>();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public decimal TotalIncome { get; set; }
        public decimal TotalExpense { get; set; }
        public decimal NetAmount { get; set; }
    }
}
