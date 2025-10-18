namespace Mo_Entities.ModelRequest
{
    public class SubCategoryRequest
    {
        public string Name { get; set; } = string.Empty;
        public long CategoryId { get; set; }
        public bool? IsActive { get; set; }
    }
}
