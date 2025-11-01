namespace BLL.Service.Email.Interface
{
    /// <summary>
    /// Interface for email sending operations
    /// </summary>
    public interface IEmailService
    {
        /// <summary>
        /// Send email asynchronously
        /// </summary>
        /// <param name="to">Recipient email address</param>
        /// <param name="subject">Email subject</param>
        /// <param name="body">Email body (HTML supported)</param>
        /// <param name="isHtml">Whether the body is HTML (default: true)</param>
        /// <returns>Task representing the async operation. Returns true if successful, false otherwise.</returns>
        Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = true);
    }
}
