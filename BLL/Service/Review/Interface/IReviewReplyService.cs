using System.Collections.Generic;
using System.Threading.Tasks;
using DAL.DTOs.ReviewReply;
using ReviewReplyModel = DAL.Models.ReviewReply;

namespace BLL.Service.ReviewReply.Interface
{
    public interface IReviewReplyService
    {
        Task<ReviewReplyModel> CreateReplyAsync(CreateReviewReplyDTO dto);
        Task<IEnumerable<ReviewReplyModel>> GetRepliesByReviewIdAsync(int reviewId);
        Task<bool> DeleteReplyAsync(int replyId, int userId);
    }
}
