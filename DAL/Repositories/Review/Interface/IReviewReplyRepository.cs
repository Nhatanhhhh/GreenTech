using System.Collections.Generic;
using System.Threading.Tasks;
using ReviewReplyModel = DAL.Models.ReviewReply;

namespace DAL.Repositories.ReviewReply.Interface
{
    public interface IReviewReplyRepository
    {
        Task<ReviewReplyModel> CreateReplyAsync(ReviewReplyModel reply);
        Task<IEnumerable<ReviewReplyModel>> GetRepliesByReviewIdAsync(int reviewId);
        Task<bool> DeleteReplyAsync(int replyId, int userId);
    }
}
