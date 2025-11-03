using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using BLL.Service.Payments.Interface;
using DAL.DTOs.Payment;
using DAL.Models.Enum;
using DAL.Utils.CryptoUtil;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BLL.Service.Payments
{
    public class VnPayPaymentProcessor : IPaymentGatewayProcessor
    {
        private readonly IConfiguration _config;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<VnPayPaymentProcessor> _logger;

        public VnPayPaymentProcessor(
            IConfiguration config,
            IHttpContextAccessor httpContextAccessor,
            ILogger<VnPayPaymentProcessor> logger
        )
        {
            _config = config;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
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
                var scheme = request.Scheme;
                var host = request.Host;

                // For localhost with HTTPS, VNPay sandbox may have issues with self-signed certificates
                // Use HTTP for localhost development (VNPay sandbox accepts localhost HTTP)
                if (host.Host == "localhost" || host.Host == "127.0.0.1")
                {
                    scheme = "http";
                    // Remove port if it's the default HTTPS port
                    if (host.Port == 7135 || host.Port == 443)
                    {
                        host = new Microsoft.AspNetCore.Http.HostString(host.Host, 5045); // Use HTTP port
                    }
                }

                returnUrl = $"{scheme}://{host}{request.PathBase}/Wallet/Return";
            }

            // Fallback if still empty
            if (string.IsNullOrEmpty(returnUrl))
            {
                // Use HTTP for localhost (VNPay sandbox works better with HTTP for localhost)
                returnUrl = "http://localhost:5045/Wallet/Return";
            }

            // Get client IP address (VNPay requires IPv4 format)
            // IMPORTANT: For VNPay sandbox testing, IP address is critical
            // VNPay sandbox accepts localhost IP, but some configurations may require a valid public IP
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
                            // For VNPay sandbox, localhost IP should work
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

            // VNPay sandbox note: If IP is still localhost and having issues,
            // you may need to configure a valid IP in your VNPay merchant account
            // or use a service like ngrok to expose localhost with a public IP

            // Format: Timestamp + UserId to ensure uniqueness (như trong demo của VNPay)
            var txnRef = DateTime.Now.Ticks.ToString(); // Hoặc có thể dùng: $"{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}_{userId}"

            // VNPay requires: "Tiếng Việt không dấu và không bao gồm các ký tự đặc biệt"
            var orderInfo = RemoveVietnameseDiacritics(description ?? "Nap tien vao vi GreenTech");

            // Use local time (Vietnam timezone GMT+7) for VNPay
            // VNPay expects local time in format: yyyyMMddHHmmss
            var createDate = DateTime.Now.ToString("yyyyMMddHHmmss");

            // ExpireDate: Thời gian hết hạn thanh toán (GMT+7), mặc định 15 phút như trong demo
            var expireDate = DateTime.Now.AddMinutes(15).ToString("yyyyMMddHHmmss");

            // Calculate amount: multiply by 100 to remove decimal
            // Example: 100,000 VND -> 10000000 (multiply by 100)
            var amountInCents = (long)(amount * 100);

            // Sắp xếp tham số theo thứ tự tăng dần của tên tham số (theo doc VNPay)
            var query = new SortedDictionary<string, string>
            {
                ["vnp_Amount"] = amountInCents.ToString(),
                ["vnp_Command"] = "pay",
                ["vnp_CreateDate"] = createDate,
                ["vnp_CurrCode"] = "VND",
                ["vnp_ExpireDate"] = expireDate,
                ["vnp_IpAddr"] = ipAddress,
                ["vnp_Locale"] = "vn",
                ["vnp_OrderInfo"] = orderInfo,
                ["vnp_OrderType"] = "other",
                ["vnp_ReturnUrl"] = returnUrl,
                ["vnp_TmnCode"] = tmnCode,
                ["vnp_TxnRef"] = txnRef,
                ["vnp_Version"] = "2.1.0",
            };

            // Tạo query string đã URL encode (theo cách code tham khảo)
            var queryStringBuilder = new StringBuilder();
            foreach (
                var kv in query.Where(kv => kv.Value != null && !string.IsNullOrEmpty(kv.Value))
            )
            {
                queryStringBuilder.Append(
                    $"{WebUtility.UrlEncode(kv.Key)}={WebUtility.UrlEncode(kv.Value)}&"
                );
            }

            var queryString = queryStringBuilder.ToString();

            // Tạo signData từ queryString đã encode (remove trailing &)
            var signDataString = queryString.TrimEnd('&');
            string signData = string.Empty;
            if (signDataString.Length > 0)
            {
                signData =
                    CryptoUtil.HMacHexStringEncode(CryptoUtil.HMACSHA512, secret, signDataString)
                    ?? throw new InvalidOperationException("Failed to generate VNPay secure hash");
            }

            // Tạo URL với queryString và hash
            var url = payUrl + "?" + queryString + "vnp_SecureHash=" + signData;

            // Log important information for debugging VNPay errors
            // Code 70: Return URL not registered or IP not whitelisted
            // Code 97: Invalid Checksum (hash mismatch)
            _logger.LogInformation(
                "[VNPay Payment Init] Creating payment request - "
                    + "Return URL: {ReturnUrl}, IP: {IpAddress}, Amount: {Amount}, TxnRef: {TxnRef}",
                returnUrl,
                ipAddress,
                amount,
                txnRef
            );
            _logger.LogDebug(
                "[VNPay Debug] Data for hash calculation (encoded): {RawData}",
                signDataString
            );
            _logger.LogDebug("[VNPay Debug] Computed hash: {Hash}", signData);
            _logger.LogDebug("[VNPay Debug] Full payment URL (before redirect): {PaymentUrl}", url);

            // Warning if using localhost (common cause of Code 70)
            if (returnUrl.Contains("localhost") || returnUrl.Contains("127.0.0.1"))
            {
                _logger.LogWarning(
                    "[VNPay Warning] Using localhost URL: {ReturnUrl}. "
                        + "Code 70 may occur if this URL is not registered in VNPay merchant account. "
                        + "Solution: 1) Register URL in VNPay merchant portal, or 2) Use ngrok for public HTTPS URL.",
                    returnUrl
                );
            }

            return Task.FromResult(
                new PaymentInitResult { GatewayTransactionId = txnRef, PayUrl = url }
            );
        }

        /// <summary>
        /// Verify VNPay callback/return parameters and secure hash
        /// Theo doc VNPay: "Merchant/website TMĐT thực hiện kiểm tra sự toàn vẹn của dữ liệu (checksum) trước khi thực hiện các thao tác khác"
        /// </summary>
        public bool VerifyCallback(Dictionary<string, string> callbackParams)
        {
            if (!callbackParams.TryGetValue("vnp_SecureHash", out var receivedHash))
            {
                _logger.LogWarning("[VNPay] Callback verification failed: Missing vnp_SecureHash");
                return false;
            }

            var secret = _config["VNPAY_HASH_SECRET"] ?? string.Empty;
            if (string.IsNullOrEmpty(secret))
            {
                _logger.LogError(
                    "[VNPay] Callback verification failed: VNPAY_HASH_SECRET is not configured"
                );
                return false;
            }

            // Remove vnp_SecureHash và vnp_SecureHashType (nếu có) từ params for signature calculation
            // Theo doc: version 2.1.0 không gửi vnp_SecureHashType sang VNPay, nhưng có thể nhận về
            var paramsForSign = callbackParams
                .Where(kv => kv.Key != "vnp_SecureHash" && kv.Key != "vnp_SecureHashType")
                .OrderBy(kv => kv.Key) // Sắp xếp tăng dần theo tên tham số (theo doc)
                .ToDictionary(kv => kv.Key, kv => kv.Value);

            // Tạo query string đã URL encode để tính hash (giống như khi tạo payment)
            // ASP.NET Core đã decode query string, nhưng để tính hash cần encode lại
            var dataBuilder = new StringBuilder();
            foreach (
                var kv in paramsForSign.Where(kv =>
                    kv.Value != null && !string.IsNullOrEmpty(kv.Value)
                )
            )
            {
                dataBuilder.Append(
                    $"{WebUtility.UrlEncode(kv.Key)}={WebUtility.UrlEncode(kv.Value)}&"
                );
            }

            // Remove trailing & trước khi tính hash
            var rawData =
                dataBuilder.Length > 0 ? dataBuilder.ToString().TrimEnd('&') : string.Empty;

            // Sử dụng HMACSHA512 để verify (theo doc version 2.1.0)
            var computedHash = CryptoUtil.HMacHexStringEncode(
                CryptoUtil.HMACSHA512,
                secret,
                rawData
            );

            if (computedHash == null)
            {
                _logger.LogError("[VNPay] Failed to compute hash for verification");
                return false;
            }

            // So sánh case-sensitive
            var isValid = string.Equals(
                computedHash,
                receivedHash,
                StringComparison.OrdinalIgnoreCase
            );

            if (!isValid)
            {
                _logger.LogWarning(
                    "[VNPay] Callback hash verification failed. "
                        + "Received: {ReceivedHash}, Computed: {ComputedHash}",
                    receivedHash,
                    computedHash
                );
                _logger.LogDebug("[VNPay] Raw data for verification: {RawData}", rawData);
            }
            else
            {
                _logger.LogInformation("[VNPay] Callback hash verification succeeded");
            }

            return isValid;
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

            // Log response codes for debugging
            if (responseCode == "70")
            {
                _logger.LogError(
                    "[VNPay Code 70] Transaction failed. "
                        + "Common causes: 1) Return URL not registered in merchant account, "
                        + "2) IP address not whitelisted. "
                        + "TxnRef: {TxnRef}, Message: {Message}",
                    txnRef,
                    message
                );
            }
            else if (responseCode == "97")
            {
                _logger.LogError(
                    "[VNPay Code 97] Invalid Checksum - Hash verification failed. "
                        + "This usually means the hash calculation is incorrect. "
                        + "TxnRef: {TxnRef}, Message: {Message}",
                    txnRef,
                    message
                );
            }

            // VNPay: ResponseCode = "00" and TransactionStatus = "00" means success
            var isSuccess = responseCode == "00" && transactionStatus == "00";

            decimal? parsedAmount = null;
            if (!string.IsNullOrEmpty(amount) && long.TryParse(amount, out var amtLong))
            {
                // VNPay amount is in cents, convert to VND
                parsedAmount = amtLong / 100m;
            }

            // Map VNPay response codes to user-friendly messages
            var finalMessage = GetVnPayResponseMessage(responseCode, message);

            return (isSuccess, txnRef, parsedAmount, finalMessage);
        }

        /// <summary>
        /// Map VNPay response codes to user-friendly Vietnamese messages
        /// </summary>
        private static string GetVnPayResponseMessage(string? responseCode, string? originalMessage)
        {
            if (!string.IsNullOrEmpty(originalMessage))
            {
                return originalMessage;
            }

            if (string.IsNullOrEmpty(responseCode))
            {
                return "Không nhận được mã phản hồi từ VNPay";
            }

            // VNPay Response Code mapping
            return responseCode switch
            {
                "00" => "Giao dịch thành công",
                "07" =>
                    "Trừ tiền thành công. Giao dịch bị nghi ngờ (liên quan tới lừa đảo, giao dịch bất thường).",
                "09" => "Thẻ/Tài khoản chưa đăng ký dịch vụ InternetBanking",
                "10" => "Xác thực thông tin thẻ/tài khoản không đúng. Quá 3 lần",
                "11" => "Đã hết hạn chờ thanh toán. Xin vui lòng thực hiện lại giao dịch",
                "12" => "Thẻ/Tài khoản bị khóa",
                "13" => "Nhập sai mật khẩu xác thực giao dịch (OTP). Quá 3 lần",
                "24" => "Giao dịch không thành công do: Khách hàng hủy giao dịch",
                "51" => "Tài khoản không đủ số dư để thực hiện giao dịch",
                "65" => "Tài khoản đã vượt quá hạn mức giao dịch trong ngày",
                "75" => "Ngân hàng thanh toán đang bảo trì",
                "79" => "Nhập sai mật khẩu thanh toán quá số lần quy định",
                "99" => "Lỗi không xác định",
                "70" => "Lỗi xử lý dữ liệu từ phía VNPay. Vui lòng liên hệ hỗ trợ.",
                _ => $"Mã lỗi không xác định: {responseCode}. Vui lòng liên hệ hỗ trợ.",
            };
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
