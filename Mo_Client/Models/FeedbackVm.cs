namespace Mo_Client.Models
{
    public class FeedbackVm
    {
        public long AccountId { get; set; }
        public string? AccountName { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public long ProductId { get; set; }
    }
}
