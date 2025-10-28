using DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Repositories.Interface
{
    public interface IReviewRepository
    {
        bool AddReview(Review review);
        bool UpdateReview(int reviewId, string content, int rating);
        bool DeleteReview(int reviewId);
        bool UploadReviewMedia(int reviewId, List<string> mediaUrls);
    }
}