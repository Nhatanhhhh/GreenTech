using DAL.DTOs.CouponTemplate;
using DAL.DTOs.User;
using DAL.Models;
using DAL.Models.Enum;
using System.Globalization;

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

        private static string FormatDateTime(DateTime dateTime)
        {
            return dateTime.ToString("dd-MM-yyyy HH:mm");
        }

        private static DateTime ParseDateTime(string dateTimeString)
        {
            return DateTime.ParseExact(dateTimeString, "dd-MM-yyyy HH:mm", CultureInfo.InvariantCulture);
        }
    }
}