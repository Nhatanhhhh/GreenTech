using System.ComponentModel.DataAnnotations;

namespace DAL.DTOs.User
{
    public class RegisterDTO
    {
        [Required(ErrorMessage = "Họ tên là bắt buộc")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Họ tên phải từ 2 đến 100 ký tự")]
        [Display(Name = "Họ và tên")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [StringLength(255, ErrorMessage = "Email không được vượt quá 255 ký tự")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        [StringLength(255, MinimumLength = 6, ErrorMessage = "Mật khẩu phải từ 6 đến 255 ký tự")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{6,}$",
            ErrorMessage = "Mật khẩu phải chứa ít nhất 1 chữ hoa, 1 chữ thường, 1 số và 1 ký tự đặc biệt")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Xác nhận mật khẩu là bắt buộc")]
        [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        [DataType(DataType.Password)]
        [Display(Name = "Xác nhận mật khẩu")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [StringLength(15, MinimumLength = 10, ErrorMessage = "Số điện thoại phải từ 10 đến 15 ký tự")]
        [RegularExpression(@"^(\+84|0)[1-9]\d{8,9}$",
            ErrorMessage = "Số điện thoại phải theo định dạng Việt Nam (+84 hoặc 0)")]
        [Display(Name = "Số điện thoại")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Tỉnh/Thành phố là bắt buộc")]
        [StringLength(150, MinimumLength = 2, ErrorMessage = "Tỉnh/Thành phố phải từ 2 đến 150 ký tự")]
        [Display(Name = "Tỉnh/Thành phố")]
        public string Province { get; set; }

        [Required(ErrorMessage = "Quận/Huyện là bắt buộc")]
        [StringLength(150, MinimumLength = 2, ErrorMessage = "Quận/Huyện phải từ 2 đến 150 ký tự")]
        [Display(Name = "Quận/Huyện")]
        public string District { get; set; }

        [Required(ErrorMessage = "Phường/Xã là bắt buộc")]
        [StringLength(150, MinimumLength = 2, ErrorMessage = "Phường/Xã phải từ 2 đến 150 ký tự")]
        [Display(Name = "Phường/Xã")]
        public string Ward { get; set; }

        [StringLength(150, ErrorMessage = "Địa chỉ cụ thể không được vượt quá 150 ký tự")]
        [Display(Name = "Địa chỉ cụ thể")]
        public string? SpecificAddress { get; set; }

        // Optional fields with validation
        [Url(ErrorMessage = "URL avatar không hợp lệ")]
        [StringLength(255, ErrorMessage = "URL avatar không được vượt quá 255 ký tự")]
        [Display(Name = "Avatar")]
        public string? Avatar { get; set; }

        // Agreement checkbox (thường dùng trong frontend)
        [Range(typeof(bool), "true", "true", ErrorMessage = "Bạn phải đồng ý với điều khoản sử dụng")]
        [Display(Name = "Đồng ý điều khoản")]
        public bool AgreeToTerms { get; set; }
    }
}
