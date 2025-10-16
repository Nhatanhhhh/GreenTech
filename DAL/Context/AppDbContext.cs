using DAL.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace DAL.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }


        // DbSets
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<WalletTransaction> WalletTransactions { get; set; }
        public DbSet<PointEarningRule> PointEarningRules { get; set; }
        public DbSet<CouponTemplate> CouponTemplates { get; set; }
        public DbSet<Coupon> Coupons { get; set; }
        public DbSet<PointTransaction> PointTransactions { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Blog> Blogs { get; set; }
        public DbSet<Banner> Banners { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<ReviewReply> ReviewReplies { get; set; }
        public DbSet<ReviewVote> ReviewVotes { get; set; }
        public DbSet<ProductRatingStat> ProductRatingStats { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure composite keys
            modelBuilder.Entity<UserRole>()
                .HasKey(ur => new { ur.UserId, ur.RoleId });

            modelBuilder.Entity<ReviewVote>()
                .HasKey(rv => new { rv.ReviewId, rv.UserId });

            // Configure unique indexes
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Phone)
                .IsUnique();

            modelBuilder.Entity<Category>()
                .HasIndex(c => c.Slug)
                .IsUnique();

            modelBuilder.Entity<Supplier>()
                .HasIndex(s => s.Code)
                .IsUnique();

            modelBuilder.Entity<Product>()
                .HasIndex(p => p.Sku)
                .IsUnique();

            modelBuilder.Entity<Product>()
                .HasIndex(p => p.Slug)
                .IsUnique();

            modelBuilder.Entity<Coupon>()
                .HasIndex(c => c.Code)
                .IsUnique();

            modelBuilder.Entity<Order>()
                .HasIndex(o => o.OrderNumber)
                .IsUnique();

            modelBuilder.Entity<Blog>()
                .HasIndex(b => b.Slug)
                .IsUnique();

            // Configure unique constraint for Cart (one cart per user)
            modelBuilder.Entity<Cart>()
                .HasIndex(c => c.UserId)
                .IsUnique();

            // Configure composite unique index for CartItem
            modelBuilder.Entity<CartItem>()
                .HasIndex(ci => new { ci.CartId, ci.ProductId })
                .IsUnique();

            // Configure self-referencing relationship for Categories
            modelBuilder.Entity<Category>()
                .HasOne(c => c.ParentCategory)
                .WithMany(c => c.SubCategories)
                .HasForeignKey(c => c.ParentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure relationships to prevent cascade delete cycles
            modelBuilder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.Coupon)
                .WithMany(c => c.Orders)
                .HasForeignKey(o => o.CouponId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Product)
                .WithMany(p => p.OrderItems)
                .HasForeignKey(oi => oi.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.Product)
                .WithMany(p => p.Reviews)
                .HasForeignKey(r => r.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reviews)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.OrderItem)
                .WithMany(oi => oi.Reviews)
                .HasForeignKey(r => r.OrderItemId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ReviewReply>()
                .HasOne(rr => rr.Review)
                .WithMany(r => r.ReviewReplies)
                .HasForeignKey(rr => rr.ReviewId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ReviewVote>()
                .HasOne(rv => rv.Review)
                .WithMany(r => r.ReviewVotes)
                .HasForeignKey(rv => rv.ReviewId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ProductRatingStat>()
                .HasOne(prs => prs.Product)
                .WithOne(p => p.ProductRatingStat)
                .HasForeignKey<ProductRatingStat>(prs => prs.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<WalletTransaction>()
                .HasOne(wt => wt.Order)
                .WithMany(o => o.WalletTransactions)
                .HasForeignKey(wt => wt.OrderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Cart>()
                .HasOne(c => c.User)
                .WithOne(u => u.Cart)
                .HasForeignKey<Cart>(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Cart>()
                .HasOne(c => c.Coupon)
                .WithMany(cp => cp.Carts)
                .HasForeignKey(c => c.CouponId)
                .OnDelete(DeleteBehavior.SetNull);

            // Configure CartItem relationships
            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.Cart)
                .WithMany(c => c.CartItems)
                .HasForeignKey(ci => ci.CartId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.Product)
                .WithMany(p => p.CartItems)
                .HasForeignKey(ci => ci.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure decimal precision
            modelBuilder.Entity<User>()
                .Property(u => u.WalletBalance)
                .HasPrecision(12, 2);

            modelBuilder.Entity<Product>()
                .Property(p => p.CostPrice)
                .HasPrecision(12, 2);

            modelBuilder.Entity<Product>()
                .Property(p => p.SellPrice)
                .HasPrecision(12, 2);

            modelBuilder.Entity<Product>()
                .Property(p => p.Weight)
                .HasPrecision(8, 2);

            modelBuilder.Entity<WalletTransaction>()
                .Property(wt => wt.Amount)
                .HasPrecision(12, 2);

            modelBuilder.Entity<WalletTransaction>()
                .Property(wt => wt.BalanceBefore)
                .HasPrecision(12, 2);

            modelBuilder.Entity<WalletTransaction>()
                .Property(wt => wt.BalanceAfter)
                .HasPrecision(12, 2);

            modelBuilder.Entity<Order>()
                .Property(o => o.Subtotal)
                .HasPrecision(12, 2);

            modelBuilder.Entity<Order>()
                .Property(o => o.DiscountAmount)
                .HasPrecision(12, 2);

            modelBuilder.Entity<Order>()
                .Property(o => o.ShippingFee)
                .HasPrecision(12, 2);

            modelBuilder.Entity<Order>()
                .Property(o => o.Total)
                .HasPrecision(12, 2);

            modelBuilder.Entity<Order>()
                .Property(o => o.WalletAmountUsed)
                .HasPrecision(12, 2);

            modelBuilder.Entity<OrderItem>()
                .Property(oi => oi.UnitCostPrice)
                .HasPrecision(12, 2);

            modelBuilder.Entity<OrderItem>()
                .Property(oi => oi.UnitSellPrice)
                .HasPrecision(12, 2);

            modelBuilder.Entity<OrderItem>()
                .Property(oi => oi.Total)
                .HasPrecision(12, 2);

            modelBuilder.Entity<ProductRatingStat>()
                .Property(prs => prs.AverageRating)
                .HasPrecision(3, 2);

            // Convert enums to strings in database
            modelBuilder.Entity<User>()
                .Property(u => u.Status)
                .HasConversion<string>();

            modelBuilder.Entity<Role>()
                .Property(r => r.RoleName)
                .HasConversion<string>();

            modelBuilder.Entity<WalletTransaction>()
                .Property(wt => wt.TransactionType)
                .HasConversion<string>();

            modelBuilder.Entity<WalletTransaction>()
                .Property(wt => wt.PaymentGateway)
                .HasConversion<string>();

            modelBuilder.Entity<WalletTransaction>()
                .Property(wt => wt.Status)
                .HasConversion<string>();

            modelBuilder.Entity<CouponTemplate>()
                .Property(ct => ct.DiscountType)
                .HasConversion<string>();

            modelBuilder.Entity<Coupon>()
                .Property(c => c.DiscountType)
                .HasConversion<string>();

            modelBuilder.Entity<Coupon>()
                .Property(c => c.Source)
                .HasConversion<string>();

            modelBuilder.Entity<PointTransaction>()
                .Property(pt => pt.TransactionType)
                .HasConversion<string>();

            modelBuilder.Entity<PointTransaction>()
                .Property(pt => pt.ReferenceType)
                .HasConversion<string>();

            modelBuilder.Entity<Order>()
                .Property(o => o.Status)
                .HasConversion<string>();

            modelBuilder.Entity<Order>()
                .Property(o => o.PaymentStatus)
                .HasConversion<string>();

            modelBuilder.Entity<Order>()
                .Property(o => o.PaymentGateway)
                .HasConversion<string>();

            modelBuilder.Entity<Notification>()
                .Property(n => n.Type)
                .HasConversion<string>();

            modelBuilder.Entity<Notification>()
                .Property(n => n.Priority)
                .HasConversion<string>();

            modelBuilder.Entity<Banner>()
                .Property(b => b.Position)
                .HasConversion<string>();

            modelBuilder.Entity<Review>()
                .Property(r => r.Status)
                .HasConversion<string>();

            modelBuilder.Entity<Cart>()
                .Property(c => c.Subtotal)
                .HasPrecision(12, 2);

            modelBuilder.Entity<Cart>()
                .Property(c => c.DiscountAmount)
                .HasPrecision(12, 2);

            modelBuilder.Entity<CartItem>()
                .Property(ci => ci.UnitPrice)
                .HasPrecision(12, 2);

            modelBuilder.Entity<CartItem>()
                .Property(ci => ci.Subtotal)
                .HasPrecision(12, 2);
        }

    }
}
