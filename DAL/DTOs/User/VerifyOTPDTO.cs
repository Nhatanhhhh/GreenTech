using System.ComponentModel.DataAnnotations;

namespace DAL.DTOs.User
{
    public class VerifyOTPDTO
    {
        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "OTP là bắt buộc")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "OTP phải có đúng 6 chữ số")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "OTP chỉ được chứa 6 chữ số")]
        [Display(Name = "Mã OTP")]
        public string OTP { get; set; }
    }
}
