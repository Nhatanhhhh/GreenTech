using BLL.Service.Payments.Interface;
using BLL.Service.Wallet.Interface;
using DAL.DTOs.Wallet;
using DAL.Models.Enum;
using DAL.Repositories.Wallet.Interface;

namespace BLL.Service.Wallet
{
    public class WalletService : IWalletService
    {
        private readonly IWalletRepository _walletRepository;
        private readonly IPaymentGatewayFactory _paymentFactory;

        public WalletService(
            IWalletRepository walletRepository,
            IPaymentGatewayFactory paymentFactory
        )
        {
            _walletRepository = walletRepository;
            _paymentFactory = paymentFactory;
        }

        public Task<decimal> GetWalletBalanceAsync(int userId)
        {
            return _walletRepository.GetWalletBalanceAsync(userId);
        }

        public Task<int> GetPointsBalanceAsync(int userId)
        {
            return _walletRepository.GetPointsBalanceAsync(userId);
        }

        public async Task<TopUpResponseDTO> TopUpAsync(TopUpRequestDTO request)
        {
            if (!Enum.TryParse<PaymentGateway>(request.Gateway, true, out var gateway))
            {
                throw new ArgumentException(
                    "Cổng thanh toán không hợp lệ",
                    nameof(request.Gateway)
                );
            }

            var processor = _paymentFactory.Get(gateway);
            var init = await processor.CreatePaymentAsync(
                request.UserId,
                request.Amount,
                request.Description
            );

            var created = await _walletRepository.CreateTopUpTransactionAsync(
                request.UserId,
                request.Amount,
                gateway,
                init.GatewayTransactionId,
                request.Description
            );

            return new TopUpResponseDTO
            {
                WalletTransactionId = created.Id,
                GatewayTransactionId = created.GatewayTransactionId,
                Gateway = gateway.ToString(),
                Amount = request.Amount,
                PayUrl = init.PayUrl,
            };
        }

        public async Task<bool> ConfirmTopUpSuccessByGatewayIdAsync(
            string gatewayTransactionId,
            decimal? finalAmount = null
        )
        {
            if (string.IsNullOrWhiteSpace(gatewayTransactionId))
            {
                return false;
            }

            // Update by gateway transaction id; repository handles status and balance updates
            return await _walletRepository.UpdateTransactionStatusByGatewayIdAsync(
                gatewayTransactionId,
                TransactionStatus.SUCCESS,
                finalAmount
            );
        }
    }
}
