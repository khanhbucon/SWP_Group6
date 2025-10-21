namespace Mo_Entities.ModelRequest;

public class CreateSubCategoryRequest
{
    public string Name { get; set; } = null!;
    public bool IsActive { get; set; } = true;
}

public class UpdateSubCategoryRequest
{
    public string Name { get; set; } = null!;
    public bool IsActive { get; set; } = true;
}
