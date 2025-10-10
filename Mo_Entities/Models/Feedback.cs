using System;
using System.Collections.Generic;

namespace Mo_Entities.Models;

public partial class Feedback
{
    public long Id { get; set; }

    public long AccountId { get; set; }

    public long ProductId { get; set; }

    public int Rating { get; set; }

    public string? Comment { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Account Account { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;

    public virtual ICollection<Reply> Replies { get; set; } = new List<Reply>();
}
