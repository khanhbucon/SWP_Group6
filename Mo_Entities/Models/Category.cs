using System;
using System.Collections.Generic;

namespace Mo_Entities.Models;

public partial class Category
{
    public long Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<SubCategory> SubCategories { get; set; } = new List<SubCategory>();
}
