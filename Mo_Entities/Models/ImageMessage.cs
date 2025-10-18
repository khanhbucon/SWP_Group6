using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mo_Entities.Models;

public partial class ImageMessage
{
    [Key] // 👈 Thêm dòng này — đánh dấu khóa chính
    [ForeignKey("Message")] // 👈 vì 1-1 với Message
    public long MessageId { get; set; }

    public byte[] ImageUrl { get; set; } = null!;

    public virtual Message Message { get; set; } = null!;
}
