using System;
using System.Collections.Generic;

namespace Mo_Entities.Models;

public partial class TextMessage
{
    public long MessageId { get; set; }

    public string Content { get; set; } = null!;

    public virtual Message Message { get; set; } = null!;
}
