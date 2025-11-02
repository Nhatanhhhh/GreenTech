using System;
using System.Threading.Tasks;
using DAL.Context;
using DAL.Models.Enum;
using DAL.Repositories.Review.Interface;
using Microsoft.EntityFrameworkCore;
using ReviewModel = DAL.Models.Review;

namespace DAL.Repositories.Review
{
    public class ReviewRepository : IReviewRepository
    {
        private readonly AppDbContext _context;

        public ReviewRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ReviewModel> CreateReviewAsync(ReviewModel review)
        {
            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();
            return review;
        }

        public async Task<ReviewModel?> GetReviewByIdAsync(int id)
        {
            return await _context.Reviews.FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<ReviewModel?> UpdateReviewAsync(ReviewModel review)
        {
            _context.Reviews.Update(review);
            await _context.SaveChangesAsync();
            return review;
        }

        public async Task<bool> DeleteReviewAsync(ReviewModel review)
        {
            _context.Reviews.Remove(review);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<ReviewModel?> ToggleReviewStatusAsync(int id)
        {
            var review = await _context.Reviews.FirstOrDefaultAsync(r => r.Id == id);
            if (review == null)
                return null;

            review.Status =
                review.Status == ReviewStatus.APPROVED
                    ? ReviewStatus.HIDDEN
                    : ReviewStatus.APPROVED;

            review.UpdatedAt = DateTime.Now;
            _context.Reviews.Update(review);
            await _context.SaveChangesAsync();

            return review;
        }

        public async Task<IEnumerable<ReviewModel>> GetAllReviewsAsync()
        {
            return await _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Product)
                .Include(r => r.OrderItem)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }
    }
}