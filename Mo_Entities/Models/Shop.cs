using System;
using System.Collections.Generic;

namespace Mo_Entities.Models;

public partial class Shop
{
    public long Id { get; set; }

    public long AccountId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public int? ReportCount { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Account Account { get; set; } = null!;

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();

    public virtual ICollection<Reply> Replies { get; set; } = new List<Reply>();
}
