using DAL.DTOs.Banner;
using DAL.DTOs.Blog;
using DAL.DTOs.Category;
using DAL.DTOs.CouponTemplate;
using DAL.DTOs.Product;
using DAL.DTOs.ProductImage;
using DAL.DTOs.Supplier;
using DAL.DTOs.User;
using DAL.Models;
using DAL.Models.Enum;

namespace DAL.Utils.AutoMapper
{
    public class AutoMapper
    {
        /// <summary>
        /// Convert User to UserResponseDTO
        /// </summary>
        /// <param name="user"></param>
        /// <returns>Mapp to UserResponseDTO</returns>
        public static UserResponseDTO ToUserResponseDTO(User user)
        {
            return new UserResponseDTO
            {
                Id = user.Id,
                FullName = FormatFullname(user.FullName),
                Email = user.Email,
                Phone = user.Phone,
                Province = user.Province,
                District = user.District,
                Ward = user.Ward,
                SpecificAddress = user.SpecificAddress,
                Avatar = user.Avatar,
                Points = user.Points,
                WalletBalance = user.WalletBalance,
                Status = user.Status.ToString()
            };
        }

        /// <summary>
        /// Convert RegisterDTO to User
        /// </summary>
        /// <param name="registerDTO"></param>
        /// <returns>Mapping to User</returns>
        public static User ToUser(RegisterDTO registerDTO)
        {
            return new User
            {
                FullName = registerDTO.FullName,
                Email = registerDTO.Email.ToLower(),
                Password = registerDTO.Password, // Will be hashed in service layer
                Phone = registerDTO.Phone,
                Province = registerDTO.Province,
                District = registerDTO.District,
                Ward = registerDTO.Ward,
                SpecificAddress = registerDTO.SpecificAddress,
                Status = UserStatus.ACTIVE,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
        }

        /// <summary>
        /// Create UserRole for user
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="roleId"></param>
        /// <returns>UserRole entity</returns>
        public static UserRole ToUserRole(int userId, int roleId)
        {
            return new UserRole
            {
                UserId = userId,
                RoleId = roleId
            };
        }

        /// <summary>
        /// Create Cart for user
        /// </summary>
        /// <param name="userId"></param>
        /// <returns>Cart entity</returns>
        public static Cart ToCart(int userId)
        {
            return new Cart
            {
                UserId = userId,
                Subtotal = 0,
                DiscountAmount = 0,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
        }

        /// <summary>
        /// Convert User to UserResponseDTO with roles
        /// </summary>
        /// <param name="user"></param>
        /// <param name="roles"></param>
        /// <returns>UserResponseDTO with roles</returns>
        public static UserResponseDTO ToUserResponseDTO(User user, List<string> roles)
        {
            var userDTO = ToUserResponseDTO(user);
            return userDTO;
        }

        /// <summary>
        /// Convert CouponTemplate to CouponTemplateDTO
        /// </summary>
        public static CouponTemplateDTO ToCouponTemplateDTO(CouponTemplate template)
        {
            return new CouponTemplateDTO
            {
                Id = template.Id,
                Name = template.Name,
                Description = template.Description,
                DiscountType = template.DiscountType,
                DiscountValue = template.DiscountValue,
                MinOrderAmount = template.MinOrderAmount,
                PointsCost = template.PointsCost,
                UsageLimitPerUser = template.UsageLimitPerUser,
                TotalUsageLimit = template.TotalUsageLimit,
                IsActive = template.IsActive,
                ValidDays = template.ValidDays,
                CreatedAt = template.CreatedAt
            };
        }

        /// <summary>
        /// Convert CreateCouponTemplateDTO to CouponTemplate
        /// </summary>
        public static CouponTemplate ToCouponTemplate(CreateCouponTemplateDTO createDto)
        {
            return new CouponTemplate
            {
                Name = createDto.Name,
                Description = createDto.Description,
                DiscountType = createDto.DiscountType,
                DiscountValue = createDto.DiscountValue,
                MinOrderAmount = createDto.MinOrderAmount,
                PointsCost = createDto.PointsCost,
                UsageLimitPerUser = createDto.UsageLimitPerUser,
                TotalUsageLimit = createDto.TotalUsageLimit,
                IsActive = createDto.IsActive,
                ValidDays = createDto.ValidDays,
                CreatedAt = DateTime.Now
            };
        }

        /// <summary>
        /// Convert CouponTemplateDTO to CouponTemplate
        /// </summary>
        public static CouponTemplate ToCouponTemplate(CouponTemplateDTO dto)
        {
            return new CouponTemplate
            {
                Id = dto.Id,
                Name = dto.Name,
                Description = dto.Description,
                DiscountType = dto.DiscountType,
                DiscountValue = dto.DiscountValue,
                MinOrderAmount = dto.MinOrderAmount,
                PointsCost = dto.PointsCost,
                UsageLimitPerUser = dto.UsageLimitPerUser,
                TotalUsageLimit = dto.TotalUsageLimit,
                IsActive = dto.IsActive,
                ValidDays = dto.ValidDays,
                CreatedAt = dto.CreatedAt
            };
        }

        public static CategoryDTO ToCategoryDTO(Category category)
        {
            if (category == null) return null;

            return new CategoryDTO
            {
                Id = category.Id,
                Name = category.Name,
                Slug = category.Slug,
                ParentId = category.ParentId,
                Image = category.Image,
                Description = category.Description,
                IsActive = category.IsActive,
                SortOrder = category.SortOrder,
                CreatedAt = category.CreatedAt,
                UpdatedAt = category.UpdatedAt,
                ParentCategoryName = category.ParentCategory?.Name,
                SubCategoriesCount = category.SubCategories?.Count ?? 0,
                ProductsCount = category.Products?.Count ?? 0
            };
        }

        public static Category ToCategory(CreateCategoryDTO createDto)
        {
            if (createDto == null) return null;

            return new Category
            {
                Name = createDto.Name,
                Slug = createDto.Slug,
                ParentId = createDto.ParentId,
                Image = createDto.Image,
                Description = createDto.Description,
                IsActive = createDto.IsActive,
                SortOrder = createDto.SortOrder,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
        }

        public static Category ToCategory(UpdateCategoryDTO updateDto)
        {
            if (updateDto == null) return null;

            return new Category
            {
                Name = updateDto.Name,
                Slug = updateDto.Slug,
                ParentId = updateDto.ParentId,
                Image = updateDto.Image,
                Description = updateDto.Description,
                IsActive = updateDto.IsActive,
                SortOrder = updateDto.SortOrder,
                UpdatedAt = DateTime.Now
            };
        }

        public static CategoryTreeDTO ToCategoryTreeDTO(Category category)
        {
            if (category == null) return null;

            return new CategoryTreeDTO
            {
                Id = category.Id,
                Name = category.Name,
                Slug = category.Slug,
                Image = category.Image,
                IsActive = category.IsActive,
                SortOrder = category.SortOrder,
                ProductsCount = category.Products?.Count ?? 0,
                SubCategories = category.SubCategories?
                    .OrderBy(c => c.SortOrder)
                    .ThenBy(c => c.Name)
                    .Select(ToCategoryTreeDTO)
                    .ToList() ?? new List<CategoryTreeDTO>()
            };
        }

        /// <summary>
        /// Convert Banner entity to BannerDTO
        /// </summary>
        public static BannerDTO ToBannerDTO(Banner banner)
        {
            if (banner == null) return null;

            return new BannerDTO
            {
                Id = banner.Id,
                Title = banner.Title,
                Description = banner.Description,
                ImageUrl = banner.ImageUrl,
                LinkUrl = banner.LinkUrl,
                Position = banner.Position.ToString(),
                SortOrder = banner.SortOrder,
                IsActive = banner.IsActive,
                StartDate = banner.StartDate,
                EndDate = banner.EndDate,
                ClickCount = banner.ClickCount,
                CreatedBy = banner.CreatedBy,
                CreatedAt = banner.CreatedAt,
                UpdatedAt = banner.UpdatedAt,
            };
        }

        /// <summary>
        /// Convert IEnumerable<Banner> to IEnumerable<BannerDTO>
        /// </summary>
        public static IEnumerable<BannerDTO> ToBannerDTOs(IEnumerable<Banner> banners)
        {
            // Sử dụng LINQ Select để gọi ToBannerDTO cho từng banner
            return banners?.Select(ToBannerDTO) ?? Enumerable.Empty<BannerDTO>();
        }

        /// <summary>
        /// Convert CreateBannerDTO to Banner entity
        /// </summary>
        public static Banner ToBanner(CreateBannerDTO createDto)
        {
            if (createDto == null) return null;

            if (!Enum.TryParse<BannerPosition>(createDto.Position, true, out var bannerPosition))
            {
                throw new ArgumentException($"Invalid Banner Position value: {createDto.Position}");
            }

            return new Banner
            {
                Title = createDto.Title,
                Description = createDto.Description,
                ImageUrl = createDto.ImageUrl,
                LinkUrl = createDto.LinkUrl,
                Position = bannerPosition,
                SortOrder = createDto.SortOrder,
                IsActive = createDto.IsActive,
                StartDate = createDto.StartDate,
                EndDate = createDto.EndDate,
            };
        }

        /// <summary>
        /// Apply updates from UpdateBannerDTO to an existing Banner entity
        /// </summary>
        /// <param name="updateDto">DTO containing updated values</param>
        /// <param name="existingBanner">The entity to update</param>
        public static void ApplyUpdatesToBanner(UpdateBannerDTO updateDto, Banner existingBanner)
        {
            if (updateDto == null || existingBanner == null) return;

            if (!Enum.TryParse<BannerPosition>(updateDto.Position, true, out var bannerPosition))
            {
                throw new ArgumentException($"Invalid Banner Position value: {updateDto.Position}");
            }

            existingBanner.Title = updateDto.Title;
            existingBanner.Description = updateDto.Description;
            existingBanner.ImageUrl = updateDto.ImageUrl;
            existingBanner.LinkUrl = updateDto.LinkUrl;
            existingBanner.Position = bannerPosition;
            existingBanner.SortOrder = updateDto.SortOrder;
            existingBanner.IsActive = updateDto.IsActive;
            existingBanner.StartDate = updateDto.StartDate;
            existingBanner.EndDate = updateDto.EndDate;
        }

        public static IEnumerable<CategoryDTO> ToCategoryDTOs(IEnumerable<Category> categories)
        {
            return categories?.Select(ToCategoryDTO) ?? Enumerable.Empty<CategoryDTO>();
        }

        public static IEnumerable<CategoryTreeDTO> ToCategoryTreeDTOs(IEnumerable<Category> categories)
        {
            return categories?.Select(ToCategoryTreeDTO) ?? Enumerable.Empty<CategoryTreeDTO>();
        }

        /// <summary>
        /// Converts a Product entity to a ProductResponseDTO.
        /// </summary>
        public static ProductResponseDTO ToProductResponseDTO(Product product)
        {
            if (product == null) return null;

            return new ProductResponseDTO
            {
                Id = product.Id,
                Sku = product.Sku,
                Name = product.Name,
                Slug = product.Slug,
                Description = product.Description,
                ShortDescription = product.ShortDescription,
                CategoryId = product.CategoryId,
                CategoryName = product.Category?.Name,
                SupplierId = product.SupplierId,
                SupplierName = product.Supplier?.Name,
                CostPrice = product.CostPrice,
                SellPrice = product.SellPrice,
                Quantity = product.Quantity,
                Image = product.Image,
                CareInstructions = product.CareInstructions,
                PlantSize = product.PlantSize,
                Weight = product.Weight,
                Dimensions = product.Dimensions,
                Tags = product.Tags,
                PointsEarned = product.PointsEarned,
                IsFeatured = product.IsFeatured,
                IsActive = product.IsActive,
                ProductImages = product.ProductImages?.Select(ToProductImageDTO).ToList() ?? new List<ProductImageDTO>()
            };
        }

        /// <summary>
        /// Converts an IEnumerable of Product entities to a list of ProductResponseDTOs.
        /// </summary>
        public static IEnumerable<ProductResponseDTO> ToProductResponseDTOs(IEnumerable<Product> products)
        {
            return products?.Select(ToProductResponseDTO) ?? Enumerable.Empty<ProductResponseDTO>();
        }

        /// <summary>
        /// Converts a CreateProductDTO to a Product entity.
        /// </summary>
        public static Product ToProduct(CreateProductDTO createDto)
        {
            if (createDto == null) return null;

            return new Product
            {
                Sku = createDto.Sku,
                Name = createDto.Name,
                Description = createDto.Description,
                ShortDescription = createDto.ShortDescription,
                CategoryId = createDto.CategoryId,
                SupplierId = createDto.SupplierId,
                CostPrice = createDto.CostPrice,
                SellPrice = createDto.SellPrice,
                Quantity = createDto.Quantity,
                CareInstructions = createDto.CareInstructions,
                PlantSize = createDto.PlantSize,
                Weight = createDto.Weight,
                Dimensions = createDto.Dimensions,
                Tags = createDto.Tags,
                PointsEarned = createDto.PointsEarned,
                IsFeatured = createDto.IsFeatured,
                IsActive = createDto.IsActive,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Applies updates from an UpdateProductDTO to an existing Product entity.
        /// </summary>
        public static void ApplyUpdatesToProduct(UpdateProductDTO updateDto, Product existingProduct)
        {
            if (updateDto == null || existingProduct == null) return;

            existingProduct.Name = updateDto.Name;
            existingProduct.Description = updateDto.Description;
            existingProduct.ShortDescription = updateDto.ShortDescription;
            existingProduct.CategoryId = updateDto.CategoryId;
            existingProduct.SupplierId = updateDto.SupplierId;
            existingProduct.CostPrice = updateDto.CostPrice;
            existingProduct.SellPrice = updateDto.SellPrice;
            existingProduct.Quantity = updateDto.Quantity;
            existingProduct.CareInstructions = updateDto.CareInstructions;
            existingProduct.PlantSize = updateDto.PlantSize;
            existingProduct.Weight = updateDto.Weight;
            existingProduct.Dimensions = updateDto.Dimensions;
            existingProduct.Tags = updateDto.Tags;
            existingProduct.PointsEarned = updateDto.PointsEarned;
            existingProduct.IsFeatured = updateDto.IsFeatured;
            existingProduct.IsActive = updateDto.IsActive;
            existingProduct.UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Converts a ProductImage entity to a ProductImageDTO.
        /// </summary>
        public static ProductImageDTO ToProductImageDTO(ProductImage image)
        {
            if (image == null) return null;

            return new ProductImageDTO
            {
                Id = image.Id,
                ImageUrl = image.ImageUrl,
                AltText = image.AltText,
                IsPrimary = image.IsPrimary
            };
        }

        /// <summary>
        /// Converts a Supplier entity to a SupplierDTO.
        /// </summary>
        public static SupplierDTO ToSupplierDTO(Supplier supplier)
        {
            if (supplier == null) return null;

            return new SupplierDTO
            {
                Id = supplier.Id,
                Name = supplier.Name,
                Code = supplier.Code,
                ContactPerson = supplier.ContactPerson,
                Phone = supplier.Phone,
                Email = supplier.Email,
                Address = supplier.Address,
                TaxCode = supplier.TaxCode,
                PaymentTerms = supplier.PaymentTerms,
                IsActive = supplier.IsActive,
                CreatedAt = supplier.CreatedAt,
                UpdatedAt = supplier.UpdatedAt
            };
        }

        /// <summary>
        /// Converts a list of Supplier entities to a list of SupplierDTOs.
        /// </summary>
        public static IEnumerable<SupplierDTO> ToSupplierDTOs(IEnumerable<Supplier> suppliers)
        {
            return suppliers?.Select(ToSupplierDTO) ?? Enumerable.Empty<SupplierDTO>();
        }

        /// <summary>
        /// Converts a CreateSupplierDTO to a Supplier entity.
        /// </summary>
        public static Supplier ToSupplier(CreateSupplierDTO createDto)
        {
            if (createDto == null) return null;

            return new Supplier
            {
                Name = createDto.Name,
                Code = createDto.Code,
                ContactPerson = createDto.ContactPerson,
                Phone = createDto.Phone,
                Email = createDto.Email,
                Address = createDto.Address,
                TaxCode = createDto.TaxCode,
                PaymentTerms = createDto.PaymentTerms,
                IsActive = createDto.IsActive,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Applies updates from an UpdateSupplierDTO to an existing Supplier entity.
        /// </summary>
        public static void ApplyUpdatesToSupplier(UpdateSupplierDTO updateDto, Supplier existingSupplier)
        {
            if (updateDto == null || existingSupplier == null) return;

            existingSupplier.Name = updateDto.Name;
            existingSupplier.ContactPerson = updateDto.ContactPerson;
            existingSupplier.Phone = updateDto.Phone;
            existingSupplier.Email = updateDto.Email;
            existingSupplier.Address = updateDto.Address;
            existingSupplier.TaxCode = updateDto.TaxCode;
            existingSupplier.PaymentTerms = updateDto.PaymentTerms;
            existingSupplier.IsActive = updateDto.IsActive;
            existingSupplier.UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Converts a Blog entity to a BlogResponseDTO.
        /// </summary>
        public static BlogResponseDTO ToBlogResponseDTO(Blog blog)
        {
            if (blog == null) return null;

            return new BlogResponseDTO
            {
                Id = blog.Id,
                Title = blog.Title,
                Slug = blog.Slug,
                Excerpt = blog.Excerpt,
                Content = blog.Content,
                FeaturedImage = blog.FeaturedImage,
                AuthorId = blog.AuthorId,
                AuthorName = blog.Author?.FullName,
                CategoryId = blog.CategoryId,
                CategoryName = blog.Category?.Name,
                Tags = blog.Tags,
                ViewCount = blog.ViewCount,
                IsFeatured = blog.IsFeatured,
                IsPublished = blog.IsPublished,
                PublishedAt = blog.PublishedAt,
                CreatedAt = blog.CreatedAt,
                UpdatedAt = blog.UpdatedAt
            };
        }

        /// <summary>
        /// Converts a list of Blog entities to a list of BlogResponseDTOs.
        /// </summary>
        public static IEnumerable<BlogResponseDTO> ToBlogResponseDTOs(IEnumerable<Blog> blogs)
        {
            return blogs?.Select(ToBlogResponseDTO) ?? Enumerable.Empty<BlogResponseDTO>();
        }

        /// <summary>
        /// Converts a CreateBlogDTO to a Blog entity.
        /// </summary>
        public static Blog ToBlog(CreateBlogDTO createDto, int authorId)
        {
            if (createDto == null) return null;

            return new Blog
            {
                Title = createDto.Title,
                Excerpt = createDto.Excerpt,
                Content = createDto.Content,
                AuthorId = authorId,
                CategoryId = createDto.CategoryId,
                Tags = createDto.Tags,
                IsFeatured = createDto.IsFeatured,
                IsPublished = createDto.IsPublished,
                SeoTitle = createDto.SeoTitle,
                SeoDescription = createDto.SeoDescription,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Applies updates from an UpdateBlogDTO to an existing Blog entity.
        /// </summary>
        public static void ApplyUpdatesToBlog(UpdateBlogDTO updateDto, Blog existingBlog)
        {
            if (updateDto == null || existingBlog == null) return;

            existingBlog.Title = updateDto.Title;
            existingBlog.Excerpt = updateDto.Excerpt;
            existingBlog.Content = updateDto.Content;
            existingBlog.CategoryId = updateDto.CategoryId;
            existingBlog.Tags = updateDto.Tags;
            existingBlog.IsFeatured = updateDto.IsFeatured;
            existingBlog.IsPublished = updateDto.IsPublished;
            existingBlog.SeoTitle = updateDto.SeoTitle;
            existingBlog.SeoDescription = updateDto.SeoDescription;
            existingBlog.UpdatedAt = DateTime.UtcNow;
        }

        private static string FormatFullname(string fullname)
        {
            if (string.IsNullOrWhiteSpace(fullname))
            {
                return fullname;
            }

            var words = fullname.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < words.Length; i++)
            {
                if (words[i].Length > 0)
                {
                    words[i] = char.ToUpper(words[i][0]) + words[i].Substring(1).ToLower();
                }
            }

            return string.Join(' ', words);
        }
    }
}