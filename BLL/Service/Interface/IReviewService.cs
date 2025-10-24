using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Service.Interface
{
    public interface IReviewService
    {
        bool CreateReview(int userId, int productId, string content, int rating);
        bool UpdateReview(int userId, int reviewId, string content, int rating);
        bool DeleteReview(int userId, int reviewId);
        bool UploadReviewMedia(int userId, int reviewId, List<string> mediaUrls);
    }
}
