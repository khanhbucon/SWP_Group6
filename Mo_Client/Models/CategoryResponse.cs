namespace Mo_Client.Models;

public class CategoryResponse
{
    public long Id { get; set; }
    public string Name { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<SubCategoryResponse> SubCategories { get; set; } = new List<SubCategoryResponse>();
}

public class SubCategoryResponse
{
    public long Id { get; set; }
    public string Name { get; set; } = null!;
    public long CategoryId { get; set; }
    public bool? IsActive { get; set; }
}

public class CreateCategoryRequest
{
    public string Name { get; set; } = null!;
}

public class UpdateCategoryRequest
{
    public string Name { get; set; } = null!;
}
