using System;
using System.Collections.Generic;

namespace Mo_Entities.Models;

public partial class OrderProduct
{
    public long Id { get; set; }

    public long AccountId { get; set; }

    public long ProductVariantId { get; set; }

    public decimal TotalAmount { get; set; }

    public int Quantity { get; set; }

    public string Status { get; set; } = null!;

    public virtual Account Account { get; set; } = null!;

    public virtual ProductVariant ProductVariant { get; set; } = null!;
}
