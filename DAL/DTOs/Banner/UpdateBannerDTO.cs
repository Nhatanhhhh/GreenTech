using System.ComponentModel.DataAnnotations;

namespace DAL.DTOs.Banner
{
    public class UpdateBannerDTO
    {
        [Required(ErrorMessage = "Tiêu đề là bắt buộc")]
        [MaxLength(255, ErrorMessage = "Tiêu đề không được vượt quá 255 ký tự")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Mô tả là bắt buộc")]
        [MaxLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự")]
        public string Description { get; set; }

        [Required(ErrorMessage = "URL hình ảnh là bắt buộc")]
        [MaxLength(500)]
        public string ImageUrl { get; set; }

        [Required(ErrorMessage = "URL liên kết là bắt buộc")]
        [MaxLength(500)]
        [Url(ErrorMessage = "URL liên kết không hợp lệ")]
        public string LinkUrl { get; set; }

        [Required(ErrorMessage = "Vị trí là bắt buộc")]
        public string Position { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Thứ tự sắp xếp phải >= 0")]
        public int SortOrder { get; set; }

        public bool IsActive { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }
    }
}
