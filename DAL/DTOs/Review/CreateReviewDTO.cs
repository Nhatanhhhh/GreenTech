using System.ComponentModel.DataAnnotations;

namespace DAL.DTOs.Review
{
    public class CreateReviewDTO
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int OrderItemId { get; set; }

        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

        public string? Content { get; set; }

        // Danh sách link ảnh/video, có thể null
        public string? MediaUrls { get; set; }

        public bool IsAnonymous { get; set; } = false;
    }
}
