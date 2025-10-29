using BLL.Service.Interface;
using DAL.DTOs.Review;
using DAL.Models;
using DAL.Repositories.Interface;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BLL.Service
{
    public class ReviewService : IReviewService
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly IFileStorageService _fileStorageService;

        public ReviewService(IReviewRepository reviewRepository, IFileStorageService fileStorageService)
        {
            _reviewRepository = reviewRepository;
            _fileStorageService = fileStorageService;
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
                return false;

            return await _reviewRepository.DeleteReviewAsync(existing);
        }

        // 🚀 Upload Review Media
        public async Task<Review?> UploadReviewMediaAsync(int reviewId, int userId, List<IFormFile> files)
        {
            var review = await _reviewRepository.GetReviewByIdAsync(reviewId);
            if (review == null || review.UserId != userId)
                return null;

            if (files == null || files.Count == 0)
                throw new ArgumentException("No files provided for upload.");

            var uploadedUrls = new List<string>();

            foreach (var file in files)
            {
                // subFolder đặt tên để dễ quản lý trên Cloudinary
                var url = await _fileStorageService.SaveFileAsync(file, "reviews");
                uploadedUrls.Add(url);
            }

            // Gộp URL cũ + mới
            var currentUrls = string.IsNullOrEmpty(review.MediaUrls)
                ? new List<string>()
                : review.MediaUrls.Split(';').ToList();

            currentUrls.AddRange(uploadedUrls);
            review.MediaUrls = string.Join(';', currentUrls);
            review.UpdatedAt = DateTime.Now;

            await _reviewRepository.UpdateReviewAsync(review);
            return review;
        }
    }
}
