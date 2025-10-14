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

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Categori__3213E83F61761B8F");

            entity.HasIndex(e => e.Name, "UQ__Categori__72E12F1BB2685783").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Feedback>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Feedback__3213E83F985A2B75");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AccountId).HasColumnName("accountId");
            entity.Property(e => e.Comment)
                .HasMaxLength(255)
                .HasColumnName("comment");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("createdAt");
            entity.Property(e => e.ProductId).HasColumnName("productId");
            entity.Property(e => e.Rating).HasColumnName("rating");

            entity.HasOne(d => d.Account).WithMany(p => p.Feedbacks)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Feedbacks_Account");

            entity.HasOne(d => d.Product).WithMany(p => p.Feedbacks)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FK_Feedbacks_Product");
        });

        modelBuilder.Entity<ImageMessage>(entity =>
        {
            entity.HasKey(e => e.MessageId).HasName("PK__ImageMes__4808B993C0842F19");

            entity.Property(e => e.MessageId)
                .ValueGeneratedNever()
                .HasColumnName("messageId");
            entity.Property(e => e.ImageUrl).HasColumnName("imageUrl");

            entity.HasOne(d => d.Message).WithOne(p => p.ImageMessage)
                .HasForeignKey<ImageMessage>(d => d.MessageId)
                .HasConstraintName("FK_ImageMessages_Message");
        });

        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Messages__3213E83F4148A675");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ReceiverId).HasColumnName("receiverId");
            entity.Property(e => e.SendAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("sendAt");
            entity.Property(e => e.SenderId).HasColumnName("senderId");
            entity.Property(e => e.Type)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("type");

            entity.HasOne(d => d.Receiver).WithMany(p => p.MessageReceivers)
                .HasForeignKey(d => d.ReceiverId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Messages_Receiver");

            entity.HasOne(d => d.Sender).WithMany(p => p.MessageSenders)
                .HasForeignKey(d => d.SenderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Messages_Sender");
        });

        modelBuilder.Entity<OrderProduct>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__OrderPro__3213E83F9D106200");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AccountId).HasColumnName("accountId");
            entity.Property(e => e.ProductVariantId).HasColumnName("productVariantId");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.Status)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasDefaultValue("PENDING")
                .HasColumnName("status");
            entity.Property(e => e.TotalAmount)
                .HasColumnType("decimal(15, 2)")
                .HasColumnName("totalAmount");

            entity.HasOne(d => d.Account).WithMany(p => p.OrderProducts)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Orders_Account");

            entity.HasOne(d => d.ProductVariant).WithMany(p => p.OrderProducts)
                .HasForeignKey(d => d.ProductVariantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OrderProducts_Variant");
        });

        modelBuilder.Entity<OrderProductProductStore>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("OrderProduct_ProductStore");

            entity.Property(e => e.OrderProductId).HasColumnName("orderProductId");
            entity.Property(e => e.ProductStoreId).HasColumnName("productStoreId");

            entity.HasOne(d => d.OrderProduct).WithMany()
                .HasForeignKey(d => d.OrderProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OPS_OrderProduct");

            entity.HasOne(d => d.ProductStore).WithMany()
                .HasForeignKey(d => d.ProductStoreId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OPS_ProductStore");
        });

        modelBuilder.Entity<PaymentTransaction>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PaymentT__3213E83FF8FC6BD9");

            entity.ToTable("PaymentTransaction");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Amount)
                .HasColumnType("decimal(15, 2)")
                .HasColumnName("amount");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("createdAt");
            entity.Property(e => e.PaymentDescription)
                .HasMaxLength(100)
                .HasColumnName("paymentDescription");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("PENDING")
                .HasColumnName("status");
            entity.Property(e => e.Type)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("type");
            entity.Property(e => e.UserId).HasColumnName("userId");

            entity.HasOne(d => d.User).WithMany(p => p.PaymentTransactions)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PaymentTransaction_User");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Products__3213E83F1823B837");

            entity.ToTable(tb => tb.HasTrigger("trg_Products_UpdateTimestamp"));

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("createdAt");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .HasColumnName("description");
            entity.Property(e => e.Details)
                .HasMaxLength(500)
                .HasColumnName("details");
            entity.Property(e => e.Fee)
                .HasDefaultValue(-1m)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("fee");
            entity.Property(e => e.Image).HasColumnName("image");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("isActive");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("name");
            entity.Property(e => e.ShopId).HasColumnName("shopId");
            entity.Property(e => e.SubCategoryId).HasColumnName("subCategoryId");
            entity.Property(e => e.UpdatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("updatedAt");

            entity.HasOne(d => d.Shop).WithMany(p => p.Products)
                .HasForeignKey(d => d.ShopId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Products_Shop");

            entity.HasOne(d => d.SubCategory).WithMany(p => p.Products)
                .HasForeignKey(d => d.SubCategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Products_SubCategory");
        });

        modelBuilder.Entity<ProductStore>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ProductS__3213E83F7BE5AA68");

            entity.ToTable(tb => tb.HasTrigger("trg_ProductStores_UpdateTimestamp"));

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Content)
                .HasMaxLength(255)
                .HasColumnName("content");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("createdAt");
            entity.Property(e => e.ProductVariantId).HasColumnName("productVariantId");
            entity.Property(e => e.Status)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasDefaultValue("AVAILABLE")
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("updatedAt");
            entity.Property(e => e.Value)
                .HasMaxLength(255)
                .HasColumnName("value");

            entity.HasOne(d => d.ProductVariant).WithMany(p => p.ProductStores)
                .HasForeignKey(d => d.ProductVariantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductStores_Variant");
        });

        modelBuilder.Entity<ProductVariant>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ProductV__3213E83F71B6C417");

            entity.ToTable(tb => tb.HasTrigger("trg_ProductVariants_UpdateTimestamp"));

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("createdAt");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("name");
            entity.Property(e => e.Price)
                .HasColumnType("decimal(15, 2)")
                .HasColumnName("price");
            entity.Property(e => e.ProductId).HasColumnName("productId");
            entity.Property(e => e.Stock)
                .HasDefaultValue(0)
                .HasColumnName("stock");
            entity.Property(e => e.UpdatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("updatedAt");

            entity.HasOne(d => d.Product).WithMany(p => p.ProductVariants)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FK_ProductVariants_Product");
        });

        modelBuilder.Entity<Reply>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Replies__3213E83FA5C7C726");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Comment)
                .HasMaxLength(255)
                .HasColumnName("comment");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("createdAt");
            entity.Property(e => e.FeedbackId).HasColumnName("feedbackId");
            entity.Property(e => e.ShopId).HasColumnName("shopId");

            entity.HasOne(d => d.Feedback).WithMany(p => p.Replies)
                .HasForeignKey(d => d.FeedbackId)
                .HasConstraintName("FK_Replies_Feedback");

            entity.HasOne(d => d.Shop).WithMany(p => p.Replies)
                .HasForeignKey(d => d.ShopId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Replies_Shop");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Roles__3213E83FE84FC5F7");

            entity.HasIndex(e => e.RoleName, "UQ__Roles__B194786153C8299A").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.RoleName)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("roleName");
        });

        modelBuilder.Entity<Shop>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Shops__3213E83F509E1028");

            entity.ToTable(tb => tb.HasTrigger("trg_Shops_UpdateTimestamp"));

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AccountId).HasColumnName("accountId");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("createdAt");
            entity.Property(e => e.Description)
                .HasMaxLength(100)
                .HasColumnName("description");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(false)
                .HasColumnName("isActive");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("name");
            entity.Property(e => e.ReportCount)
                .HasDefaultValue(0)
                .HasColumnName("reportCount");
            entity.Property(e => e.UpdatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("updatedAt");

            entity.HasOne(d => d.Account).WithMany(p => p.Shops)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Shops_Account");
        });

        modelBuilder.Entity<SubCategory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__SubCateg__3213E83F80A62B77");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CategoryId).HasColumnName("categoryId");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("isActive");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("name");

            entity.HasOne(d => d.Category).WithMany(p => p.SubCategories)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("FK_SubCategories_Category");
        });

        modelBuilder.Entity<SupportTicket>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__SupportT__3213E83FA58D94D3");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AccountId).HasColumnName("accountId");
            entity.Property(e => e.Content).HasColumnName("content");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("createdAt");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.Phone)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasColumnName("phone");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("title");

            entity.HasOne(d => d.Account).WithMany(p => p.SupportTickets)
                .HasForeignKey(d => d.AccountId)
                .HasConstraintName("FK_SupportTickets_Account");
        });

        modelBuilder.Entity<SystemsConfig>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("SystemsConfig");

            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.Fee)
                .HasDefaultValue(0.50m)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("fee");
            entity.Property(e => e.GoogleAppPassword)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("googleAppPassword");
        });

        modelBuilder.Entity<TextMessage>(entity =>
        {
            entity.HasKey(e => e.MessageId).HasName("PK__TextMess__4808B9930271E4B3");

            entity.Property(e => e.MessageId)
                .ValueGeneratedNever()
                .HasColumnName("messageId");
            entity.Property(e => e.Content).HasColumnName("content");

            entity.HasOne(d => d.Message).WithOne(p => p.TextMessage)
                .HasForeignKey<TextMessage>(d => d.MessageId)
                .HasConstraintName("FK_TextMessages_Message");
        });

        modelBuilder.Entity<Token>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Tokens__3213E83F6F3A3E5F");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AccessToken)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("accessToken");
            entity.Property(e => e.AccountId).HasColumnName("accountId");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("createdAt");
            entity.Property(e => e.ExpiresAt)
                .HasPrecision(0)
                .HasColumnName("expiresAt");
            entity.Property(e => e.RefreshToken)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("refreshToken");

            entity.HasOne(d => d.Account).WithMany(p => p.Tokens)
                .HasForeignKey(d => d.AccountId)
                .HasConstraintName("FK_Tokens_Account");
        });

        modelBuilder.Entity<VnpayTransaction>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__VnpayTra__3213E83F29611770");

            entity.ToTable("VnpayTransaction");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.BankName)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("bankName");
            entity.Property(e => e.Content)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("content");
            entity.Property(e => e.Date)
                .HasPrecision(0)
                .HasColumnName("date");
            entity.Property(e => e.PaymentAccount)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("paymentAccount");
            entity.Property(e => e.PaymentNumber)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("paymentNumber");
            entity.Property(e => e.PaymentTransactionId).HasColumnName("paymentTransactionId");
            entity.Property(e => e.Value)
                .HasColumnType("decimal(15, 2)")
                .HasColumnName("value");

            entity.HasOne(d => d.PaymentTransaction).WithMany(p => p.VnpayTransactions)
                .HasForeignKey(d => d.PaymentTransactionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_VnpayTransaction_Payment");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
