using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BLL.Service.ReviewReply.Interface;
using DAL.DTOs.ReviewReply;
using DAL.Repositories.ReviewReply.Interface;
using ReviewReplyModel = DAL.Models.ReviewReply;

namespace BLL.Service.ReviewReply
{
    public class ReviewReplyService : IReviewReplyService
    {
        private readonly IReviewReplyRepository _replyRepository;

        public ReviewReplyService(IReviewReplyRepository replyRepository)
        {
            _replyRepository = replyRepository;
        }

        public async Task<ReviewReplyModel> CreateReplyAsync(CreateReviewReplyDTO dto)
        {
            var reply = new ReviewReplyModel
            {
                ReviewId = dto.ReviewId,
                UserId = dto.UserId,
                Content = dto.Content,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
            };
            return await _replyRepository.CreateReplyAsync(reply);
        }

        public async Task<IEnumerable<ReviewReplyModel>> GetRepliesByReviewIdAsync(int reviewId)
        {
            return await _replyRepository.GetRepliesByReviewIdAsync(reviewId);
        }

        public async Task<bool> DeleteReplyAsync(int replyId, int userId)
        {
            return await _replyRepository.DeleteReplyAsync(replyId, userId);
        }
        public async Task<ReviewReplyModel?> UpdateReplyAsync(int replyId, int userId, string content)
        {
            return await _replyRepository.UpdateReplyAsync(replyId, userId, content);
        }
    }
}
