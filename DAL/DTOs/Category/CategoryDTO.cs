using System.ComponentModel.DataAnnotations;

namespace DAL.DTOs.Category
{
    public class CategoryDTO
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên danh mục là bắt buộc")]
        [MaxLength(255, ErrorMessage = "Tên danh mục không được vượt quá 255 ký tự")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Slug là bắt buộc")]
        [MaxLength(255, ErrorMessage = "Slug không được vượt quá 255 ký tự")]
        public string Slug { get; set; }

        public int? ParentId { get; set; }

        [Required(ErrorMessage = "Hình ảnh là bắt buộc")]
        [MaxLength(255)]
        public string Image { get; set; }

        [Required(ErrorMessage = "Mô tả là bắt buộc")]
        public string Description { get; set; }

        public bool IsActive { get; set; }

        public int SortOrder { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        // Navigation properties
        public string ParentCategoryName { get; set; }
        public int SubCategoriesCount { get; set; }
        public int ProductsCount { get; set; }
    }
}
