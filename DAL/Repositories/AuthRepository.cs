using DAL.Context;
using DAL.DTOs.User;
using DAL.Models;
using DAL.Repositories.Interface;
using Microsoft.EntityFrameworkCore;
using DAL.Models.Enum;
using DAL.Utils.AutoMapper;

namespace DAL.Repositories
{
    public class AuthRepository : IAuthRepository
    {
        private readonly AppDbContext _context;

        public AuthRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<User> RegisterAsync(RegisterDTO registerDTO)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Create user using AutoMapper
                var user = AutoMapper.ToUser(registerDTO);
                await _context.Users.AddAsync(user);
                await _context.SaveChangesAsync();

                // Assign ROLE_CUSTOMER to user using AutoMapper
                var customerRole = await _context.Roles
                    .FirstOrDefaultAsync(r => r.RoleName == RoleName.ROLE_CUSTOMER);

                if (customerRole == null)
                {
                    throw new Exception("Customer role not found");
                }

                var userRole = AutoMapper.ToUserRole(user.Id, customerRole.Id);
                await _context.UserRoles.AddAsync(userRole);
                await _context.SaveChangesAsync();

                // Create cart for user using AutoMapper
                var cart = AutoMapper.ToCart(user.Id);
                await _context.Carts.AddAsync(cart);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return user;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<User?> LoginAsync(LoginDTO loginDTO)
        {
            return await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Email.ToLower() == loginDTO.Email.ToLower() && u.Status == UserStatus.ACTIVE);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Users
                .AnyAsync(u => u.Email.ToLower() == email.ToLower());
        }

        public async Task<bool> PhoneExistsAsync(string phone)
        {
            return await _context.Users
                .AnyAsync(u => u.Phone == phone);
        }
    }
}
