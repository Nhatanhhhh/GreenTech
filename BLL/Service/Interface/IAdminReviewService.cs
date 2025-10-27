using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.Models;

namespace BLL.Service.Interface
{
    public interface IAdminReviewService
    {
        List<Review> GetAllReviews(int adminUserId);
    }
}
