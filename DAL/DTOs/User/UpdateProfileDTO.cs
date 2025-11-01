using System.ComponentModel.DataAnnotations;
using DAL.ValidationAttributes;

namespace DAL.DTOs.User
{
    public class UpdateProfileDTO
    {
        /// <summary>
        /// UserId for excluding current user from uniqueness validation (set by service, not displayed)
        /// </summary>
        [ScaffoldColumn(false)]
        public int? UserId { get; set; }

        [Required(ErrorMessage = "Họ tên là bắt buộc")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Họ tên phải từ 2 đến 100 ký tự")]
        [Display(Name = "Họ và tên")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [StringLength(255, ErrorMessage = "Email không được vượt quá 255 ký tự")]
        [UniqueEmail(
            ExcludeUserIdProperty = "UserId",
            ErrorMessage = "Email đã được sử dụng bởi tài khoản khác"
        )]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [StringLength(
            15,
            MinimumLength = 10,
            ErrorMessage = "Số điện thoại phải từ 10 đến 15 ký tự"
        )]
        [RegularExpression(
            @"^(\+84|0)[1-9]\d{8,9}$",
            ErrorMessage = "Số điện thoại phải theo định dạng Việt Nam (+84 hoặc 0)"
        )]
        [UniquePhone(
            ExcludeUserIdProperty = "UserId",
            ErrorMessage = "Số điện thoại đã được sử dụng bởi tài khoản khác"
        )]
        [Display(Name = "Số điện thoại")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Tỉnh/Thành phố là bắt buộc")]
        [Display(Name = "Tỉnh/Thành phố")]
        public string Province { get; set; }

        [Required(ErrorMessage = "Quận/Huyện là bắt buộc")]
        [Display(Name = "Quận/Huyện")]
        public string District { get; set; }

        [Required(ErrorMessage = "Phường/Xã là bắt buộc")]
        [Display(Name = "Phường/Xã")]
        public string Ward { get; set; }

        [StringLength(150, ErrorMessage = "Địa chỉ cụ thể không được vượt quá 150 ký tự")]
        [Display(Name = "Địa chỉ cụ thể")]
        public string? SpecificAddress { get; set; }
    }
}
