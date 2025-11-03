using DAL.Models.Enum;

namespace DAL.DTOs.Review
{
    public class ReviewResponseDTO
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string UserAvatar { get; set; }
        public int OrderItemId { get; set; }
        public int Rating { get; set; }
        public string Content { get; set; }
        public List<string> MediaUrls { get; set; } = new List<string>();
        public int HelpfulCount { get; set; }
        public bool IsAnonymous { get; set; }
        public ReviewStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<ReviewReplyResponseDTO> Replies { get; set; } =
            new List<ReviewReplyResponseDTO>();
        public bool HasUserVoted { get; set; }
        public bool? IsHelpful { get; set; } // null if not voted, true/false if voted
    }
}
