using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BLL.Service.Interface;
using DAL.Context;
using DAL.Models;

namespace BLL.Service
{
    public class AdminReviewService : IAdminReviewService
    {
        private readonly ApplicationDbContext _context;

        public AdminReviewService(ApplicationDbContext context)
        {
            _context = context;
        }

        private bool IsAdminRole(int userId)
        {
            var roleName = (from ur in _context.UserRoles
                            join r in _context.Roles on ur.RoleId equals r.Id
                            where ur.UserId == userId
                            select r.RoleName.ToString())
                            .FirstOrDefault();

            return roleName == "ROLE_ADMIN";
        }

        public List<Review> GetAllReviews(int adminUserId)
        {
            if (!IsAdminRole(adminUserId))
                return new List<Review>(); // Không có quyền -> trả về rỗng

            var reviews = _context.Reviews
                .OrderByDescending(r => r.CreatedAt)
                .ToList();

            return reviews;
        }
    }
}
