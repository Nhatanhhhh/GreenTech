using BLL.Service.Interface;
using DAL.Models;
using DAL.Repositories.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Service
{
    public class ReviewService : IReviewService
    {
        private readonly IReviewRepository _reviewRepo;

        public ReviewService(IReviewRepository reviewRepo)
        {
            _reviewRepo = reviewRepo;
        }

        public bool CreateReview(int userId, int productId, string content, int rating)
        {
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
    }
}