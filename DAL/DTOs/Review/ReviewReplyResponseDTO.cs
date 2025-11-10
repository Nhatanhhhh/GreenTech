using DAL.Models.Enum;

namespace DAL.DTOs.Review
{
    public class ReviewReplyResponseDTO
    {
        public int Id { get; set; }
        public int ReviewId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string UserAvatar { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<string> UserRoles { get; set; } = new List<string>();
    }
}
