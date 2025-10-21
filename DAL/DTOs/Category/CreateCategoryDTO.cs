using System.ComponentModel.DataAnnotations;

namespace DAL.DTOs.Category
{
    public class CreateCategoryDTO
    {
        [Required(ErrorMessage = "Tên danh mục là bắt buộc")]
        [MaxLength(255, ErrorMessage = "Tên danh mục không được vượt quá 255 ký tự")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Slug là bắt buộc")]
        [MaxLength(255, ErrorMessage = "Slug không được vượt quá 255 ký tự")]
        [RegularExpression(@"^[a-z0-9]+(?:-[a-z0-9]+)*$",
            ErrorMessage = "Slug chỉ được chứa chữ thường, số và dấu gạch ngang")]
        public string Slug { get; set; }

        public int? ParentId { get; set; }

        [Required(ErrorMessage = "Hình ảnh là bắt buộc")]
        [MaxLength(255)]
        public string Image { get; set; }

        [Required(ErrorMessage = "Mô tả là bắt buộc")]
        public string Description { get; set; }

        public bool IsActive { get; set; } = true;

        [Range(0, int.MaxValue, ErrorMessage = "Thứ tự sắp xếp phải >= 0")]
        public int SortOrder { get; set; } = 0;
    }
}
