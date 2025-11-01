namespace Mo_Client.Models
{
    public class Notification
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? RelatedEntityType { get; set; }
        public long? RelatedEntityId { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ReadAt { get; set; }
    }

    public class UnreadCountResponse
    {
        public int UnreadCount { get; set; }
    }
}
