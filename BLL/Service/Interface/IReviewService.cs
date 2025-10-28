using DAL.DTOs.Review;
using DAL.Models;
using System.Threading.Tasks;


namespace BLL.Service.Interface
{
    public interface IReviewService
    {
        Task<Review> CreateReviewAsync(CreateReviewDTO dto);
        Task<Review?> UpdateReviewAsync(UpdateReviewDTO dto, int userId);
        Task<bool> DeleteReviewAsync(int reviewId, int userId);
    }
}