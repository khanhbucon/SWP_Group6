namespace Mo_Entities.Models.Response
{
    public class OrderProductRp
    {
        public long Id { get; set; }
        public long AccountId { get; set; }
        public string? AccountName { get; set; }
        public long ProductVariantId { get; set; }
        public string? ProductName { get; set; }
        public decimal TotalAmount { get; set; }
        public int Quantity { get; set; }
        public string Status { get; set; } = "";

    }
}
