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
    }
}
