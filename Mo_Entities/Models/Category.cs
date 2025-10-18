using System.ComponentModel.DataAnnotations.Schema;

namespace Mo_Entities.Models
{
    [Table("Categories")] // 👈 trỏ đúng bảng
    public partial class Category
    {
        [Column("id")]      // 👈 trỏ đúng tên cột trong SQL
        public long Id { get; set; }

        [Column("name")]    // 👈 trỏ đúng tên cột trong SQL
        public string Name { get; set; } = null!;

        public virtual ICollection<SubCategory> SubCategories { get; set; } = new List<SubCategory>();
    }
}
