using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace DAL.Utils.CryptoUtil
{
    /// <summary>
    /// Provides cryptographic utility methods including password hashing, HMAC,
    /// RSA encryption/decryption, and byte/hex conversions.
    /// </summary>
    public static class CryptoUtil
    {
        /// <summary>
        /// Supported HMAC algorithms
        /// </summary>
        public static readonly string HMACMD5 = "HmacMD5";
        public static readonly string HMACSHA1 = "HmacSHA1";
        public static readonly string HMACSHA256 = "HmacSHA256";
        public static readonly string HMACSHA512 = "HmacSHA512";

        // UTF-8 charset
        private static readonly Encoding UTF8 = Encoding.UTF8;

        /// <summary>
        /// Hash password using PBKDF2 with HMACSHA256
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        public static string HashPassword(string password)
        {
            byte[] salt = new byte[128 / 8];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            string hashed = Convert.ToBase64String(
                KeyDerivation.Pbkdf2(
                    password: password,
                    salt: salt,
                    prf: KeyDerivationPrf.HMACSHA256,
                    iterationCount: 100000,
                    numBytesRequested: 256 / 8
                )
            );

            return $"{Convert.ToBase64String(salt)}:{hashed}";
        }

        /// <summary>
        /// Hash password using HMACSHA512 with salt
        /// </summary>
        /// <param name="password">Plain text password</param>
        /// <param name="key">HMAC key (if null, uses default key)</param>
        /// <returns>Base64 encoded hash with salt</returns>
        public static string HashPasswordHmacSHA512(string password, string key = null)
        {
            if (string.IsNullOrEmpty(password))
                return null;

            // Generate salt
            byte[] salt = new byte[64 / 8];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            // Use default key if not provided
            if (string.IsNullOrEmpty(key))
            {
                key = "GreenTech2024!@#$%^&*()SecretKey";
            }

            // Combine password with salt
            string dataWithSalt = $"{password}{Convert.ToBase64String(salt)}";

            // Compute HMAC-SHA512
            string hmacHash = HMacBase64Encode(HMACSHA512, key, dataWithSalt);

            // Return salt:hash format
            return $"{Convert.ToBase64String(salt)}:{hmacHash}";
        }

        /// <summary>
        /// Verify password against HMACSHA512 hashed password
        /// </summary>
        /// <param name="hashedPassword">Stored hash (format: salt:hash)</param>
        /// <param name="providedPassword">Password to verify</param>
        /// <param name="key">HMAC key (if null, uses default key)</param>
        /// <returns>True if password matches</returns>
        public static bool VerifyPasswordHmacSHA512(
            string hashedPassword,
            string providedPassword,
            string key = null
        )
        {
            if (string.IsNullOrEmpty(hashedPassword) || string.IsNullOrEmpty(providedPassword))
                return false;

            var parts = hashedPassword.Split(':');
            if (parts.Length != 2)
                return false;

            byte[] salt = Convert.FromBase64String(parts[0]);
            string storedHash = parts[1];

            // Use default key if not provided
            if (string.IsNullOrEmpty(key))
            {
                key = "GreenTech2024!@#$%^&*()SecretKey";
            }

            // Combine password with salt
            string dataWithSalt = $"{providedPassword}{Convert.ToBase64String(salt)}";

            // Compute HMAC-SHA512
            string computedHash = HMacBase64Encode(HMACSHA512, key, dataWithSalt);

            return storedHash == computedHash;
        }

        /// <summary>
        /// Verify password against a hashed password
        /// </summary>
        /// <param name="hashedPassword"></param>
        /// <param name="providedPassword"></param>
        /// <returns></returns>
        public static bool VerifyPassword(string hashedPassword, string providedPassword)
        {
            if (string.IsNullOrEmpty(hashedPassword) || string.IsNullOrEmpty(providedPassword))
                return false;

            var parts = hashedPassword.Split(':');
            if (parts.Length != 2)
                return false;

            byte[] salt = Convert.FromBase64String(parts[0]);
            string hash = parts[1];

            string hashedProvided = Convert.ToBase64String(
                KeyDerivation.Pbkdf2(
                    password: providedPassword,
                    salt: salt,
                    prf: KeyDerivationPrf.HMACSHA256,
                    iterationCount: 100000,
                    numBytesRequested: 256 / 8
                )
            );

            return hash == hashedProvided;
        }

        /// <summary>
        /// HMAC encoding with specified algorithm
        /// </summary>
        /// <param name="algorithm"></param>
        /// <param name="key"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        private static byte[] HMacEncode(string algorithm, string key, string data)
        {
            try
            {
                using (var hmac = HMAC.Create(algorithm))
                {
                    if (hmac == null)
                        return null;

                    var keyBytes = UTF8.GetBytes(key);
                    var dataBytes = UTF8.GetBytes(data);
                    hmac.Key = keyBytes;
                    return hmac.ComputeHash(dataBytes);
                }
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// HMAC with Base64 output
        /// </summary>
        /// <param name="algorithm"></param>
        /// <param name="key"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string HMacBase64Encode(string algorithm, string key, string data)
        {
            byte[] hmacEncodeBytes = HMacEncode(algorithm, key, data);
            return hmacEncodeBytes != null ? Convert.ToBase64String(hmacEncodeBytes) : null;
        }

        /// <summary>
        /// HMAC with Hex output
        /// </summary>
        /// <param name="algorithm"></param>
        /// <param name="key"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string HMacHexStringEncode(string algorithm, string key, string data)
        {
            byte[] hmacEncodeBytes = HMacEncode(algorithm, key, data);
            return hmacEncodeBytes != null ? ByteArrayToHexString(hmacEncodeBytes) : null;
        }

        /// <summary>
        /// Convert byte array to hexadecimal string
        /// </summary>
        /// <param name="raw"></param>
        /// <returns></returns>
        public static string ByteArrayToHexString(byte[] raw)
        {
            char[] hex = new char[2 * raw.Length];
            int index = 0;
            const string HEX_CHARS = "0123456789abcdef";

            foreach (byte b in raw)
            {
                int v = b & 0xFF;
                hex[index++] = HEX_CHARS[v >> 4];
                hex[index++] = HEX_CHARS[v & 0xF];
            }

            return new string(hex);
        }

        /// <summary>
        /// Convert hexadecimal string to byte array
        /// </summary>
        /// <param name="hex"></param>
        /// <returns></returns>
        public static byte[] HexStringToByteArray(string hex)
        {
            hex = hex.ToLowerInvariant();
            int sz = hex.Length / 2;
            byte[] bytesResult = new byte[sz];

            for (int i = 0, idx = 0; i < sz; i++)
            {
                char c1 = hex[idx++];
                char c2 = hex[idx++];

                int v1 = c1 > '9' ? c1 - 'a' + 10 : c1 - '0';
                int v2 = c2 > '9' ? c2 - 'a' + 10 : c2 - '0';

                bytesResult[i] = (byte)((v1 << 4) + v2);
            }

            return bytesResult;
        }

        /// <summary>
        /// RSA encryption and decryption
        /// </summary>
        private static RSA _rsa;

        static CryptoUtil()
        {
            try
            {
                _rsa = RSA.Create();
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to initialize RSA", ex);
            }
        }

        /// <summary>
        /// Create RSA public key from Base64-encoded string
        /// </summary>
        /// <param name="pubkeyStr"></param>
        /// <returns></returns>
        public static RSAParameters StringToPublicKey(string pubkeyStr)
        {
            byte[] keyBytes = Convert.FromBase64String(pubkeyStr);
            _rsa.ImportSubjectPublicKeyInfo(keyBytes, out _);
            return _rsa.ExportParameters(false);
        }

        /// <summary>
        /// Create RSA private key from Base64-encoded string
        /// </summary>
        /// <param name="prikeyStr"></param>
        /// <returns></returns>
        public static RSAParameters StringToPrivateKey(string prikeyStr)
        {
            byte[] keyBytes = Convert.FromBase64String(prikeyStr);
            _rsa.ImportPkcs8PrivateKey(keyBytes, out _);
            return _rsa.ExportParameters(true);
        }

        /// <summary>
        /// RSA encrypt with public key
        /// </summary>
        /// <param name="publicKey"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        /// <exception cref="CryptographicException"></exception>
        public static string Encrypt(RSAParameters publicKey, string message)
        {
            try
            {
                using (var rsa = RSA.Create())
                {
                    rsa.ImportParameters(publicKey);
                    byte[] messageBytes = UTF8.GetBytes(message);
                    byte[] encrypted = rsa.Encrypt(messageBytes, RSAEncryptionPadding.Pkcs1);
                    return Convert.ToBase64String(encrypted);
                }
            }
            catch (Exception ex)
            {
                throw new CryptographicException("RSA encryption failed", ex);
            }
        }

        /// <summary>
        /// RSA encrypt with Base64-encoded public key string
        /// </summary>
        /// <param name="pubkeyStr"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static string Encrypt(string pubkeyStr, string message)
        {
            var publicKey = StringToPublicKey(pubkeyStr);
            return Encrypt(publicKey, message);
        }

        /// <summary>
        /// RSA decrypt with private key
        /// </summary>
        /// <param name="privateKey"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        /// <exception cref="CryptographicException"></exception>
        public static byte[] Decrypt(RSAParameters privateKey, string message)
        {
            try
            {
                using (var rsa = RSA.Create())
                {
                    rsa.ImportParameters(privateKey);
                    byte[] messageBytes = Convert.FromBase64String(message);
                    return rsa.Decrypt(messageBytes, RSAEncryptionPadding.Pkcs1);
                }
            }
            catch (Exception ex)
            {
                throw new CryptographicException("RSA decryption failed", ex);
            }
        }

        /// <summary>
        /// RSA decrypt with Base64-encoded private key string
        /// </summary>
        /// <param name="prikeyStr"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static byte[] Decrypt(string prikeyStr, string message)
        {
            var privateKey = StringToPrivateKey(prikeyStr);
            return Decrypt(privateKey, message);
        }

        /// <summary>
        /// Generate a password reset token that contains email and expiry time
        /// Token format: Base64(Email|ExpiryTimestamp|Nonce):HMAC
        /// </summary>
        /// <param name="email">User email</param>
        /// <param name="expiryHours">Hours until token expires (default 1 hour)</param>
        /// <param name="key">HMAC key (if null, uses default key)</param>
        /// <returns>Reset token string</returns>
        public static string GeneratePasswordResetToken(
            string email,
            int expiryHours = 1,
            string key = null
        )
        {
            if (string.IsNullOrEmpty(email))
                throw new ArgumentException("Email cannot be null or empty", nameof(email));

            // Generate nonce for uniqueness
            byte[] nonceBytes = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(nonceBytes);
            }
            string nonce = Convert.ToBase64String(nonceBytes);

            // Calculate expiry timestamp
            long expiryTimestamp = (
                (DateTimeOffset)DateTime.UtcNow.AddHours(expiryHours)
            ).ToUnixTimeSeconds();

            // Create payload: email|timestamp|nonce
            string payload = $"{email}|{expiryTimestamp}|{nonce}";

            // Encode payload to Base64
            byte[] payloadBytes = UTF8.GetBytes(payload);
            string encodedPayload = Convert.ToBase64String(payloadBytes);

            // Use default key if not provided
            if (string.IsNullOrEmpty(key))
            {
                key = "GreenTech2024!@#$%^&*()SecretKey";
            }

            // Generate HMAC signature
            string hmacSignature = HMacBase64Encode(HMACSHA512, key, encodedPayload);

            // Return token: encodedPayload:hmacSignature
            return $"{encodedPayload}:{hmacSignature}";
        }

        /// <summary>
        /// Verify and decode password reset token
        /// </summary>
        /// <param name="token">Reset token to verify</param>
        /// <param name="expectedEmail">Expected email (optional, for additional validation)</param>
        /// <param name="key">HMAC key (if null, uses default key)</param>
        /// <returns>Tuple (isValid, email, expiryTimestamp). Returns (false, null, 0) if invalid.</returns>
        public static (bool isValid, string email, long expiryTimestamp) VerifyPasswordResetToken(
            string token,
            string expectedEmail = null,
            string key = null
        )
        {
            if (string.IsNullOrEmpty(token))
                return (false, null, 0);

            var parts = token.Split(':');
            if (parts.Length != 2)
                return (false, null, 0);

            string encodedPayload = parts[0];
            string providedHmac = parts[1];

            // Use default key if not provided
            if (string.IsNullOrEmpty(key))
            {
                key = "GreenTech2024!@#$%^&*()SecretKey";
            }

            // Verify HMAC signature
            string computedHmac = HMacBase64Encode(HMACSHA512, key, encodedPayload);
            if (computedHmac != providedHmac)
                return (false, null, 0);

            try
            {
                // Decode payload
                byte[] payloadBytes = Convert.FromBase64String(encodedPayload);
                string payload = UTF8.GetString(payloadBytes);

                var payloadParts = payload.Split('|');
                if (payloadParts.Length != 3)
                    return (false, null, 0);

                string email = payloadParts[0];
                long expiryTimestamp = long.Parse(payloadParts[1]);

                // Verify expiry
                long currentTimestamp = ((DateTimeOffset)DateTime.UtcNow).ToUnixTimeSeconds();
                if (expiryTimestamp < currentTimestamp)
                    return (false, email, expiryTimestamp); // Token expired

                // Verify email if provided
                if (
                    !string.IsNullOrEmpty(expectedEmail)
                    && email.ToLowerInvariant() != expectedEmail.ToLowerInvariant()
                )
                    return (false, email, expiryTimestamp); // Email mismatch

                return (true, email, expiryTimestamp);
            }
            catch
            {
                return (false, null, 0);
            }
        }
    }
}
