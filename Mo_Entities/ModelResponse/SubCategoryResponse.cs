namespace Mo_Entities.ModelResponse
{
    public class SubCategoryResponse
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public long CategoryId { get; set; }
        public bool? IsActive { get; set; }
        public string? CategoryName { get; set; }
    }
}
