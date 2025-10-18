using Microsoft.EntityFrameworkCore;
using Mo_Entities.Models;  // để nhận ra Account, Message, v.v.

namespace Mo_DataAccess
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // ✅ Các DbSet (bảng trong database)
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Message> Messages { get; set; }

        // ✅ Cấu hình quan hệ phức tạp
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Thiết lập quan hệ giữa Message ↔ Account
            modelBuilder.Entity<Message>()
                .HasOne(m => m.Sender)
                .WithMany(a => a.MessageSenders)
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Message>()
                .HasOne(m => m.Receiver)
                .WithMany(a => a.MessageReceivers)
                .HasForeignKey(m => m.ReceiverId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
