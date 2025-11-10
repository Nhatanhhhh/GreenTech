using System.Threading.Tasks;
using DAL.DTOs.Review;
using Microsoft.AspNetCore.Http;
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
        Task<string> UploadReviewMediaAsync(int reviewId, IFormFile file);

        // Methods for customer review functionality
        Task<ReviewPaginationDTO> GetReviewsByProductIdAsync(
            int productId,
            int pageNumber,
            int pageSize,
            int? currentUserId = null
        );
        Task<ReviewStatisticsDTO> GetReviewStatisticsByProductIdAsync(int productId);
        Task<ReviewModel?> GetReviewByOrderItemIdAsync(int orderItemId);
        Task<bool> CheckReviewExistsForOrderItemAsync(int orderItemId);
    }
}
