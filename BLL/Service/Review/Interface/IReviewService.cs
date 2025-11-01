using System.Threading.Tasks;
using DAL.DTOs.Review;
using ReviewModel = DAL.Models.Review;
using Microsoft.AspNetCore.Http;

namespace BLL.Service.Review.Interface
{
    public interface IReviewService
    {
        Task<ReviewModel> CreateReviewAsync(CreateReviewDTO dto);
        Task<ReviewModel?> UpdateReviewAsync(UpdateReviewDTO dto, int userId);
        Task<bool> DeleteReviewAsync(int reviewId, int userId);
        Task<IEnumerable<ReviewModel>> GetAllReviewsAsync();
        Task<string> UploadReviewMediaAsync(int reviewId, IFormFile file);
    }
}
