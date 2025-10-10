using System;
using System.Collections.Generic;

namespace Mo_Entities.Models;

public partial class Reply
{
    public long Id { get; set; }

    public long FeedbackId { get; set; }

    public long ShopId { get; set; }

    public string Comment { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public virtual Feedback Feedback { get; set; } = null!;

    public virtual Shop Shop { get; set; } = null!;
}
