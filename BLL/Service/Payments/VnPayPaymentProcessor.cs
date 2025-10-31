using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using BLL.Service.Payments.Interface;
using DAL.DTOs.Payment;
using DAL.Models.Enum;
using DAL.Utils.CryptoUtil;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace BLL.Service.Payments
{
    public class VnPayPaymentProcessor : IPaymentGatewayProcessor
    {
        private readonly IConfiguration _config;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public VnPayPaymentProcessor(
            IConfiguration config,
            IHttpContextAccessor httpContextAccessor
        )
        {
            _config = config;
            _httpContextAccessor = httpContextAccessor;
        }

        public PaymentGateway Gateway => PaymentGateway.VNPAY;

        public Task<PaymentInitResult> CreatePaymentAsync(
            int userId,
            decimal amount,
            string? description
        )
        {
            var tmnCode = _config["VNPAY_TMN_CODE"] ?? string.Empty;
            var secret = _config["VNPAY_HASH_SECRET"] ?? string.Empty;
            var payUrl = _config["VNPAY_PAY_URL"] ?? string.Empty;
            var returnUrl = _config["VNPAY_RETURN_URL"];

            // Validate required configuration (same as VNPay demo)
            if (string.IsNullOrEmpty(tmnCode) || string.IsNullOrEmpty(secret))
            {
                throw new InvalidOperationException(
                    "Vui lòng cấu hình các tham số: VNPAY_TMN_CODE, VNPAY_HASH_SECRET trong file .env"
                );
            }

            if (string.IsNullOrEmpty(payUrl))
            {
                throw new InvalidOperationException(
                    "Vui lòng cấu hình VNPAY_PAY_URL trong file .env"
                );
            }

            var httpContext = _httpContextAccessor.HttpContext;

            // Get base URL dynamically if not configured
            if (string.IsNullOrEmpty(returnUrl) && httpContext != null)
            {
                var request = httpContext.Request;
                returnUrl = $"{request.Scheme}://{request.Host}{request.PathBase}/Wallet/Return";
            }

            // Fallback if still empty
            if (string.IsNullOrEmpty(returnUrl))
            {
                returnUrl = "https://localhost:7135/Wallet/Return";
            }

            // Get client IP address (VNPay requires IPv4 format)
            var ipAddress = "127.0.0.1";
            if (httpContext != null)
            {
                // Try to get real IP from headers (for proxies/load balancers)
                var forwardedFor = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
                if (!string.IsNullOrEmpty(forwardedFor))
                {
                    var ip = forwardedFor.Split(',')[0].Trim();
                    // Convert IPv6 to IPv4 if needed
                    if (IPAddress.TryParse(ip, out var parsedIp))
                    {
                        // If IPv6 localhost (::1), use IPv4
                        if (
                            ip == "::1"
                            || (
                                parsedIp.AddressFamily == AddressFamily.InterNetworkV6
                                && IPAddress.IsLoopback(parsedIp)
                            )
                        )
                        {
                            ipAddress = "127.0.0.1";
                        }
                        // If IPv4, use as is
                        else if (parsedIp.AddressFamily == AddressFamily.InterNetwork)
                        {
                            ipAddress = ip;
                        }
                        // If IPv6, try to map to IPv4 or use default
                        else
                        {
                            ipAddress = "127.0.0.1";
                        }
                    }
                }
                else if (httpContext.Connection.RemoteIpAddress != null)
                {
                    var remoteIp = httpContext.Connection.RemoteIpAddress;
                    var remoteIpString = remoteIp.ToString();

                    // Convert IPv6 to IPv4 if needed
                    if (
                        remoteIpString == "::1"
                        || (
                            remoteIp.AddressFamily == AddressFamily.InterNetworkV6
                            && IPAddress.IsLoopback(remoteIp)
                        )
                    )
                    {
                        ipAddress = "127.0.0.1";
                    }
                    // If IPv4, use as is
                    else if (remoteIp.AddressFamily == AddressFamily.InterNetwork)
                    {
                        ipAddress = remoteIpString;
                    }
                    // If IPv6, use default
                    else
                    {
                        ipAddress = "127.0.0.1";
                    }
                }
            }

            // Format: Timestamp + UserId to ensure uniqueness
            // Alternative: Use simple numeric like demo (DateTime.Now.Ticks or orderId)
            var txnRef = $"{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}_{userId}";

            // VNPay requires: "Tiếng Việt không dấu và không bao gồm các ký tự đặc biệt"
            var orderInfo = RemoveVietnameseDiacritics(description ?? "Nap tien vao vi GreenTech");

            // Use local time (Vietnam timezone) for VNPay, same as demo
            // VNPay expects local time in format: yyyyMMddHHmmss
            var createDate = DateTime.Now.ToString("yyyyMMddHHmmss");

            // Calculate amount: multiply by 100 to remove decimal (same as demo)
            // Example: 100,000 VND -> 10000000 (multiply by 100)
            var amountInCents = (long)(amount * 100);

            var query = new SortedDictionary<string, string>
            {
                ["vnp_Version"] = "2.1.0",
                ["vnp_Command"] = "pay",
                ["vnp_TmnCode"] = tmnCode,
                ["vnp_Amount"] = amountInCents.ToString(),
                ["vnp_CurrCode"] = "VND",
                ["vnp_TxnRef"] = txnRef,
                ["vnp_OrderInfo"] = orderInfo,
                ["vnp_OrderType"] = "other",
                ["vnp_Locale"] = "vn",
                ["vnp_ReturnUrl"] = returnUrl,
                ["vnp_IpAddr"] = ipAddress,
                ["vnp_CreateDate"] = createDate,
            };

            // IMPORTANT: For VNPay hash calculation, do NOT URL encode values
            // Only encode when building final URL
            var raw = string.Join('&', query.Select(kv => $"{kv.Key}={kv.Value}"));
            var signData = CryptoUtil.HMacHexStringEncode(CryptoUtil.HMACSHA512, secret, raw);

            // Build URL with URL-encoded parameters (for final URL only)
            var urlParams = string.Join(
                '&',
                query.Select(kv => $"{kv.Key}={Uri.EscapeDataString(kv.Value)}")
            );
            var url = payUrl + "?" + urlParams + "&vnp_SecureHash=" + signData;

            return Task.FromResult(
                new PaymentInitResult { GatewayTransactionId = txnRef, PayUrl = url }
            );
        }

        /// <summary>
        /// Verify VNPay callback/return parameters and secure hash
        /// </summary>
        public bool VerifyCallback(Dictionary<string, string> callbackParams)
        {
            if (!callbackParams.TryGetValue("vnp_SecureHash", out var receivedHash))
            {
                return false;
            }

            var secret = _config["VNPAY_HASH_SECRET"] ?? string.Empty;
            if (string.IsNullOrEmpty(secret))
            {
                return false;
            }

            // Remove vnp_SecureHash from params for signature calculation
            var paramsForSign = callbackParams
                .Where(kv => kv.Key != "vnp_SecureHash")
                .OrderBy(kv => kv.Key)
                .ToDictionary(kv => kv.Key, kv => kv.Value);

            // IMPORTANT: For VNPay hash verification, do NOT URL encode values
            var raw = string.Join('&', paramsForSign.Select(kv => $"{kv.Key}={kv.Value}"));

            var computedHash = CryptoUtil.HMacHexStringEncode(CryptoUtil.HMACSHA512, secret, raw);

            return computedHash == receivedHash;
        }

        /// <summary>
        /// Parse callback parameters from all params (filters vnp_ prefix)
        /// </summary>
        public Dictionary<string, string> ParseCallbackParams(Dictionary<string, string> allParams)
        {
            var result = new Dictionary<string, string>();
            foreach (var kvp in allParams)
            {
                if (kvp.Key.StartsWith("vnp_"))
                {
                    result[kvp.Key] = kvp.Value;
                }
            }
            return result;
        }

        /// <summary>
        /// Extract transaction information from VNPay callback
        /// </summary>
        public (
            bool isSuccess,
            string? gatewayTransactionId,
            decimal? amount,
            string? message
        ) ExtractCallbackResult(Dictionary<string, string> callbackParams)
        {
            var responseCode = callbackParams.GetValueOrDefault("vnp_ResponseCode");
            var txnRef = callbackParams.GetValueOrDefault("vnp_TxnRef");
            var transactionStatus = callbackParams.GetValueOrDefault("vnp_TransactionStatus");
            var amount = callbackParams.GetValueOrDefault("vnp_Amount");
            var message = callbackParams.GetValueOrDefault("vnp_Message");

            // VNPay: ResponseCode = "00" and TransactionStatus = "00" means success
            var isSuccess = responseCode == "00" && transactionStatus == "00";

            decimal? parsedAmount = null;
            if (!string.IsNullOrEmpty(amount) && long.TryParse(amount, out var amtLong))
            {
                // VNPay amount is in cents, convert to VND
                parsedAmount = amtLong / 100m;
            }

            var finalMessage =
                message ?? (isSuccess ? "Giao dịch thành công" : "Giao dịch thất bại");

            return (isSuccess, txnRef, parsedAmount, finalMessage);
        }

        /// <summary>
        /// Remove Vietnamese diacritics and special characters for VNPay OrderInfo
        /// VNPay requires: "Tiếng Việt không dấu và không bao gồm các ký tự đặc biệt"
        /// </summary>
        private static string RemoveVietnameseDiacritics(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            // Convert to lowercase and trim
            string str = input.ToLower().Trim();

            // Remove Vietnamese diacritics
            str = Regex.Replace(str, @"[àáạảãâầấậẩẫăằắặẳẵ]", "a");
            str = Regex.Replace(str, @"[èéẹẻẽêềếệểễ]", "e");
            str = Regex.Replace(str, @"[ìíịỉĩ]", "i");
            str = Regex.Replace(str, @"[òóọỏõôồốộổỗơờớợởỡ]", "o");
            str = Regex.Replace(str, @"[ùúụủũưừứựửữ]", "u");
            str = Regex.Replace(str, @"[ỳýỵỷỹ]", "y");
            str = Regex.Replace(str, @"[đ]", "d");

            // Remove special characters, keep only letters, numbers, and spaces
            str = Regex.Replace(str, @"[^a-z0-9\s]", "");

            // Replace multiple spaces with single space
            str = Regex.Replace(str, @"\s+", " ").Trim();

            return str;
        }
    }
}
