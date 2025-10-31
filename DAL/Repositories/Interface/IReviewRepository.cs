using DAL.Models;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace DAL.Repositories.Interface
{
    public interface IReviewRepository
    {
        Task<Review> CreateReviewAsync(Review review);
        Task<Review?> GetReviewByIdAsync(int id);
        Task<Review?> UpdateReviewAsync(Review review);
        Task<bool> DeleteReviewAsync(Review review);
        Task<Review?> ToggleReviewStatusAsync(int id);
    }
}