using System;
using System.Collections.Generic;

namespace Mo_Entities.Models;

public partial class SubCategory
{
    public long Id { get; set; }

    public long CategoryId { get; set; }

    public string Name { get; set; } = null!;

    public bool? IsActive { get; set; }

    public virtual Category Category { get; set; } = null!;

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
