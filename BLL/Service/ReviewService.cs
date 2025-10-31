using BLL.Service.Interface;
using DAL.DTOs.Review;
using DAL.Models;
using DAL.Models.Enum;
using DAL.Repositories.Interface;
using System;
using System.Threading.Tasks;

namespace BLL.Service
{
    public class ReviewService : IReviewService
    {
        private readonly IReviewRepository _reviewRepository;

        public ReviewService(IReviewRepository reviewRepository)
        {
            _reviewRepository = reviewRepository;
        }

        public async Task<Review> CreateReviewAsync(CreateReviewDTO dto)
        {
            var review = new Review
            {
                ProductId = dto.ProductId,
                UserId = dto.UserId,
                OrderItemId = dto.OrderItemId,
                Rating = dto.Rating,
                Content = dto.Content,
                MediaUrls = dto.MediaUrls,
                IsAnonymous = dto.IsAnonymous,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            return await _reviewRepository.CreateReviewAsync(review);
        }

        public async Task<Review?> UpdateReviewAsync(UpdateReviewDTO dto, int userId)
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
        public async Task<IEnumerable<Review>> GetAllReviewsAsync()
        {
            return await _reviewRepository.GetAllReviewsAsync();
        }
    }
}