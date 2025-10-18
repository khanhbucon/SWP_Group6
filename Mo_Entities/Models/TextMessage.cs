using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mo_Entities.Models;

public partial class TextMessage
{
    [Key]  // 👈 Đánh dấu đây là khóa chính
    [ForeignKey("Message")] // 👈 Quan hệ 1–1 với Message
    public long MessageId { get; set; }

    public string Content { get; set; } = null!;

    public virtual Message Message { get; set; } = null!;
}
