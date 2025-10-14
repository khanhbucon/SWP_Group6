using Microsoft.EntityFrameworkCore;
using Mo_Entities; // nếu entity classes ở project Mo_Entities

namespace Mo_DataAccess
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        // thêm các DbSet khác nếu cần
    }
}
