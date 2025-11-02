using DAL.Models;
using DAL.Models.Enum;
using DAL.Utils.CryptoUtil;
using Microsoft.EntityFrameworkCore;

namespace DAL.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

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
            modelBuilder.Entity<UserRole>().HasKey(ur => new { ur.UserId, ur.RoleId });

            modelBuilder.Entity<ReviewVote>().HasKey(rv => new { rv.ReviewId, rv.UserId });

            // Configure unique indexes
            modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();

            modelBuilder.Entity<User>().HasIndex(u => u.Phone).IsUnique();

            modelBuilder.Entity<Category>().HasIndex(c => c.Slug).IsUnique();

            modelBuilder.Entity<Supplier>().HasIndex(s => s.Code).IsUnique();

            modelBuilder.Entity<Product>().HasIndex(p => p.Sku).IsUnique();

            modelBuilder.Entity<Product>().HasIndex(p => p.Slug).IsUnique();

            modelBuilder.Entity<Coupon>().HasIndex(c => c.Code).IsUnique();

            modelBuilder.Entity<Order>().HasIndex(o => o.OrderNumber).IsUnique();

            modelBuilder.Entity<Blog>().HasIndex(b => b.Slug).IsUnique();

            // Configure unique constraint for Cart (one cart per user)
            modelBuilder.Entity<Cart>().HasIndex(c => c.UserId).IsUnique();

            // Configure composite unique index for CartItem
            modelBuilder
                .Entity<CartItem>()
                .HasIndex(ci => new { ci.CartId, ci.ProductId })
                .IsUnique();

            // Configure self-referencing relationship for Categories
            modelBuilder
                .Entity<Category>()
                .HasOne(c => c.ParentCategory)
                .WithMany(c => c.SubCategories)
                .HasForeignKey(c => c.ParentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure relationships to prevent cascade delete cycles
            modelBuilder
                .Entity<Order>()
                .HasOne(o => o.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder
                .Entity<Order>()
                .HasOne(o => o.Coupon)
                .WithMany(c => c.Orders)
                .HasForeignKey(o => o.CouponId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder
                .Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder
                .Entity<OrderItem>()
                .HasOne(oi => oi.Product)
                .WithMany(p => p.OrderItems)
                .HasForeignKey(oi => oi.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder
                .Entity<Review>()
                .HasOne(r => r.Product)
                .WithMany(p => p.Reviews)
                .HasForeignKey(r => r.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder
                .Entity<Review>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reviews)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder
                .Entity<Review>()
                .HasOne(r => r.OrderItem)
                .WithMany(oi => oi.Reviews)
                .HasForeignKey(r => r.OrderItemId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder
                .Entity<ReviewReply>()
                .HasOne(rr => rr.Review)
                .WithMany(r => r.ReviewReplies)
                .HasForeignKey(rr => rr.ReviewId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder
                .Entity<ReviewVote>()
                .HasOne(rv => rv.Review)
                .WithMany(r => r.ReviewVotes)
                .HasForeignKey(rv => rv.ReviewId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder
                .Entity<ProductRatingStat>()
                .HasOne(prs => prs.Product)
                .WithOne(p => p.ProductRatingStat)
                .HasForeignKey<ProductRatingStat>(prs => prs.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder
                .Entity<WalletTransaction>()
                .HasOne(wt => wt.Order)
                .WithMany(o => o.WalletTransactions)
                .HasForeignKey(wt => wt.OrderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder
                .Entity<Cart>()
                .HasOne(c => c.User)
                .WithOne(u => u.Cart)
                .HasForeignKey<Cart>(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder
                .Entity<Cart>()
                .HasOne(c => c.Coupon)
                .WithMany(cp => cp.Carts)
                .HasForeignKey(c => c.CouponId)
                .OnDelete(DeleteBehavior.SetNull);

            // Configure CartItem relationships
            modelBuilder
                .Entity<CartItem>()
                .HasOne(ci => ci.Cart)
                .WithMany(c => c.CartItems)
                .HasForeignKey(ci => ci.CartId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder
                .Entity<CartItem>()
                .HasOne(ci => ci.Product)
                .WithMany(p => p.CartItems)
                .HasForeignKey(ci => ci.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure decimal precision
            modelBuilder.Entity<User>().Property(u => u.WalletBalance).HasPrecision(12, 2);

            modelBuilder.Entity<Product>().Property(p => p.CostPrice).HasPrecision(12, 2);

            modelBuilder.Entity<Product>().Property(p => p.SellPrice).HasPrecision(12, 2);

            modelBuilder.Entity<Product>().Property(p => p.Weight).HasPrecision(8, 2);

            modelBuilder.Entity<WalletTransaction>().Property(wt => wt.Amount).HasPrecision(12, 2);

            modelBuilder
                .Entity<WalletTransaction>()
                .Property(wt => wt.BalanceBefore)
                .HasPrecision(12, 2);

            modelBuilder
                .Entity<WalletTransaction>()
                .Property(wt => wt.BalanceAfter)
                .HasPrecision(12, 2);

            modelBuilder.Entity<Order>().Property(o => o.Subtotal).HasPrecision(12, 2);

            modelBuilder.Entity<Order>().Property(o => o.DiscountAmount).HasPrecision(12, 2);

            modelBuilder.Entity<Order>().Property(o => o.ShippingFee).HasPrecision(12, 2);

            modelBuilder.Entity<Order>().Property(o => o.Total).HasPrecision(12, 2);

            modelBuilder.Entity<Order>().Property(o => o.WalletAmountUsed).HasPrecision(12, 2);

            modelBuilder.Entity<OrderItem>().Property(oi => oi.UnitCostPrice).HasPrecision(12, 2);

            modelBuilder.Entity<OrderItem>().Property(oi => oi.UnitSellPrice).HasPrecision(12, 2);

            modelBuilder.Entity<OrderItem>().Property(oi => oi.Total).HasPrecision(12, 2);

            modelBuilder
                .Entity<ProductRatingStat>()
                .Property(prs => prs.AverageRating)
                .HasPrecision(3, 2);

            // Convert enums to strings in database
            modelBuilder.Entity<User>().Property(u => u.Status).HasConversion<string>();

            modelBuilder.Entity<Role>().Property(r => r.RoleName).HasConversion<string>();

            modelBuilder
                .Entity<WalletTransaction>()
                .Property(wt => wt.TransactionType)
                .HasConversion<string>();

            modelBuilder
                .Entity<WalletTransaction>()
                .Property(wt => wt.PaymentGateway)
                .HasConversion<string>();

            modelBuilder
                .Entity<WalletTransaction>()
                .Property(wt => wt.Status)
                .HasConversion<string>();

            modelBuilder
                .Entity<CouponTemplate>()
                .Property(ct => ct.DiscountType)
                .HasConversion<string>();

            modelBuilder.Entity<Coupon>().Property(c => c.DiscountType).HasConversion<string>();

            modelBuilder.Entity<Coupon>().Property(c => c.Source).HasConversion<string>();

            modelBuilder
                .Entity<PointTransaction>()
                .Property(pt => pt.TransactionType)
                .HasConversion<string>();

            modelBuilder
                .Entity<PointTransaction>()
                .Property(pt => pt.ReferenceType)
                .HasConversion<string>();

            modelBuilder.Entity<Order>().Property(o => o.Status).HasConversion<string>();

            modelBuilder.Entity<Order>().Property(o => o.PaymentStatus).HasConversion<string>();

            modelBuilder.Entity<Order>().Property(o => o.PaymentGateway).HasConversion<string>();

            // Allow CancelledReason to be nullable
            modelBuilder.Entity<Order>().Property(o => o.CancelledReason).IsRequired(false);

            modelBuilder.Entity<Notification>().Property(n => n.Type).HasConversion<string>();

            modelBuilder.Entity<Notification>().Property(n => n.Priority).HasConversion<string>();

            modelBuilder.Entity<Banner>().Property(b => b.Position).HasConversion<string>();

            modelBuilder.Entity<Review>().Property(r => r.Status).HasConversion<string>();

            modelBuilder.Entity<Cart>().Property(c => c.Subtotal).HasPrecision(12, 2);

            modelBuilder.Entity<Cart>().Property(c => c.DiscountAmount).HasPrecision(12, 2);

            modelBuilder.Entity<CartItem>().Property(ci => ci.UnitPrice).HasPrecision(12, 2);

            modelBuilder.Entity<CartItem>().Property(ci => ci.Subtotal).HasPrecision(12, 2);

            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            var now = new DateTime(2025, 10, 28, 3, 25, 6, 233, DateTimeKind.Utc);

            // --- Roles ---
            var adminRoleId = 1;
            var customerRoleId = 2;
            var staffRoleId = 3;
            modelBuilder
                .Entity<Role>()
                .HasData(
                    new Role
                    {
                        Id = adminRoleId,
                        RoleName = RoleName.ROLE_ADMIN,
                        CreatedAt = now,
                    },
                    new Role
                    {
                        Id = customerRoleId,
                        RoleName = RoleName.ROLE_CUSTOMER,
                        CreatedAt = now,
                    },
                    new Role
                    {
                        Id = staffRoleId,
                        RoleName = RoleName.ROLE_STAFF,
                        CreatedAt = now,
                    }
                );

            var adminUserId = 1;
            var customerUserId = 2;
            var staffUserId = 3;

            // Hash passwords using HMACSHA512 (fixed salt for seed data)
            // Password: Admin@123
            string adminPasswordHash = HashPasswordSeedData("Admin@123");
            // Password: Customer@123
            string customerPasswordHash = HashPasswordSeedData("Customer@123");
            // Password: Staff@123
            string staffPasswordHash = HashPasswordSeedData("Staff@123");

            modelBuilder
                .Entity<User>()
                .HasData(
                    new User
                    {
                        Id = adminUserId,
                        FullName = "Admin User",
                        Email = "admin@example.com",
                        Phone = "0123456789",
                        Password = adminPasswordHash,
                        Province = "Can Tho",
                        District = "Ninh Kieu",
                        Ward = "An Khanh",
                        SpecificAddress = "123 Admin St",
                        Status = UserStatus.ACTIVE,
                        Points = 0,
                        WalletBalance = 0,
                        CreatedAt = now,
                        UpdatedAt = now,
                        EmailVerifiedAt = now,
                        PhoneVerifiedAt = now,
                    },
                    new User
                    {
                        Id = customerUserId,
                        FullName = "Customer User",
                        Email = "customer@example.com",
                        Phone = "0987654321",
                        Password = customerPasswordHash,
                        Province = "Can Tho",
                        District = "Cai Rang",
                        Ward = "Le Binh",
                        SpecificAddress = "456 Customer Ave",
                        Status = UserStatus.ACTIVE,
                        Points = 100,
                        WalletBalance = 50000,
                        CreatedAt = now,
                        UpdatedAt = now,
                        EmailVerifiedAt = now,
                        PhoneVerifiedAt = now,
                    },
                    new User
                    {
                        Id = staffUserId,
                        FullName = "Staff User",
                        Email = "staff@example.com",
                        Phone = "0111222333",
                        Password = staffPasswordHash,
                        Province = "Can Tho",
                        District = "Ninh Kieu",
                        Ward = "Xuan Khanh",
                        SpecificAddress = "789 Staff Street",
                        Status = UserStatus.ACTIVE,
                        Points = 0,
                        WalletBalance = 0,
                        CreatedAt = now,
                        UpdatedAt = now,
                        EmailVerifiedAt = now,
                        PhoneVerifiedAt = now,
                    }
                );

            // --- UserRoles ---
            modelBuilder
                .Entity<UserRole>()
                .HasData(
                    new UserRole { UserId = adminUserId, RoleId = adminRoleId },
                    new UserRole { UserId = customerUserId, RoleId = customerRoleId },
                    new UserRole { UserId = staffUserId, RoleId = staffRoleId }
                );

            // --- Carts (Một giỏ hàng cho mỗi user) ---
            modelBuilder
                .Entity<Cart>()
                .HasData(
                    new Cart
                    {
                        Id = 1,
                        UserId = adminUserId,
                        Subtotal = 0,
                        DiscountAmount = 0,
                        TotalItems = 0,
                        CreatedAt = now,
                        UpdatedAt = now,
                    },
                    new Cart
                    {
                        Id = 2,
                        UserId = customerUserId,
                        Subtotal = 0,
                        DiscountAmount = 0,
                        TotalItems = 0,
                        CreatedAt = now,
                        UpdatedAt = now,
                    },
                    new Cart
                    {
                        Id = 3,
                        UserId = staffUserId,
                        Subtotal = 0,
                        DiscountAmount = 0,
                        TotalItems = 0,
                        CreatedAt = now,
                        UpdatedAt = now,
                    }
                );

            // --- Suppliers ---
            var supplierId1 = 1;
            modelBuilder
                .Entity<Supplier>()
                .HasData(
                    new Supplier
                    {
                        Id = supplierId1,
                        Name = "Nhà Cung Cấp Vườn Xinh",
                        Code = "VX001",
                        ContactPerson = "Ms. Lan",
                        Email = "vuonxinh@supplier.com",
                        Phone = "02923111222",
                        Address = "Khu Công Nghiệp Trà Nóc, Cần Thơ",
                        TaxCode = "1234567890",
                        PaymentTerms = "30 ngày",
                        IsActive = true,
                        CreatedAt = now,
                        UpdatedAt = now,
                    }
                );

            // --- Categories ---
            var indoorCategoryId = 1;
            var outdoorCategoryId = 2;
            modelBuilder
                .Entity<Category>()
                .HasData(
                    new Category
                    {
                        Id = indoorCategoryId,
                        Name = "Cây Trong Nhà",
                        Slug = "cay-trong-nha",
                        Description = "Các loại cây phù hợp trồng trong nhà, văn phòng.",
                        Image =
                            "https://res.cloudinary.com/dvsqjznt2/image/upload/v1761620174/trong-cay-van-phong-cay-trong-van-phong-dep-1_otymq5.jpg",
                        IsActive = true,
                        SortOrder = 1,
                        ParentId = null,
                        CreatedAt = now,
                        UpdatedAt = now,
                    },
                    new Category
                    {
                        Id = outdoorCategoryId,
                        Name = "Cây Ngoài Trời",
                        Slug = "cay-ngoai-troi",
                        Description = "Các loại cây cảnh, cây ăn quả trồng ngoài trời.",
                        Image =
                            "https://res.cloudinary.com/dvsqjznt2/image/upload/v1761620241/147202426979463-collage_tl9zw4.jpg",
                        IsActive = true,
                        SortOrder = 2,
                        ParentId = null,
                        CreatedAt = now,
                        UpdatedAt = now,
                    }
                );

            // --- Products ---
            var productId1 = 1;
            var productId2 = 2;
            modelBuilder
                .Entity<Product>()
                .HasData(
                    new Product
                    {
                        Id = productId1,
                        Name = "Cây Lưỡi Hổ",
                        Slug = "cay-luoi-ho",
                        Sku = "CLH001",
                        ShortDescription = "Cây phong thủy, lọc không khí tốt.",
                        Description =
                            "Cây Lưỡi Hổ (Sansevieria trifasciata) là loại cây phổ biến, dễ chăm sóc, có khả năng lọc bỏ các độc tố trong không khí.",
                        CareInstructions =
                            "Tưới nước vừa phải khi đất khô. Tránh ánh nắng trực tiếp quá gắt.",
                        PlantSize = "Nhỏ (30-40cm)",
                        Dimensions = "Chậu 15cm",
                        Image =
                            "https://res.cloudinary.com/dvsqjznt2/image/upload/v1761620341/cay-luoi-ho-ten-khoa-hoc-sansevieria-trifasciata_v0kubp.jpg",
                        CostPrice = 200000,
                        SellPrice = 150000,
                        Quantity = 50,
                        PointsEarned = 10,
                        IsActive = true,
                        IsFeatured = true,
                        Weight = 1.5m,
                        Tags = "cay-trong-nha,phong-thuy,loc-khong-khi",
                        SeoTitle = "Mua Cây Lưỡi Hổ - Lọc Không Khí, Dễ Chăm Sóc",
                        SeoDescription =
                            "Cây Lưỡi Hổ đẹp, giá tốt, phù hợp trang trí nhà cửa, văn phòng. Giúp thanh lọc không khí hiệu quả.",
                        CategoryId = indoorCategoryId,
                        SupplierId = supplierId1,
                        CreatedAt = now,
                        UpdatedAt = now,
                    },
                    new Product
                    {
                        Id = productId2,
                        Name = "Cây Phát Tài",
                        Slug = "cay-phat-tai",
                        Sku = "CPT005",
                        ShortDescription = "Mang lại may mắn, tài lộc cho gia chủ.",
                        Description =
                            "Cây Phát Tài (Dracaena fragrans) hay còn gọi là Thiết Mộc Lan, được tin là mang lại may mắn và tài lộc. Cây có sức sống tốt, dễ trồng.",
                        CareInstructions = "Ưa bóng râm, tưới nước 2-3 lần/tuần. Bón phân định kỳ.",
                        PlantSize = "Trung bình (60-80cm)",
                        Dimensions = "Chậu 25cm",
                        Image =
                            "https://res.cloudinary.com/dvsqjznt2/image/upload/v1761620487/ca_CC_81ch-cha_CC_86m-so_CC_81c-ca_CC_82y-pha_CC_81t-ta_CC_80i-de_CC_82_CC_89-trong-nha_CC_80_mmkhjo.jpg",
                        CostPrice = 350000,
                        SellPrice = 250000,
                        Quantity = 30,
                        PointsEarned = 30,
                        IsActive = true,
                        IsFeatured = false,
                        Weight = 3.0m, // decimal?
                        Tags = "cay-trong-nha,phong-thuy,may-man,tai-loc",
                        SeoTitle = "Cây Phát Tài (Thiết Mộc Lan) - Mang May Mắn, Tài Lộc",
                        SeoDescription =
                            "Bán cây Phát Tài hợp phong thủy, trang trí nội thất sang trọng. Cây dễ chăm, mang lại vượng khí.",
                        CategoryId = indoorCategoryId,
                        SupplierId = supplierId1,
                        CreatedAt = now,
                        UpdatedAt = now,
                    }
                );

            // --- ProductImages (Thêm ảnh phụ cho sản phẩm) ---
            modelBuilder
                .Entity<ProductImage>()
                .HasData(
                    new ProductImage
                    {
                        Id = 1,
                        ProductId = productId1,
                        ImageUrl =
                            "https://res.cloudinary.com/dvsqjznt2/image/upload/v1761620780/cay-luoi-ho-2_fxisjk.jpg",
                        AltText = "Ảnh chi tiết cây Lưỡi Hổ",
                        IsPrimary = false,
                        SortOrder = 1,
                        CreatedAt = now,
                    },
                    new ProductImage
                    {
                        Id = 2,
                        ProductId = productId1,
                        ImageUrl =
                            "https://res.cloudinary.com/dvsqjznt2/image/upload/v1761620716/chau-cay-hoa-de-ban-2_apigsc.jpg",
                        AltText = "Chậu cây Lưỡi Hổ",
                        IsPrimary = false,
                        SortOrder = 2,
                        CreatedAt = now,
                    },
                    new ProductImage
                    {
                        Id = 3,
                        ProductId = productId2,
                        ImageUrl =
                            "https://res.cloudinary.com/dvsqjznt2/image/upload/v1761620845/cay-phat-tai-1-goc-dep_u5pi5c.jpg",
                        AltText = "Thân cây Phát Tài",
                        IsPrimary = false,
                        SortOrder = 1,
                        CreatedAt = now,
                    }
                );

            // --- ProductRatingStats (Khởi tạo thống kê đánh giá) ---
            modelBuilder
                .Entity<ProductRatingStat>()
                .HasData(
                    new ProductRatingStat
                    {
                        ProductId = productId1,
                        AverageRating = 0,
                        TotalReviews = 0,
                        Star1Count = 0,
                        Star2Count = 0,
                        Star3Count = 0,
                        Star4Count = 0,
                        Star5Count = 0,
                        WithContentCount = 0,
                        WithMediaCount = 0,
                        LastUpdated = now,
                    },
                    new ProductRatingStat
                    {
                        ProductId = productId2,
                        AverageRating = 0,
                        TotalReviews = 0,
                        Star1Count = 0,
                        Star2Count = 0,
                        Star3Count = 0,
                        Star4Count = 0,
                        Star5Count = 0,
                        WithContentCount = 0,
                        WithMediaCount = 0,
                        LastUpdated = now,
                    }
                );

            // --- CouponTemplates ---
            var couponTemplateId = 1;
            modelBuilder
                .Entity<CouponTemplate>()
                .HasData(
                    new CouponTemplate
                    {
                        Id = couponTemplateId,
                        Name = "Giảm 10% cho đơn hàng trên 500K",
                        Description = "Áp dụng cho đơn hàng tối thiểu 500.000 VNĐ",
                        DiscountType = DiscountType.PERCENT,
                        DiscountValue = 10,
                        MinOrderAmount = 500000,
                        PointsCost = 500,
                        UsageLimitPerUser = 2,
                        TotalUsageLimit = 100,
                        IsActive = true,
                        ValidDays = 30,
                        CreatedAt = now,
                    },
                    new CouponTemplate
                    {
                        Id = 2,
                        Name = "Giảm 100K cho đơn hàng trên 1 triệu",
                        Description = "Áp dụng cho đơn hàng tối thiểu 1.000.000 VNĐ",
                        DiscountType = DiscountType.FIXED_AMOUNT,
                        DiscountValue = 100000,
                        MinOrderAmount = 1000000,
                        PointsCost = 800,
                        UsageLimitPerUser = 1,
                        TotalUsageLimit = 50,
                        IsActive = true,
                        ValidDays = 60,
                        CreatedAt = now,
                    }
                );

            // --- Coupons ---
            var couponId1 = 1;
            var couponId2 = 2;
            modelBuilder
                .Entity<Coupon>()
                .HasData(
                    new Coupon
                    {
                        Id = couponId1,
                        Code = "WELCOME10",
                        TemplateId = couponTemplateId,
                        UserId = null, // Coupon dùng chung
                        Name = "Chào mừng giảm 10%",
                        DiscountType = DiscountType.PERCENT,
                        DiscountValue = 10,
                        MinOrderAmount = 500000,
                        UsageLimit = 100,
                        UsedCount = 0,
                        Source = CouponSource.SYSTEM,
                        PointsUsed = 0,
                        StartDate = now,
                        EndDate = now.AddDays(60),
                        IsActive = true,
                        CreatedAt = now,
                    },
                    new Coupon
                    {
                        Id = couponId2,
                        Code = "VIP100K",
                        TemplateId = 2,
                        UserId = null,
                        Name = "VIP giảm 100K",
                        DiscountType = DiscountType.FIXED_AMOUNT,
                        DiscountValue = 100000,
                        MinOrderAmount = 1000000,
                        UsageLimit = 50,
                        UsedCount = 0,
                        Source = CouponSource.PROMOTION,
                        PointsUsed = 0,
                        StartDate = now,
                        EndDate = now.AddDays(90),
                        IsActive = true,
                        CreatedAt = now,
                    }
                );

            // --- PointEarningRules ---
            modelBuilder
                .Entity<PointEarningRule>()
                .HasData(
                    new PointEarningRule
                    {
                        Id = 1,
                        Name = "Quy tắc tích điểm chuẩn",
                        PointsPerAmount = 1,
                        MinOrderAmount = 0,
                        MaxPointsPerOrder = 1000,
                        IsActive = true,
                        ValidFrom = now,
                        ValidUntil = null,
                        CreatedBy = adminUserId,
                        CreatedAt = now,
                    },
                    new PointEarningRule
                    {
                        Id = 2,
                        Name = "Khuyến mãi tích điểm x2",
                        PointsPerAmount = 2,
                        MinOrderAmount = 1000000,
                        MaxPointsPerOrder = 2000,
                        IsActive = true,
                        ValidFrom = now,
                        ValidUntil = now.AddMonths(3),
                        CreatedBy = adminUserId,
                        CreatedAt = now,
                    }
                );

            // --- Blogs ---
            var blogId1 = 1;
            modelBuilder
                .Entity<Blog>()
                .HasData(
                    new Blog
                    {
                        Id = blogId1,
                        Title = "Cách chăm sóc cây cảnh trong nhà",
                        Slug = "cach-cham-soc-cay-canh-trong-nha",
                        Excerpt =
                            "Hướng dẫn chi tiết cách chăm sóc cây cảnh trong nhà để cây luôn xanh tươi, khỏe mạnh.",
                        Content = "Nội dung chi tiết về cách chăm sóc cây cảnh...",
                        FeaturedImage =
                            "https://res.cloudinary.com/dvsqjznt2/image/upload/v1761618345/cach-cham-soc-cay-xanh-trong-nha-3_jwt3pb.webp",
                        AuthorId = adminUserId,
                        CategoryId = indoorCategoryId,
                        Tags = "cham-soc-cay,cay-trong-nha,meo-hay",
                        ViewCount = 0,
                        IsFeatured = true,
                        IsPublished = true,
                        PublishedAt = now,
                        SeoTitle = "Cách chăm sóc cây cảnh trong nhà - Hướng dẫn chi tiết",
                        SeoDescription =
                            "Tổng hợp những bí quyết chăm sóc cây cảnh trong nhà hiệu quả, giúp không gian sống thêm xanh.",
                        CreatedAt = now,
                        UpdatedAt = now,
                    },
                    new Blog
                    {
                        Id = 2,
                        Title = "Top 10 loại cây phong thủy",
                        Slug = "top-10-loai-cay-phong-thuy",
                        Excerpt =
                            "Khám phá 10 loại cây phong thủy mang lại tài lộc, may mắn cho gia đình.",
                        Content = "Nội dung chi tiết về các loại cây phong thủy...",
                        FeaturedImage =
                            "https://res.cloudinary.com/dvsqjznt2/image/upload/v1761618409/top-19-loai-cay-canh-trong-nha-hop-phong-thuy-va-de-cham-soc-nhat-hien-nay-651645fae15e8c8b38af38ad_flrmui.webp",
                        AuthorId = adminUserId,
                        CategoryId = indoorCategoryId,
                        Tags = "phong-thuy,cay-phong-thuy,tai-loc",
                        ViewCount = 0,
                        IsFeatured = false,
                        IsPublished = true,
                        PublishedAt = now,
                        SeoTitle = "Top 10 cây phong thủy - Mang tài lộc vào nhà",
                        SeoDescription =
                            "Danh sách các loại cây phong thủy nên trồng trong nhà để mang lại may mắn và tài lộc.",
                        CreatedAt = now,
                        UpdatedAt = now,
                    }
                );

            // --- Banners ---
            modelBuilder
                .Entity<Banner>()
                .HasData(
                    new Banner
                    {
                        Id = 1,
                        Title = "Chào mừng đến với GreenTech",
                        Description = "Nền tảng mua sắm cây cảnh số 1 Việt Nam",
                        ImageUrl =
                            "https://res.cloudinary.com/dvsqjznt2/image/upload/v1761619562/Screenshot_2081_gnwkty.png",
                        LinkUrl = "/#",
                        Position = BannerPosition.HOME_SLIDER,
                        SortOrder = 1,
                        IsActive = true,
                        StartDate = now,
                        EndDate = now.AddMonths(6),
                        ClickCount = 0,
                        CreatedBy = adminUserId,
                        CreatedAt = now,
                        UpdatedAt = now,
                    },
                    new Banner
                    {
                        Id = 2,
                        Title = "Giảm giá 20% cho đơn đầu tiên",
                        Description = "Áp dụng cho khách hàng mới",
                        ImageUrl =
                            "https://res.cloudinary.com/dvsqjznt2/image/upload/v1761618541/Baner_m0uvff.jpg",
                        LinkUrl = "/#",
                        Position = BannerPosition.HOME_SLIDER,
                        SortOrder = 2,
                        IsActive = true,
                        StartDate = now,
                        EndDate = now.AddMonths(3),
                        ClickCount = 0,
                        CreatedBy = adminUserId,
                        CreatedAt = now,
                        UpdatedAt = now,
                    }
                );

            // --- Notifications ---
            modelBuilder
                .Entity<Notification>()
                .HasData(
                    new Notification
                    {
                        Id = 1,
                        UserId = customerUserId,
                        Title = "Chào mừng đến với GreenTech",
                        Message = "Cảm ơn bạn đã đăng ký tài khoản! Bạn có 10.000 điểm thưởng.",
                        Type = NotificationType.SYSTEM,
                        Priority = NotificationPriority.MEDIUM,
                        IsRead = false,
                        ReadAt = null,
                        ReferenceId = null,
                        CreatedAt = now,
                    }
                );

            // --- Suppliers (Thêm một vài suppliers nữa) ---
            var supplierId2 = 2;
            var supplierId3 = 3;
            modelBuilder
                .Entity<Supplier>()
                .HasData(
                    new Supplier
                    {
                        Id = supplierId2,
                        Name = "Công ty Cây Xanh ABC",
                        Code = "CXABC001",
                        ContactPerson = "Mr. Hai",
                        Email = "cayxanh@abc.com",
                        Phone = "02923888999",
                        Address = "12 Đường 3/2, Quận Ninh Kiều, TP. Cần Thơ",
                        TaxCode = "9876543210",
                        PaymentTerms = "60 ngày",
                        IsActive = true,
                        CreatedAt = now,
                        UpdatedAt = now,
                    },
                    new Supplier
                    {
                        Id = supplierId3,
                        Name = "Nhà Vườn Sinh Thái",
                        Code = "NVST001",
                        ContactPerson = "Ms. Mai",
                        Email = "sinhthai@nhavuon.com",
                        Phone = "02923555777",
                        Address = "Đường 30/4, Quận Cai Rang, TP. Cần Thơ",
                        TaxCode = "5555555555",
                        PaymentTerms = "45 ngày",
                        IsActive = true,
                        CreatedAt = now,
                        UpdatedAt = now,
                    }
                );

            // --- Products (Thêm thêm products) ---
            var productId3 = 3;
            var productId4 = 4;
            modelBuilder
                .Entity<Product>()
                .HasData(
                    new Product
                    {
                        Id = productId3,
                        Name = "Cây Trầu Bà",
                        Slug = "cay-trau-ba",
                        Sku = "CTB003",
                        ShortDescription = "Cây lọc không khí tuyệt vời, dễ trồng.",
                        Description =
                            "Cây Trầu Bà (Epipremnum aureum) là một trong những cây lọc không khí tốt nhất. Cây dễ chăm sóc, phù hợp cho người mới bắt đầu.",
                        CareInstructions =
                            "Tưới nước khi đất khô. Không cần ánh sáng trực tiếp. Bón phân mỗi tháng một lần.",
                        PlantSize = "Nhỏ (20-30cm)",
                        Dimensions = "Chậu 12cm",
                        Image =
                            "https://res.cloudinary.com/dvsqjznt2/image/upload/v1761619808/images_vm4li5.jpg",
                        CostPrice = 150000,
                        SellPrice = 80000,
                        Quantity = 80,
                        PointsEarned = 8,
                        IsActive = true,
                        IsFeatured = false,
                        Weight = 0.8m,
                        Tags = "cay-trong-nha,loc-khong-khi,de-cham",
                        SeoTitle = "Mua Cây Trầu Bà - Lọc Không Khí Hiệu Quả",
                        SeoDescription =
                            "Cây Trầu Bà đẹp, dễ chăm, giúp thanh lọc không khí trong nhà hiệu quả.",
                        CategoryId = indoorCategoryId,
                        SupplierId = supplierId2,
                        CreatedAt = now,
                        UpdatedAt = now,
                    },
                    new Product
                    {
                        Id = productId4,
                        Name = "Cây Đa Búp Đỏ",
                        Slug = "cay-da-bup-do",
                        Sku = "CDB004",
                        ShortDescription = "Cây để bàn làm việc đẹp mắt.",
                        Description =
                            "Cây Đa Búp Đỏ (Ficus elastica) với lá bóng, xanh mướt, mang lại không gian tươi mát cho phòng làm việc.",
                        CareInstructions =
                            "Tưới nước 1-2 lần/tuần. Nên đặt nơi có ánh sáng gián tiếp. Lau lá thường xuyên.",
                        PlantSize = "Nhỏ (25-35cm)",
                        Dimensions = "Chậu 15cm",
                        Image =
                            "https://res.cloudinary.com/dvsqjznt2/image/upload/v1761619886/Cay-da-bup-do_p1whyu.png",
                        CostPrice = 150000,
                        SellPrice = 100000,
                        Quantity = 60,
                        PointsEarned = 10,
                        IsActive = true,
                        IsFeatured = true,
                        Weight = 1.2m,
                        Tags = "cay-de-ban,tin-cay,xanh-mat",
                        SeoTitle = "Cây Đa Búp Đỏ - Trang Trí Văn Phòng",
                        SeoDescription =
                            "Cây Đa Búp Đỏ đẹp, dễ chăm, phù hợp trang trí bàn làm việc, phòng khách.",
                        CategoryId = indoorCategoryId,
                        SupplierId = supplierId3,
                        CreatedAt = now,
                        UpdatedAt = now,
                    }
                );

            // --- Products (Cây Ngoài Trời) ---
            var productId5 = 5;
            var productId6 = 6;
            var productId7 = 7;
            var productId8 = 8;
            modelBuilder
                .Entity<Product>()
                .HasData(
                    new Product
                    {
                        Id = productId5,
                        Name = "Cây Hoa Giấy",
                        Slug = "cay-hoa-giay",
                        Sku = "CHG005",
                        ShortDescription = "Cây hoa giấy đẹp rực rỡ, trồng ngoài trời.",
                        Description =
                            "Cây Hoa Giấy (Bougainvillea) là loại cây leo có hoa đẹp rực rỡ, nhiều màu sắc. Cây thích hợp trồng ngoài trời, chịu nắng tốt, dễ chăm sóc và có thể cắt tỉa tạo dáng đẹp.",
                        CareInstructions =
                            "Tưới nước đều đặn, tránh úng. Cần ánh sáng đầy đủ, bón phân định kỳ để cây ra hoa nhiều. Cắt tỉa sau mỗi đợt hoa.",
                        PlantSize = "Trung bình (80-120cm)",
                        Dimensions = "Chậu 30cm",
                        Image =
                            "https://res.cloudinary.com/dvsqjznt2/image/upload/v1761983435/cay-hoa-giay-canh-nhieu-mau_gpageh.jpg",
                        CostPrice = 400000,
                        SellPrice = 300000,
                        Quantity = 40,
                        PointsEarned = 30,
                        IsActive = true,
                        IsFeatured = true,
                        Weight = 5.0m,
                        Tags = "cay-ngoai-troi,hoa-dep,leo,trang-tri",
                        SeoTitle = "Mua Cây Hoa Giấy - Cây Leo Đẹp, Nhiều Màu Sắc",
                        SeoDescription =
                            "Cây Hoa Giấy đẹp rực rỡ, nhiều màu, dễ trồng ngoài trời. Phù hợp trang trí sân vườn, ban công.",
                        CategoryId = outdoorCategoryId,
                        SupplierId = supplierId1,
                        CreatedAt = now,
                        UpdatedAt = now,
                    },
                    new Product
                    {
                        Id = productId6,
                        Name = "Cây Mai Vàng",
                        Slug = "cay-mai-vang",
                        Sku = "CMV006",
                        ShortDescription = "Cây mai vàng truyền thống, chưng Tết.",
                        Description =
                            "Cây Mai Vàng (Ochna integerrima) là loại cây cảnh truyền thống của Việt Nam, đặc biệt phổ biến vào dịp Tết. Cây có hoa vàng đẹp, mang ý nghĩa may mắn, tài lộc.",
                        CareInstructions =
                            "Cần ánh sáng đầy đủ, tưới nước vừa phải. Bón phân định kỳ. Cắt tỉa sau Tết để cây phát triển tốt. Cần chăm sóc đặc biệt trước Tết để hoa nở đúng thời điểm.",
                        PlantSize = "Lớn (100-150cm)",
                        Dimensions = "Chậu 40cm",
                        Image =
                            "https://res.cloudinary.com/dvsqjznt2/image/upload/v1761983794/cay-hoa-mai-vang_qhbl3q.jpg",
                        CostPrice = 800000,
                        SellPrice = 600000,
                        Quantity = 25,
                        PointsEarned = 60,
                        IsActive = true,
                        IsFeatured = true,
                        Weight = 8.0m,
                        Tags = "cay-ngoai-troi,mai-vang,tet,phong-thuy",
                        SeoTitle = "Mua Cây Mai Vàng - Chưng Tết, Mang May Mắn",
                        SeoDescription =
                            "Cây Mai Vàng đẹp, hợp phong thủy, chưng Tết truyền thống. Mang lại may mắn, tài lộc cho gia đình.",
                        CategoryId = outdoorCategoryId,
                        SupplierId = supplierId2,
                        CreatedAt = now,
                        UpdatedAt = now,
                    },
                    new Product
                    {
                        Id = productId7,
                        Name = "Cây Chuối Cảnh",
                        Slug = "cay-chuoi-canh",
                        Sku = "CCC007",
                        ShortDescription = "Cây chuối cảnh nhiệt đới, xanh mát.",
                        Description =
                            "Cây Chuối Cảnh (Musa spp.) có lá to, xanh mướt tạo cảnh quan nhiệt đới đẹp mắt. Cây phù hợp trồng ngoài sân vườn, tạo điểm nhấn cho không gian xanh.",
                        CareInstructions =
                            "Tưới nước thường xuyên, đặc biệt vào mùa khô. Cần ánh sáng đầy đủ hoặc bán bóng. Bón phân hữu cơ định kỳ. Cắt bỏ lá già để cây đẹp hơn.",
                        PlantSize = "Lớn (120-180cm)",
                        Dimensions = "Chậu 50cm",
                        Image =
                            "https://res.cloudinary.com/dvsqjznt2/image/upload/v1761984589/hoa-lua-180-366280e2-3c49-4bba-8f3f-c9a9cb3cda71_l5tzes.jpg",
                        CostPrice = 500000,
                        SellPrice = 350000,
                        Quantity = 35,
                        PointsEarned = 35,
                        IsActive = true,
                        IsFeatured = false,
                        Weight = 10.0m,
                        Tags = "cay-ngoai-troi,nhiet-doi,xanh-mat,trang-tri-san",
                        SeoTitle = "Cây Chuối Cảnh - Tạo Không Gian Nhiệt Đới Xanh Mát",
                        SeoDescription =
                            "Cây Chuối Cảnh đẹp, xanh mướt, phù hợp trang trí sân vườn. Tạo không gian nhiệt đới sinh động.",
                        CategoryId = outdoorCategoryId,
                        SupplierId = supplierId3,
                        CreatedAt = now,
                        UpdatedAt = now,
                    },
                    new Product
                    {
                        Id = productId8,
                        Name = "Cây Tre Cảnh",
                        Slug = "cay-tre-canh",
                        Sku = "CTC008",
                        ShortDescription = "Tre cảnh thanh tao, phong thủy tốt.",
                        Description =
                            "Cây Tre Cảnh (Bambusa spp.) mang vẻ đẹp thanh tao, cổ điển. Theo phong thủy, tre mang lại may mắn, thịnh vượng và bảo vệ gia đình. Cây dễ trồng, chịu được nhiều điều kiện khí hậu.",
                        CareInstructions =
                            "Tưới nước đều đặn, không để khô hoàn toàn. Cần ánh sáng tốt. Bón phân định kỳ. Kiểm soát sâu bệnh đặc biệt là mối. Cắt tỉa thân già để cây trẻ đẹp hơn.",
                        PlantSize = "Trung bình (100-150cm)",
                        Dimensions = "Chậu 35cm",
                        Image =
                            "https://res.cloudinary.com/dvsqjznt2/image/upload/v1761984743/cay-tre-3-638371520309931454_mgmsko.webp",
                        CostPrice = 450000,
                        SellPrice = 320000,
                        Quantity = 30,
                        PointsEarned = 32,
                        IsActive = true,
                        IsFeatured = false,
                        Weight = 7.0m,
                        Tags = "cay-ngoai-troi,tre-canh,phong-thuy,thanh-tao",
                        SeoTitle = "Cây Tre Cảnh - Phong Thủy, Thanh Tao, May Mắn",
                        SeoDescription =
                            "Cây Tre Cảnh đẹp thanh tao, hợp phong thủy. Mang lại may mắn, thịnh vượng cho gia đình. Dễ trồng, dễ chăm sóc.",
                        CategoryId = outdoorCategoryId,
                        SupplierId = supplierId1,
                        CreatedAt = now,
                        UpdatedAt = now,
                    }
                );

            // --- ProductRatingStats (Thêm cho products mới) ---
            modelBuilder
                .Entity<ProductRatingStat>()
                .HasData(
                    new ProductRatingStat
                    {
                        ProductId = productId3,
                        AverageRating = 0,
                        TotalReviews = 0,
                        Star1Count = 0,
                        Star2Count = 0,
                        Star3Count = 0,
                        Star4Count = 0,
                        Star5Count = 0,
                        WithContentCount = 0,
                        WithMediaCount = 0,
                        LastUpdated = now,
                    },
                    new ProductRatingStat
                    {
                        ProductId = productId4,
                        AverageRating = 0,
                        TotalReviews = 0,
                        Star1Count = 0,
                        Star2Count = 0,
                        Star3Count = 0,
                        Star4Count = 0,
                        Star5Count = 0,
                        WithContentCount = 0,
                        WithMediaCount = 0,
                        LastUpdated = now,
                    },
                    new ProductRatingStat
                    {
                        ProductId = productId5,
                        AverageRating = 0,
                        TotalReviews = 0,
                        Star1Count = 0,
                        Star2Count = 0,
                        Star3Count = 0,
                        Star4Count = 0,
                        Star5Count = 0,
                        WithContentCount = 0,
                        WithMediaCount = 0,
                        LastUpdated = now,
                    },
                    new ProductRatingStat
                    {
                        ProductId = productId6,
                        AverageRating = 0,
                        TotalReviews = 0,
                        Star1Count = 0,
                        Star2Count = 0,
                        Star3Count = 0,
                        Star4Count = 0,
                        Star5Count = 0,
                        WithContentCount = 0,
                        WithMediaCount = 0,
                        LastUpdated = now,
                    },
                    new ProductRatingStat
                    {
                        ProductId = productId7,
                        AverageRating = 0,
                        TotalReviews = 0,
                        Star1Count = 0,
                        Star2Count = 0,
                        Star3Count = 0,
                        Star4Count = 0,
                        Star5Count = 0,
                        WithContentCount = 0,
                        WithMediaCount = 0,
                        LastUpdated = now,
                    },
                    new ProductRatingStat
                    {
                        ProductId = productId8,
                        AverageRating = 0,
                        TotalReviews = 0,
                        Star1Count = 0,
                        Star2Count = 0,
                        Star3Count = 0,
                        Star4Count = 0,
                        Star5Count = 0,
                        WithContentCount = 0,
                        WithMediaCount = 0,
                        LastUpdated = now,
                    }
                );

            // --- ProductImages (Thêm cho products mới) ---
            modelBuilder
                .Entity<ProductImage>()
                .HasData(
                    new ProductImage
                    {
                        Id = 4,
                        ProductId = productId3,
                        ImageUrl =
                            "https://res.cloudinary.com/dvsqjznt2/image/upload/v1761620927/trau-ba-mini_jdnmjk.jpg",
                        AltText = "Cây Trầu Bà nhỏ xinh",
                        IsPrimary = false,
                        SortOrder = 1,
                        CreatedAt = now,
                    },
                    new ProductImage
                    {
                        Id = 5,
                        ProductId = productId4,
                        ImageUrl =
                            "https://res.cloudinary.com/dvsqjznt2/image/upload/v1761620976/cay-da-bup-do_b7k0h6.jpg",
                        AltText = "Cây Đa Búp Đỏ đẹp mắt",
                        IsPrimary = false,
                        SortOrder = 1,
                        CreatedAt = now,
                    },
                    new ProductImage
                    {
                        Id = 6,
                        ProductId = productId5,
                        ImageUrl =
                            "https://res.cloudinary.com/dvsqjznt2/image/upload/v1761983631/hoa-giay-5_p7inkl.jpg",
                        AltText = "Hoa giấy nhiều màu sắc",
                        IsPrimary = false,
                        SortOrder = 1,
                        CreatedAt = now,
                    },
                    new ProductImage
                    {
                        Id = 7,
                        ProductId = productId6,
                        ImageUrl =
                            "https://res.cloudinary.com/dvsqjznt2/image/upload/v1761986192/A_CC_89nh-C_C3_A2y-Mai-300x300_fey4bn.jpg",
                        AltText = "Cây mai vàng nở hoa",
                        IsPrimary = false,
                        SortOrder = 1,
                        CreatedAt = now,
                    },
                    new ProductImage
                    {
                        Id = 8,
                        ProductId = productId7,
                        ImageUrl =
                            "https://res.cloudinary.com/dvsqjznt2/image/upload/v1761984662/chuoi-canh3jpg_vbbfsa.jpg",
                        AltText = "Chuối cảnh trang trí sân vườn",
                        IsPrimary = false,
                        SortOrder = 1,
                        CreatedAt = now,
                    },
                    new ProductImage
                    {
                        Id = 9,
                        ProductId = productId8,
                        ImageUrl =
                            "https://res.cloudinary.com/dvsqjznt2/image/upload/v1761984792/cay-tre-canh_grande_b49xkr.jpg",
                        AltText = "Tre cảnh thanh tao",
                        IsPrimary = false,
                        SortOrder = 1,
                        CreatedAt = now,
                    }
                );
        }

        /// <summary>
        /// Hash password for seed data with fixed salt (deterministic)
        /// </summary>
        private string HashPasswordSeedData(string password)
        {
            if (string.IsNullOrEmpty(password))
                return null;

            // Fixed salt for seed data (deterministic)
            string fixedSaltBase64 = "Vm9pY2VzQW5kRnVyaWVz";
            byte[] salt = Convert.FromBase64String(fixedSaltBase64);
            string key = "GreenTech2024!@#$%^&*()SecretKey";

            // Combine password with salt
            string dataWithSalt = $"{password}{fixedSaltBase64}";

            // Compute HMAC-SHA512
            string hmacHash = CryptoUtil.HMacBase64Encode(CryptoUtil.HMACSHA512, key, dataWithSalt);

            // Return salt:hash format
            return $"{fixedSaltBase64}:{hmacHash}";
        }
    }
}
