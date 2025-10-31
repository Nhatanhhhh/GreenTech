using System;
using System.Threading.Tasks;
using BLL.Service.Review.Interface;
using DAL.DTOs.Review;
using DAL.Models.Enum;
using DAL.Repositories.Review.Interface;
using ReviewModel = DAL.Models.Review;

namespace BLL.Service.Review
{
    public class ReviewService : IReviewService
    {
        private readonly IReviewRepository _reviewRepository;

        public ReviewService(IReviewRepository reviewRepository)
        {
            _reviewRepository = reviewRepository;
        }

        public async Task<ReviewModel> CreateReviewAsync(CreateReviewDTO dto)
        {
            var review = DAL.Utils.AutoMapper.AutoMapper.ToReview(dto);
            return await _reviewRepository.CreateReviewAsync(review);
        }

        public async Task<ReviewModel?> UpdateReviewAsync(UpdateReviewDTO dto, int userId)
        {
            var existing = await _reviewRepository.GetReviewByIdAsync(dto.Id);
            if (existing == null || existing.UserId != userId)
                return null;

            existing.Rating = dto.Rating;
            existing.Content = dto.Content;
            existing.MediaUrls = dto.MediaUrls;
            existing.IsAnonymous = dto.IsAnonymous;
            existing.UpdatedAt = DateTime.Now;

            return await _reviewRepository.UpdateReviewAsync(existing);
        }

        public async Task<bool> DeleteReviewAsync(int reviewId, int userId)
        {
            var existing = await _reviewRepository.GetReviewByIdAsync(reviewId);
            if (existing == null || existing.UserId != userId)
                return false; // Không có quyền hoặc không tồn tại

            return await _reviewRepository.DeleteReviewAsync(existing);
        }

        public async Task<ReviewModel?> ToggleReviewStatusAsync(int id)
        {
            return await _reviewRepository.ToggleReviewStatusAsync(id);
        }
    }
}
