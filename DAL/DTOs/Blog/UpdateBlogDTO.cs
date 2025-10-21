using System.ComponentModel.DataAnnotations;

namespace DAL.DTOs.Blog
{
    public class UpdateBlogDTO
    {
        [Required(ErrorMessage = "Tiêu đề bài viết là bắt buộc.")]
        [StringLength(255, ErrorMessage = "Tiêu đề không được vượt quá 255 ký tự.")]
        public string Title { get; set; }

        [StringLength(500, ErrorMessage = "Mô tả ngắn không được vượt quá 500 ký tự.")]
        public string Excerpt { get; set; }

        [Required(ErrorMessage = "Nội dung bài viết là bắt buộc.")]
        public string Content { get; set; }

        public int? CategoryId { get; set; }

        [StringLength(500, ErrorMessage = "Tags không được vượt quá 500 ký tự.")]
        public string Tags { get; set; }

        public bool IsFeatured { get; set; }
        public bool IsPublished { get; set; }

        [StringLength(255)]
        public string SeoTitle { get; set; }

        [StringLength(500)]
        public string SeoDescription { get; set; }
    }
}
