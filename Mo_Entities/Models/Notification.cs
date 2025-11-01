using System;
using System.Collections.Generic;

namespace Mo_Entities.Models;

public partial class Notification
{
    public long Id { get; set; }

    public long UserId { get; set; }

    public string Type { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string Content { get; set; } = null!;

    public string? RelatedEntityType { get; set; }

    public long? RelatedEntityId { get; set; }

    public bool IsRead { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? ReadAt { get; set; }

    public virtual Account User { get; set; } = null!;
}
