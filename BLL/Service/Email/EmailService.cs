using System.Net;
using System.Net.Mail;
using BLL.Config;
using BLL.Service.Email.Interface;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BLL.Service.Email
{
    /// <summary>
    /// Service implementation for sending emails via SMTP
    /// </summary>
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _settings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<EmailSettings> emailSettings, ILogger<EmailService> logger)
        {
            _settings = emailSettings.Value;
            _logger = logger;
        }

        public async Task<bool> SendEmailAsync(
            string to,
            string subject,
            string body,
            bool isHtml = true
        )
        {
            try
            {
                // Validate settings
                if (
                    string.IsNullOrEmpty(_settings.SmtpServer)
                    || string.IsNullOrEmpty(_settings.Username)
                    || string.IsNullOrEmpty(_settings.Password)
                    || string.IsNullOrEmpty(_settings.FromEmail)
                )
                {
                    _logger.LogError("Email settings are invalid or missing.");
                    return false;
                }

                using var mailMessage = new MailMessage();
                mailMessage.From = new MailAddress(_settings.FromEmail, _settings.FromName);
                mailMessage.To.Add(new MailAddress(to));
                mailMessage.Subject = subject;
                mailMessage.Body = body;
                mailMessage.IsBodyHtml = isHtml;

                using var smtpClient = new SmtpClient(_settings.SmtpServer, _settings.SmtpPort);
                smtpClient.Credentials = new NetworkCredential(
                    _settings.Username,
                    _settings.Password
                );
                smtpClient.EnableSsl = _settings.EnableSsl;

                await smtpClient.SendMailAsync(mailMessage);

                _logger.LogInformation($"Email sent successfully to {to}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send email to {to}");
                return false;
            }
        }
    }
}
