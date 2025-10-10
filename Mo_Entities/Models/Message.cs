using System;
using System.Collections.Generic;

namespace Mo_Entities.Models;

public partial class Message
{
    public long Id { get; set; }

    public long SenderId { get; set; }

    public long ReceiverId { get; set; }

    public string Type { get; set; } = null!;

    public DateTime? SendAt { get; set; }

    public virtual ImageMessage? ImageMessage { get; set; }

    public virtual Account Receiver { get; set; } = null!;

    public virtual Account Sender { get; set; } = null!;

    public virtual TextMessage? TextMessage { get; set; }
}
