using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DAL.Context;
using DAL.Repositories.ReviewReply.Interface;
using Microsoft.EntityFrameworkCore;
using ReviewReplyModel = DAL.Models.ReviewReply;

namespace DAL.Repositories.ReviewReply
{
    public class ReviewReplyRepository : IReviewReplyRepository
    {
        private readonly AppDbContext _context;

        public ReviewReplyRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ReviewReplyModel> CreateReplyAsync(ReviewReplyModel reply)
        {
            _context.ReviewReplies.Add(reply);
            await _context.SaveChangesAsync();
            return reply;
        }

        public async Task<IEnumerable<ReviewReplyModel>> GetRepliesByReviewIdAsync(int reviewId)
        {
            return await _context
                .ReviewReplies.Include(r => r.User)
                .ThenInclude(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .Where(r => r.ReviewId == reviewId)
                .OrderBy(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> DeleteReplyAsync(int replyId, int userId)
        {
            var reply = await _context.ReviewReplies.FirstOrDefaultAsync(r =>
                r.Id == replyId && r.UserId == userId
            );

            if (reply == null)
                return false;

            _context.ReviewReplies.Remove(reply);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<ReviewReplyModel?> UpdateReplyAsync(
            int replyId,
            int userId,
            string content
        )
        {
            var reply = await _context.ReviewReplies.FirstOrDefaultAsync(r =>
                r.Id == replyId && r.UserId == userId
            );

            if (reply == null)
                return null;

            reply.Content = content;
            reply.UpdatedAt = DateTime.Now;

            _context.ReviewReplies.Update(reply);
            await _context.SaveChangesAsync();

            return reply;
        }
    }
}
