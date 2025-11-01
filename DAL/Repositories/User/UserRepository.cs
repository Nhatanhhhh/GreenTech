using DAL.Context;
using DAL.Models.Enum;
using DAL.Repositories.User.Interface;
using Microsoft.EntityFrameworkCore;
using UserModel = DAL.Models.User;

namespace DAL.Repositories.User
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<UserModel?> GetByIdAsync(int userId)
        {
            return await _context
                .Users.Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == userId && u.Status == UserStatus.ACTIVE);
        }

        public async Task<UserModel?> GetByEmailAsync(string email)
        {
            return await _context
                .Users.Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u =>
                    u.Email.ToLower() == email.ToLower() && u.Status == UserStatus.ACTIVE
                );
        }

        public async Task<UserModel> UpdateAsync(UserModel user)
        {
            user.UpdatedAt = DateTime.Now;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return user;
        }
    }
}
