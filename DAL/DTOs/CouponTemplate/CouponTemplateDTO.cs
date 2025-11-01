using System.ComponentModel.DataAnnotations;
using DAL.Models.Enum;
using DAL.ValidationAttributes;

namespace DAL.DTOs.CouponTemplate
{
    public class CouponTemplateDTO
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên template là bắt buộc")]
        [StringLength(255, ErrorMessage = "Tên không được vượt quá 255 ký tự")]
        public string Name { get; set; }

        public string Description { get; set; }

        [Required(ErrorMessage = "Loại giảm giá là bắt buộc")]
        public DiscountType DiscountType { get; set; }

        [Required(ErrorMessage = "Giá trị giảm giá là bắt buộc")]
        [DiscountValue]
        public decimal DiscountValue { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Giá trị đơn hàng tối thiểu không được âm")]
        public decimal MinOrderAmount { get; set; } = 0;

        [Required(ErrorMessage = "Chi phí điểm là bắt buộc")]
        [Range(1, int.MaxValue, ErrorMessage = "Chi phí điểm phải lớn hơn 0")]
        public int PointsCost { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Giới hạn sử dụng phải lớn hơn 0")]
        public int UsageLimitPerUser { get; set; } = 1;

        [Range(1, int.MaxValue, ErrorMessage = "Tổng giới hạn sử dụng phải lớn hơn 0")]
        public int? TotalUsageLimit { get; set; }

        public bool IsActive { get; set; } = true;

        [Range(1, 365, ErrorMessage = "Số ngày hợp lệ phải từ 1 đến 365")]
        public int ValidDays { get; set; } = 30;

        public DateTime CreatedAt { get; set; }
    }
}
