using DAL.Context;
using DAL.DTOs.User;
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
            return await GetByIdAsync(userId, includeBlocked: false);
        }

        public async Task<UserModel?> GetByIdAsync(int userId, bool includeBlocked)
        {
            var query = _context
                .Users.Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .Where(u => u.Id == userId);

            if (!includeBlocked)
            {
                query = query.Where(u => u.Status == UserStatus.ACTIVE);
            }

            return await query.FirstOrDefaultAsync();
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

        public async Task<IEnumerable<UserModel>> GetAllAsync(UserQueryParamsDTO queryParams)
        {
            var query = _context
                .Users.Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .Where(u =>
                    u.UserRoles.Any(ur =>
                        ur.Role.RoleName == RoleName.ROLE_STAFF
                        || ur.Role.RoleName == RoleName.ROLE_CUSTOMER
                    )
                )
                .AsQueryable();

            query = ApplyFilters(query, queryParams);
            query = ApplySorting(query, queryParams);

            return await query
                .Skip((queryParams.PageNumber - 1) * queryParams.PageSize)
                .Take(queryParams.PageSize)
                .ToListAsync();
        }

        public async Task<int> CountAsync(UserQueryParamsDTO queryParams)
        {
            var query = _context
                .Users.Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .Where(u =>
                    u.UserRoles.Any(ur =>
                        ur.Role.RoleName == RoleName.ROLE_STAFF
                        || ur.Role.RoleName == RoleName.ROLE_CUSTOMER
                    )
                ) // Chỉ đếm Staff và Customer, loại bỏ Admin
                .AsQueryable();
            query = ApplyFilters(query, queryParams);
            return await query.CountAsync();
        }

        public async Task<UserModel> BlockUserAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                throw new KeyNotFoundException("Người dùng không tồn tại");
            }

            user.Status = UserStatus.BLOCKED;
            user.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<UserModel> UnblockUserAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                throw new KeyNotFoundException("Người dùng không tồn tại");
            }

            user.Status = UserStatus.ACTIVE;
            user.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();
            return user;
        }

        private IQueryable<UserModel> ApplyFilters(
            IQueryable<UserModel> query,
            UserQueryParamsDTO queryParams
        )
        {
            if (!string.IsNullOrWhiteSpace(queryParams.SearchTerm))
            {
                var searchTermLower = queryParams.SearchTerm.ToLower();
                query = query.Where(u =>
                    u.FullName.ToLower().Contains(searchTermLower)
                    || u.Email.ToLower().Contains(searchTermLower)
                    || u.Phone.Contains(searchTermLower)
                );
            }

            if (!string.IsNullOrWhiteSpace(queryParams.Status))
            {
                if (Enum.TryParse<UserStatus>(queryParams.Status.ToUpper(), out var status))
                {
                    query = query.Where(u => u.Status == status);
                }
            }

            return query;
        }

        private IQueryable<UserModel> ApplySorting(
            IQueryable<UserModel> query,
            UserQueryParamsDTO queryParams
        )
        {
            var isDescending = queryParams.SortOrder?.ToLower() == "desc";

            switch (queryParams.SortBy?.ToLower())
            {
                case "fullname":
                    query = isDescending
                        ? query.OrderByDescending(u => u.FullName)
                        : query.OrderBy(u => u.FullName);
                    break;
                case "email":
                    query = isDescending
                        ? query.OrderByDescending(u => u.Email)
                        : query.OrderBy(u => u.Email);
                    break;
                case "createdat":
                    query = isDescending
                        ? query.OrderByDescending(u => u.CreatedAt)
                        : query.OrderBy(u => u.CreatedAt);
                    break;
                case "points":
                    query = isDescending
                        ? query.OrderByDescending(u => u.Points)
                        : query.OrderBy(u => u.Points);
                    break;
                case "walletbalance":
                    query = isDescending
                        ? query.OrderByDescending(u => u.WalletBalance)
                        : query.OrderBy(u => u.WalletBalance);
                    break;
                default:
                    query = query.OrderByDescending(u => u.CreatedAt);
                    break;
            }

            return query;
        }

        public async Task<UserModel> ChangeUserRoleAsync(int userId, string roleName)
        {
            if (!Enum.TryParse<RoleName>(roleName.ToUpper(), out var roleNameEnum))
            {
                throw new ArgumentException($"Invalid role name: {roleName}");
            }

            // Không cho phép thay đổi role thành ADMIN
            if (roleNameEnum == RoleName.ROLE_ADMIN)
            {
                throw new UnauthorizedAccessException("Không được phép thay đổi role thành ADMIN");
            }

            var user = await _context
                .Users.Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                throw new KeyNotFoundException("Người dùng không tồn tại");
            }

            // Tìm role mới
            var newRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == roleNameEnum);

            if (newRole == null)
            {
                throw new KeyNotFoundException($"Role {roleName} không tồn tại");
            }

            // Xóa tất cả role hiện tại của user
            var existingUserRoles = _context.UserRoles.Where(ur => ur.UserId == userId);
            _context.UserRoles.RemoveRange(existingUserRoles);

            // Thêm role mới
            var userRole = new DAL.Models.UserRole { UserId = userId, RoleId = newRole.Id };
            await _context.UserRoles.AddAsync(userRole);

            user.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            // Reload user với role mới
            await _context.Entry(user).Collection(u => u.UserRoles).LoadAsync();
            foreach (var ur in user.UserRoles)
            {
                await _context.Entry(ur).Reference(ur => ur.Role).LoadAsync();
            }

            return user;
        }
    }
}
