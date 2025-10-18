namespace Mo_Entities.ModelRequest
{
    public class OrderProductRequest
    {
        public long AccountId { get; set; }
        public long ProductVariantId { get; set; }
        public decimal TotalAmount { get; set; }
        public int Quantity { get; set; }
        public string Status { get; set; } = "PENDING";
    }
}
