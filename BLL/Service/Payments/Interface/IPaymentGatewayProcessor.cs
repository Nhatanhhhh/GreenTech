using DAL.DTOs.Payment;
using DAL.Models.Enum;

namespace BLL.Service.Payments.Interface
{
    public interface IPaymentGatewayProcessor
    {
        PaymentGateway Gateway { get; }
        Task<PaymentInitResult> CreatePaymentAsync(int userId, decimal amount, string? description);

        /// <summary>
        /// Verify callback parameters from payment gateway
        /// </summary>
        /// <param name="callbackParams">Dictionary of callback parameters</param>
        /// <returns>True if callback is valid and verified</returns>
        bool VerifyCallback(Dictionary<string, string> callbackParams);

        /// <summary>
        /// Parse callback parameters from query/form data
        /// Gateway-specific implementations will filter relevant parameters
        /// </summary>
        Dictionary<string, string> ParseCallbackParams(Dictionary<string, string> allParams);

        /// <summary>
        /// Extract transaction information from verified callback
        /// </summary>
        /// <returns>Tuple: (isSuccess, gatewayTransactionId, amount, message)</returns>
        (
            bool isSuccess,
            string? gatewayTransactionId,
            decimal? amount,
            string? message
        ) ExtractCallbackResult(Dictionary<string, string> callbackParams);
    }
}
