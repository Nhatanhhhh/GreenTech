using System.Threading.Tasks;
using ReviewModel = DAL.Models.Review;

namespace DAL.Repositories.Review.Interface
{
    public interface IReviewRepository
    {
        Task<ReviewModel> CreateReviewAsync(ReviewModel review);
        Task<ReviewModel?> GetReviewByIdAsync(int id);
        Task<ReviewModel?> UpdateReviewAsync(ReviewModel review);
        Task<bool> DeleteReviewAsync(ReviewModel review);
        Task<ReviewModel?> ToggleReviewStatusAsync(int id);
        Task<IEnumerable<ReviewModel>> GetAllReviewsAsync();
    }
}
