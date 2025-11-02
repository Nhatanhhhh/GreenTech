using System;
using System.Threading.Tasks;
using BLL.Service.Review.Interface;
using DAL.DTOs.Review;
using DAL.Models.Enum;
using DAL.Repositories.Review.Interface;
using ReviewModel = DAL.Models.Review;
using Microsoft.AspNetCore.Http;
using BLL.Service.Cloudinary.Interface;

namespace BLL.Service.Review
{
    public class ReviewService : IReviewService
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly IFileStorageService _fileStorageService;

        public ReviewService(IReviewRepository reviewRepository, IFileStorageService fileStorageService)
        {
            _reviewRepository = reviewRepository ?? throw new ArgumentNullException(nameof(reviewRepository));
            _fileStorageService = fileStorageService ?? throw new ArgumentNullException(nameof(fileStorageService));
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
                return false;

            return await _reviewRepository.DeleteReviewAsync(existing);
        }

        public async Task<ReviewModel?> ToggleReviewStatusAsync(int id)
        {
            return await _reviewRepository.ToggleReviewStatusAsync(id);
        }

        public async Task<IEnumerable<ReviewModel>> GetAllReviewsAsync()
        {
            return await _reviewRepository.GetAllReviewsAsync();
        }

        public async Task<string> UploadReviewMediaAsync(int reviewId, IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File cannot be null or empty.");

            var review = await _reviewRepository.GetReviewByIdAsync(reviewId);
            if (review == null)
                throw new KeyNotFoundException($"Review with ID {reviewId} not found.");

            var mediaUrl = await _fileStorageService.SaveFileAsync(file, "reviews");

            if (string.IsNullOrEmpty(review.MediaUrls))
                review.MediaUrls = mediaUrl;
            else
                review.MediaUrls += $",{mediaUrl}";

            review.UpdatedAt = DateTime.Now;
            await _reviewRepository.UpdateReviewAsync(review);

            return mediaUrl;
        }
    }
}