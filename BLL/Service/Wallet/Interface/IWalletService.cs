using DAL.DTOs.Wallet;

namespace BLL.Service.Wallet.Interface
{
    public interface IWalletService
    {
        Task<decimal> GetWalletBalanceAsync(int userId);
        Task<int> GetPointsBalanceAsync(int userId);
        Task<TopUpResponseDTO> TopUpAsync(TopUpRequestDTO request);
        Task<bool> ConfirmTopUpSuccessByGatewayIdAsync(
            string gatewayTransactionId,
            decimal? finalAmount = null
        );
    }
}
