using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Mo_Entities.Models;

public partial class SwpGroup6Context : DbContext
{
    public SwpGroup6Context()
    {
    }

    public SwpGroup6Context(DbContextOptions<SwpGroup6Context> options)
        : base(options)
    {
    }

    public virtual DbSet<Account> Accounts { get; set; }
    public virtual DbSet<Category> Categories { get; set; }
    public virtual DbSet<Feedback> Feedbacks { get; set; }
    public virtual DbSet<ImageMessage> ImageMessages { get; set; }
    public virtual DbSet<Message> Messages { get; set; }
    public virtual DbSet<OrderProduct> OrderProducts { get; set; }
    public virtual DbSet<OrderProductProductStore> OrderProductProductStores { get; set; }
    public virtual DbSet<PaymentTransaction> PaymentTransactions { get; set; }
    public virtual DbSet<Product> Products { get; set; }
    public virtual DbSet<ProductStore> ProductStores { get; set; }
    public virtual DbSet<ProductVariant> ProductVariants { get; set; }
    public virtual DbSet<Reply> Replies { get; set; }
    public virtual DbSet<Role> Roles { get; set; }
    public virtual DbSet<Shop> Shops { get; set; }
    public virtual DbSet<SubCategory> SubCategories { get; set; }
    public virtual DbSet<SupportTicket> SupportTickets { get; set; }
    public virtual DbSet<SystemsConfig> SystemsConfigs { get; set; }
    public virtual DbSet<TextMessage> TextMessages { get; set; }
    public virtual DbSet<Token> Tokens { get; set; }
    public virtual DbSet<VnpayTransaction> VnpayTransactions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Accounts__3213E83FD8F5DD9F");

            entity.ToTable(tb =>
            {
                tb.HasTrigger("trg_Accounts_DeleteCascadeMessages");
                tb.HasTrigger("trg_Accounts_UpdateTimestamp");
            });

            entity.HasIndex(e => e.Email, "UQ__Accounts__AB6E61642FC6BB27").IsUnique();
            entity.HasIndex(e => e.Username, "UQ__Accounts__F3DBC57274B8F59B").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Balance)
                .HasDefaultValue(0.00m)
                .HasColumnType("decimal(15, 2)")
                .HasColumnName("balance");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("createdAt");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.GoogleId)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("googleId");
            entity.Property(e => e.IdentificationB)
                .HasMaxLength(500)
                .HasColumnName("identificationB");
            entity.Property(e => e.IdentificationF)
                .HasMaxLength(500)
                .HasColumnName("identificationF");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("isActive");
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("password");
            entity.Property(e => e.Phone)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasColumnName("phone");
            entity.Property(e => e.UpdatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("updatedAt");
            entity.Property(e => e.Username)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("username");

            entity.HasMany(d => d.Roles).WithMany(p => p.Accounts)
                .UsingEntity<Dictionary<string, object>>(
                    "AccountRole",
                    r => r.HasOne<Role>().WithMany()
                        .HasForeignKey("RoleId")
                        .HasConstraintName("FK_AccountRoles_Role"),
                    l => l.HasOne<Account>().WithMany()
                        .HasForeignKey("AccountId")
                        .HasConstraintName("FK_AccountRoles_Account"),
                    j =>
                    {
                        j.HasKey("AccountId", "RoleId");
                        j.ToTable("AccountRoles");
                        j.IndexerProperty<long>("AccountId").HasColumnName("accountId");
                        j.IndexerProperty<long>("RoleId").HasColumnName("roleId");
                    });
        });

        // Explicit Message -> Account relationships (Sender / Receiver)
        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.SenderId).HasColumnName("senderId");
            entity.Property(e => e.ReceiverId).HasColumnName("receiverId");
            entity.Property(e => e.Type).HasColumnName("type");
            entity.Property(e => e.SendAt).HasColumnName("sendAt");

            entity.HasOne(m => m.Sender)
                .WithMany(a => a.MessageSenders)
                .HasForeignKey(m => m.SenderId)
                .HasConstraintName("FK_Messages_Sender")
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(m => m.Receiver)
                .WithMany(a => a.MessageReceivers)
                .HasForeignKey(m => m.ReceiverId)
                .HasConstraintName("FK_Messages_Receiver")
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ImageMessage: one-to-one with Message, primary key = MessageId
        modelBuilder.Entity<ImageMessage>(entity =>
        {
            entity.HasKey(e => e.MessageId);
            entity.Property(e => e.MessageId).HasColumnName("messageId");
            entity.Property(e => e.ImageUrl).HasColumnName("imageUrl");

            entity.HasOne(im => im.Message)
                .WithOne(m => m.ImageMessage)
                .HasForeignKey<ImageMessage>(im => im.MessageId)
                .HasConstraintName("FK_ImageMessages_Message")
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<OrderProductProductStore>(entity =>
        {
            entity.HasKey(e => new { e.OrderProductId, e.ProductStoreId });

            entity.Property(e => e.OrderProductId).HasColumnName("orderProductId");
            entity.Property(e => e.ProductStoreId).HasColumnName("productStoreId");

            entity.HasOne(d => d.OrderProduct)
                .WithMany()
                .HasForeignKey(d => d.OrderProductId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_OrderProductProductStore_OrderProduct");

            entity.HasOne(d => d.ProductStore)
                .WithMany()
                .HasForeignKey(d => d.ProductStoreId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_OrderProductProductStore_ProductStore");
        });

        // If TextMessage is similar (one-to-one), add the same pattern:
        // modelBuilder.Entity<TextMessage>( ... )

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
