namespace DAL.Utils.FormEmail
{
    /// <summary>
    /// Email template for sending OTP code for password reset
    /// </summary>
    public static class SendEmailOtp
    {
        /// <summary>
        /// Generates HTML email body for OTP password reset
        /// </summary>
        /// <param name="fullName">User's full name</param>
        /// <param name="otp">OTP code to display</param>
        /// <param name="expiryMinutes">OTP expiry time in minutes</param>
        /// <returns>HTML formatted email body</returns>
        public static string GenerateEmailBody(string fullName, string otp, int expiryMinutes = 10)
        {
            return $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <h2 style='color: #007934;'>Đặt lại mật khẩu</h2>
                    <p>Xin chào <strong>{fullName}</strong>,</p>
                    <p>Bạn đã yêu cầu đặt lại mật khẩu cho tài khoản của mình.</p>
                    <p>Mã OTP của bạn là:</p>
                    <div style='background-color: #f0f0f0; padding: 20px; text-align: center; margin: 20px 0; border-radius: 5px;'>
                        <h1 style='color: #007934; font-size: 32px; letter-spacing: 5px; margin: 0;'>{otp}</h1>
                    </div>
                    <p>Mã OTP này có hiệu lực trong <strong>{expiryMinutes} phút</strong>.</p>
                    <p>Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng bỏ qua email này.</p>
                    <hr style='border: none; border-top: 1px solid #eee; margin: 20px 0;'/>
                    <p style='color: #666; font-size: 12px;'>Email này được gửi tự động từ hệ thống GreenTech. Vui lòng không trả lời email này.</p>
                </div>
            ";
        }
    }
}

