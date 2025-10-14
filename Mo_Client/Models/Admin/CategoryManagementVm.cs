using System;
using System.Collections.Generic;

namespace Mo_Client.Models.Admin
{
    public class CategoryManagementVm
    {
        public List<CategoryVm> Categories { get; set; } = new List<CategoryVm>();
        public string? Error { get; set; }
        public string? Success { get; set; }
        public int TotalCount { get; set; }
    }

    public class CategoryVm
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int ProductCount { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<SubCategoryVm> SubCategories { get; set; } = new List<SubCategoryVm>();
    }

    public class SubCategoryVm
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int ProductCount { get; set; }
        public bool IsActive { get; set; }
        public long CategoryId { get; set; }
    }
}
