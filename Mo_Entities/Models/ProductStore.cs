using System;
using System.Collections.Generic;

namespace Mo_Entities.Models;

public partial class ProductStore
{
    public long Id { get; set; }

    public long ProductVariantId { get; set; }

    public string Content { get; set; } = null!;

    public string Value { get; set; } = null!;

    public string Status { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ProductVariant ProductVariant { get; set; } = null!;
}
