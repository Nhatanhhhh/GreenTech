using DAL.Context;
using DAL.Models;
using DAL.Models.Enum;
using DAL.Repositories.Wallet.Interface;
using DAL.Utils.AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories.Wallet
{
    public class WalletRepository : IWalletRepository
    {
        private readonly AppDbContext _dbContext;

        public WalletRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<decimal> GetWalletBalanceAsync(int userId)
        {
            var user = await _dbContext
                .Users.AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                throw new KeyNotFoundException("User not found");

            return user.WalletBalance;
        }

        public async Task<int> GetPointsBalanceAsync(int userId)
        {
            var user = await _dbContext
                .Users.AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                throw new KeyNotFoundException("User not found");

            return user.Points;
        }

        public async Task<WalletTransaction> CreateTopUpTransactionAsync(
            int userId,
            decimal amount,
            PaymentGateway gateway,
            string gatewayTransactionId,
            string? description
        )
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                throw new KeyNotFoundException("User not found");

            // Use AutoMapper to create complete transaction
            var transaction = AutoMapper.ToTopUpWalletTransaction(
                userId,
                amount,
                gateway,
                gatewayTransactionId,
                description,
                user.WalletBalance
            );

            _dbContext.WalletTransactions.Add(transaction);
            await _dbContext.SaveChangesAsync();
            return transaction;
        }

        public async Task<WalletTransaction> GetTransactionByIdAsync(int transactionId)
        {
            return await _dbContext
                .WalletTransactions.AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == transactionId);
        }

        public async Task<List<WalletTransaction>> GetUserTransactionsAsync(
            int userId,
            int page = 1,
            int pageSize = 10
        )
        {
            return await _dbContext
                .WalletTransactions.AsNoTracking()
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<bool> UpdateTransactionStatusAsync(
            int transactionId,
            TransactionStatus status,
            decimal? finalAmount = null
        )
        {
            var transaction = await _dbContext.WalletTransactions.FirstOrDefaultAsync(t =>
                t.Id == transactionId
            );

            if (transaction == null)
                return false;

            // Use AutoMapper to update transaction status
            AutoMapper.UpdateTransactionStatus(transaction, status, finalAmount);

            // Check if should update user balance using AutoMapper helper
            if (AutoMapper.ShouldUpdateUserBalance(transaction, status))
            {
                // Update user wallet balance
                var user = await _dbContext.Users.FirstOrDefaultAsync(u =>
                    u.Id == transaction.UserId
                );

                if (user != null)
                {
                    user.WalletBalance += transaction.Amount;
                    user.UpdatedAt = DateTime.UtcNow;
                }
            }

            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateTransactionStatusByGatewayIdAsync(
            string gatewayTransactionId,
            TransactionStatus status,
            decimal? finalAmount = null
        )
        {
            if (string.IsNullOrWhiteSpace(gatewayTransactionId))
                return false;
            var transaction = await _dbContext.WalletTransactions.FirstOrDefaultAsync(t =>
                t.GatewayTransactionId == gatewayTransactionId
            );

            if (transaction == null)
                return false;

            AutoMapper.UpdateTransactionStatus(transaction, status, finalAmount);

            if (AutoMapper.ShouldUpdateUserBalance(transaction, status))
            {
                var user = await _dbContext.Users.FirstOrDefaultAsync(u =>
                    u.Id == transaction.UserId
                );
                if (user != null)
                {
                    user.WalletBalance += transaction.Amount;
                    user.UpdatedAt = DateTime.UtcNow;
                }
            }

            await _dbContext.SaveChangesAsync();
            return true;
        }
    }
}
