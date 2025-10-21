namespace Mo_Client.Models;

public class CategoryVm
{
    public long Id { get; set; }
    public string Name { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<SubCategoryVm> SubCategories { get; set; } = new List<SubCategoryVm>();
}

public class SubCategoryVm
{
    public long Id { get; set; }
    public string Name { get; set; } = null!;
    public long CategoryId { get; set; }
    public bool? IsActive { get; set; }
}

public class CreateCategoryVm
{
    public string Name { get; set; } = null!;
}

public class UpdateCategoryVm
{
    public string Name { get; set; } = null!;
}
