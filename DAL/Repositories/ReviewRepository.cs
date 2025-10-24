using DAL.Context;
using DAL.Models;
using DAL.Repositories.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Repositories
{
    public class ReviewRepository : IReviewRepository
    {
        private readonly AppDbContext _context;

        public ReviewRepository(AppDbContext context)
        {
            _context = context;
        }

        public bool AddReview(Review review)
        {
            try
            {
                _context.Reviews.Add(review);
                _context.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }
        public bool UpdateReview(int reviewId, string content, int rating)
        {
            try
            {
                var review = _context.Reviews.FirstOrDefault(r => r.Id == reviewId);
                if (review == null) return false;

                review.Content = content;
                review.Rating = rating;
                review.UpdatedAt = DateTime.Now;

                _context.Reviews.Update(review);
                _context.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }
        public bool DeleteReview(int reviewId)
        {
            try
            {
                var review = _context.Reviews.FirstOrDefault(r => r.Id == reviewId);
                if (review == null) return false;

                _context.Reviews.Remove(review);
                _context.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }
        public bool UploadReviewMedia(int reviewId, List<string> mediaUrls)
        {
            try
            {
                var review = _context.Reviews.FirstOrDefault(r => r.Id == reviewId);
                if (review == null) return false;
                if (mediaUrls == null || mediaUrls.Count == 0 || mediaUrls.Count > 5)
                    return false;
                review.MediaUrls = string.Join(";", mediaUrls);
                review.UpdatedAt = DateTime.Now;
                _context.Reviews.Update(review);
                _context.SaveChanges();
                return true;
            }
            catch
            {
                return false;
            }
        }

    }
}