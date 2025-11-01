using System.Security.Cryptography;
using BLL.Service.OTP.Interface;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace BLL.Service.OTP
{
    /// <summary>
    /// Service implementation for OTP operations using in-memory cache
    /// </summary>
    public class OTPService : IOTPService
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger<OTPService> _logger;
        private const string OTP_PREFIX = "OTP_";

        public OTPService(IMemoryCache cache, ILogger<OTPService> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        public string GenerateOTP(string email, int expiryMinutes = 10)
        {
            if (string.IsNullOrEmpty(email))
            {
                throw new ArgumentException("Email cannot be null or empty", nameof(email));
            }

            // Generate 6-digit OTP
            var randomNumber = RandomNumberGenerator.GetInt32(100000, 999999);
            var otp = randomNumber.ToString("D6");

            // Store OTP in cache with expiry
            var cacheKey = $"{OTP_PREFIX}{email.ToLowerInvariant()}";
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(expiryMinutes),
            };

            _cache.Set(cacheKey, otp, cacheOptions);

            _logger.LogInformation(
                $"OTP generated for {email}. Expires in {expiryMinutes} minutes."
            );
            return otp;
        }

        public bool VerifyOTP(string email, string otp)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(otp))
            {
                return false;
            }

            var cacheKey = $"{OTP_PREFIX}{email.ToLowerInvariant()}";
            if (!_cache.TryGetValue(cacheKey, out string? storedOTP))
            {
                _logger.LogWarning(
                    $"OTP verification failed for {email}: OTP not found or expired"
                );
                return false;
            }

            // Compare OTP (case-insensitive)
            var isValid = string.Equals(storedOTP, otp, StringComparison.OrdinalIgnoreCase);
            if (isValid)
            {
                _logger.LogInformation($"OTP verified successfully for {email}");
            }
            else
            {
                _logger.LogWarning($"OTP verification failed for {email}: Invalid OTP");
            }

            return isValid;
        }

        public void RemoveOTP(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return;
            }

            var cacheKey = $"{OTP_PREFIX}{email.ToLowerInvariant()}";
            _cache.Remove(cacheKey);
            _logger.LogInformation($"OTP removed for {email}");
        }
    }
}
