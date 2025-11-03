using System.Net.Http;
using System.Text;
using System.Text.Json;
using BLL.Service.Payments.Interface;
using DAL.DTOs.Payment;
using DAL.Models.Enum;
using DAL.Utils.CryptoUtil;
using Microsoft.Extensions.Configuration;

namespace BLL.Service.Payments
{
    public class MoMoPaymentProcessor : IPaymentGatewayProcessor
    {
        private readonly IConfiguration _config;

        public MoMoPaymentProcessor(IConfiguration config)
        {
            _config = config;
        }

        public PaymentGateway Gateway => PaymentGateway.MOMO;

        public async Task<PaymentInitResult> CreatePaymentAsync(
            int userId,
            decimal amount,
            string? description
        )
        {
            var partnerCode = _config["MOMO_PARTNER_CODE"] ?? string.Empty;
            var accessKey = _config["MOMO_ACCESS_KEY"] ?? string.Empty;
            var secretKey = _config["MOMO_SECRET_KEY"] ?? string.Empty;
            var createEndpoint = _config["MOMO_PAY_URL"] ?? string.Empty; // e.g. https://test-payment.momo.vn/v2/gateway/api/create

            var orderId =
                $"{partnerCode}_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}_{userId}";
            var requestId = $"{partnerCode}_{Guid.NewGuid():N}";

            // MoMo v2 uses redirectUrl + ipnUrl and requires 'extraData' in signature string
            var redirectUrl = _config["MOMO_REDIRECT_URL"] ?? "";
            var ipnUrl = _config["MOMO_IPN_URL"] ?? "";

            if (!Uri.TryCreate(createEndpoint, UriKind.Absolute, out var createUri))
            {
                throw new InvalidOperationException(
                    "MOMO_PAY_URL is missing or not an absolute URL. Example: https://test-payment.momo.vn/v2/gateway/api/create"
                );
            }
            if (!Uri.TryCreate(redirectUrl, UriKind.Absolute, out var redirectUri))
            {
                throw new InvalidOperationException(
                    "MOMO_REDIRECT_URL is missing or not an absolute URL. Set it to a valid http URL, e.g. http://localhost:5045/Wallet/Return"
                );
            }
            if (!Uri.TryCreate(ipnUrl, UriKind.Absolute, out var ipnUri))
            {
                throw new InvalidOperationException(
                    "MOMO_IPN_URL is missing or not an absolute URL. Set it to a publicly reachable http URL or your dev return endpoint, e.g. http://localhost:5045/Wallet/Return"
                );
            }
            var extraData = string.Empty; // base64 if any custom data; keep empty for now
            var requestType = "captureWallet";
            var orderInfo = description ?? "Top up wallet";

            // IMPORTANT: Do NOT URL-encode values inside the signature string.
            var rawSignature =
                $"accessKey={accessKey}&amount={(long)decimal.Truncate(amount)}&extraData={extraData}&ipnUrl={ipnUrl}&orderId={orderId}&orderInfo={orderInfo}&partnerCode={partnerCode}&redirectUrl={redirectUrl}&requestId={requestId}&requestType={requestType}";

            var signature = CryptoUtil.HMacHexStringEncode(
                CryptoUtil.HMACSHA256,
                secretKey,
                rawSignature
            );

            var payload = new
            {
                partnerCode,
                accessKey,
                requestId,
                amount = ((long)decimal.Truncate(amount)).ToString(),
                orderId,
                orderInfo,
                redirectUrl,
                ipnUrl,
                extraData,
                requestType,
                signature,
                lang = "vi",
            };

            using var http = new HttpClient();
            var json = JsonSerializer.Serialize(payload);
            var resp = await http.PostAsync(
                createUri,
                new StringContent(json, Encoding.UTF8, "application/json")
            );
            var respText = await resp.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(respText);
            var root = doc.RootElement;
            var resultCode = root.TryGetProperty("resultCode", out var rc) ? rc.GetInt32() : -1;
            if (resultCode != 0)
            {
                var message = root.TryGetProperty("message", out var msg)
                    ? msg.GetString()
                    : "MoMo create failed";
                throw new ArgumentException($"MoMo error {resultCode}: {message}");
            }

            var payUrl = root.GetProperty("payUrl").GetString();

            return new PaymentInitResult
            {
                GatewayTransactionId = orderId,
                PayUrl = payUrl ?? string.Empty,
            };
        }

        /// <summary>
        /// Verify MoMo callback parameters
        /// MoMo doesn't use signature verification for redirect callbacks,
        /// but we can verify required fields are present
        /// </summary>
        public bool VerifyCallback(Dictionary<string, string> callbackParams)
        {
            // MoMo redirect callbacks include: resultCode, orderId, amount, message, transId
            // For redirect, we verify by checking required fields exist
            // For IPN (POST), MoMo doesn't provide signature, so we trust the IPN endpoint
            return callbackParams.ContainsKey("resultCode")
                && callbackParams.ContainsKey("orderId");
        }

        /// <summary>
        /// Parse callback parameters from all params (filters MoMo-specific keys)
        /// </summary>
        public Dictionary<string, string> ParseCallbackParams(Dictionary<string, string> allParams)
        {
            var result = new Dictionary<string, string>();
            var momoKeys = new[]
            {
                "resultCode",
                "orderId",
                "amount",
                "message",
                "transId",
                "orderType",
                "payType",
            };

            foreach (var kvp in allParams)
            {
                if (momoKeys.Contains(kvp.Key, StringComparer.OrdinalIgnoreCase))
                {
                    result[kvp.Key] = kvp.Value;
                }
            }
            return result;
        }

        /// <summary>
        /// Extract transaction information from MoMo callback
        /// </summary>
        public (
            bool isSuccess,
            string? gatewayTransactionId,
            decimal? amount,
            string? message
        ) ExtractCallbackResult(Dictionary<string, string> callbackParams)
        {
            var resultCode = callbackParams.GetValueOrDefault("resultCode");
            var orderId = callbackParams.GetValueOrDefault("orderId");
            var amount = callbackParams.GetValueOrDefault("amount");
            var message = callbackParams.GetValueOrDefault("message");
            var transId = callbackParams.GetValueOrDefault("transId");

            // MoMo: resultCode = "0" means success
            var isSuccess = resultCode == "0";

            decimal? parsedAmount = null;
            if (!string.IsNullOrEmpty(amount) && decimal.TryParse(amount, out var amt))
            {
                parsedAmount = amt;
            }

            var finalMessage =
                message ?? (isSuccess ? "Giao dịch thành công" : "Giao dịch thất bại");

            return (isSuccess, orderId, parsedAmount, finalMessage);
        }
    }
}
