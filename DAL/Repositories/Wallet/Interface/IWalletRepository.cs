using DAL.DTOs.Wallet;
using DAL.Models;
using DAL.Models.Enum;

namespace DAL.Repositories.Wallet.Interface
{
    public interface IWalletRepository
    {
        Task<decimal> GetWalletBalanceAsync(int userId);
        Task<int> GetPointsBalanceAsync(int userId);
        Task<WalletTransaction> CreateTopUpTransactionAsync(
            int userId,
            decimal amount,
            PaymentGateway gateway,
            string gatewayTransactionId,
            string? description
        );
        Task<WalletTransaction> GetTransactionByIdAsync(int transactionId);
        Task<List<WalletTransaction>> GetUserTransactionsAsync(
            int userId,
            int page = 1,
            int pageSize = 10
        );
        Task<bool> UpdateTransactionStatusAsync(
            int transactionId,
            TransactionStatus status,
            decimal? finalAmount = null
        );
        Task<bool> UpdateTransactionStatusByGatewayIdAsync(
            string gatewayTransactionId,
            TransactionStatus status,
            decimal? finalAmount = null
        );
        Task<WalletTransaction> CreateHoldTransactionAsync(
            int userId,
            int orderId,
            decimal amount,
            string? description
        );
        Task<bool> ConfirmHoldTransactionAsync(int orderId, TransactionStatus status);
        Task<WalletTransaction> CreateRefundTransactionAsync(
            int userId,
            int orderId,
            decimal amount,
            string? description
        );
        Task<WalletTransaction?> GetHoldTransactionByOrderIdAsync(int orderId);
    }
}
