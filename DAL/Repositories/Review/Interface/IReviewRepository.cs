using System.Threading.Tasks;
using DAL.DTOs.Review;
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

        // Methods for customer review functionality
        Task<IEnumerable<ReviewModel>> GetReviewsByProductIdAsync(
            int productId,
            int pageNumber,
            int pageSize
        );
        Task<int> GetReviewsCountByProductIdAsync(int productId);
        Task<ReviewModel?> GetReviewByOrderItemIdAsync(int orderItemId);
        Task<IEnumerable<ReviewModel>> GetApprovedReviewsByProductIdAsync(int productId);
    }
}
