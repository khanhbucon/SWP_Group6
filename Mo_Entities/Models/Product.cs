using System;
using System.Collections.Generic;

namespace Mo_Entities.Models;

public partial class Product
{
    public long Id { get; set; }

    public long ShopId { get; set; }

    public long SubCategoryId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public byte[]? Image { get; set; }

    public bool? IsActive { get; set; }

    public string? Details { get; set; }

    public decimal? Fee { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();

    public virtual ICollection<ProductVariant> ProductVariants { get; set; } = new List<ProductVariant>();

    public virtual Shop Shop { get; set; } = null!;

    public virtual SubCategory SubCategory { get; set; } = null!;
}
