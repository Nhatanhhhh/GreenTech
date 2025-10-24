using BLL.Service.Interface;
using DAL.Context;
using DAL.Models;
using DAL.Repositories.Interface;
using System;
using System.Linq;

namespace BLL.Service
{
    public class ReviewService : IReviewService
    {
        private readonly IReviewRepository _reviewRepo;
        private readonly AppDbContext _context;

        public ReviewService(IReviewRepository reviewRepo, AppDbContext context)
        {
            _reviewRepo = reviewRepo;
            _context = context;
        }

        private bool IsCustomerRole(int userId)
        {
            var roleName = (from ur in _context.UserRoles
                            join r in _context.Roles on ur.RoleId equals r.Id
                            where ur.UserId == userId
                            select r.RoleName.ToString())
                            .FirstOrDefault();

            return roleName == "ROLE_CUSTOMER";
        }

        public bool CreateReview(int userId, int productId, string content, int rating)
        {
            if (!IsCustomerRole(userId)) return false;
            if (string.IsNullOrEmpty(content) || rating < 1 || rating > 5)
                return false;

            var review = new Review
            {
                UserId = userId,
                ProductId = productId,
                Content = content,
                Rating = rating,
                CreatedAt = DateTime.Now
            };

            return _reviewRepo.AddReview(review);
        }

        public bool UpdateReview(int userId, int reviewId, string content, int rating)
        {
            if (!IsCustomerRole(userId)) return false;
            if (string.IsNullOrEmpty(content) || rating < 1 || rating > 5)
                return false;

            return _reviewRepo.UpdateReview(reviewId, content, rating);
        }

        public bool DeleteReview(int userId, int reviewId)
        {
            if (!IsCustomerRole(userId)) return false;
            if (reviewId <= 0) return false;

            return _reviewRepo.DeleteReview(reviewId);
        }
        public bool UploadReviewMedia(int userId, int reviewId, List<string> mediaUrls)
        {
            if (!IsCustomerRole(userId)) return false;
            if (mediaUrls == null || mediaUrls.Count == 0 || mediaUrls.Count > 5)
                return false;

            return _reviewRepo.UploadReviewMedia(reviewId, mediaUrls);
        }
    }
}
