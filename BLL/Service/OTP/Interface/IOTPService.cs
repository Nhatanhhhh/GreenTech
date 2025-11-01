namespace BLL.Service.OTP.Interface
{
    /// <summary>
    /// Interface for OTP (One-Time Password) operations
    /// </summary>
    public interface IOTPService
    {
        /// <summary>
        /// Generate and store OTP for a given email
        /// </summary>
        /// <param name="email">User email address</param>
        /// <param name="expiryMinutes">OTP expiry time in minutes (default: 10)</param>
        /// <returns>Generated OTP code (6 digits)</returns>
        string GenerateOTP(string email, int expiryMinutes = 10);

        /// <summary>
        /// Verify OTP for a given email
        /// </summary>
        /// <param name="email">User email address</param>
        /// <param name="otp">OTP code to verify</param>
        /// <returns>True if OTP is valid, false otherwise</returns>
        bool VerifyOTP(string email, string otp);

        /// <summary>
        /// Remove OTP for a given email (after successful verification)
        /// </summary>
        /// <param name="email">User email address</param>
        void RemoveOTP(string email);
    }
}
