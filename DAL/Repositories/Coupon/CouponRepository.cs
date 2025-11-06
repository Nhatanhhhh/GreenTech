using DAL.Context;
using DAL.Repositories.Coupon.Interface;
using Microsoft.EntityFrameworkCore;
using CouponModel = DAL.Models.Coupon;

namespace DAL.Repositories.Coupon
{
    public class CouponRepository : ICouponRepository
    {
        private readonly AppDbContext _context;

        public CouponRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<CouponModel?> GetByIdAndUserIdAsync(int couponId, int userId)
        {
            return await _context.Coupons.FirstOrDefaultAsync(c =>
                c.Id == couponId && c.UserId == userId && c.IsActive
            );
        }

        public async Task<CouponModel?> GetByIdAsync(int couponId)
        {
            return await _context.Coupons.FirstOrDefaultAsync(c => c.Id == couponId);
        }
    }
}
