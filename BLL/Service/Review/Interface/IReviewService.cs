using System.Threading.Tasks;
using DAL.DTOs.Review;
using ReviewModel = DAL.Models.Review;

namespace BLL.Service.Review.Interface
{
    public interface IReviewService
    {
        Task<ReviewModel> CreateReviewAsync(CreateReviewDTO dto);
        Task<ReviewModel?> UpdateReviewAsync(UpdateReviewDTO dto, int userId);
        Task<bool> DeleteReviewAsync(int reviewId, int userId);
        Task<ReviewModel?> ToggleReviewStatusAsync(int id);
        Task<IEnumerable<ReviewModel>> GetAllReviewsAsync();
    }
}
