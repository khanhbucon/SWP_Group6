using System;
using System.Collections.Generic;

namespace Mo_Entities.Models;

public partial class ProductVariant
{
    public long Id { get; set; }

    public long ProductId { get; set; }

    public string Name { get; set; } = null!;

    public decimal Price { get; set; }

    public int? Stock { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<OrderProduct> OrderProducts { get; set; } = new List<OrderProduct>();

    public virtual Product Product { get; set; } = null!;

    public virtual ICollection<ProductStore> ProductStores { get; set; } = new List<ProductStore>();
}
