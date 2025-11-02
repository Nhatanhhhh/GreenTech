using System.ComponentModel.DataAnnotations;

namespace DAL.DTOs.Review
{
    public class UpdateReviewDTO
    {
        [Required]
        public int Id { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; }

        public string? Content { get; set; }

        public string? MediaUrls { get; set; }

        public bool IsAnonymous { get; set; } = false;
    }
}