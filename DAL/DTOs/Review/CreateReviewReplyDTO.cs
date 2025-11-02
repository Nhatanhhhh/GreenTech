using System.ComponentModel.DataAnnotations;

namespace DAL.DTOs.ReviewReply
{
    public class CreateReviewReplyDTO
    {
        [Required]
        public int ReviewId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public string Content { get; set; }
    }
}
