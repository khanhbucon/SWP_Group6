using System;
using System.Collections.Generic;

namespace Mo_Entities.Models;

public partial class ImageMessage
{
    public long MessageId { get; set; }

    public byte[] ImageUrl { get; set; } = null!;

    public virtual Message Message { get; set; } = null!;
}
