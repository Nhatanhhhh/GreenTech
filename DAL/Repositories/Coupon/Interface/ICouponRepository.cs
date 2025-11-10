using CouponModel = DAL.Models.Coupon;

namespace DAL.Repositories.Coupon.Interface
{
    public interface ICouponRepository
    {
        Task<CouponModel?> GetByIdAndUserIdAsync(int couponId, int userId);
        Task<CouponModel?> GetByIdAsync(int couponId);
    }
}
