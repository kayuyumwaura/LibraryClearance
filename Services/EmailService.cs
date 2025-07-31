using System.Net.Mail;
using System.Net;
using Microsoft.Extensions.Configuration;

namespace LibraryClearance.Services
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly IConfiguration _configuration;

        public EmailService(ILogger<EmailService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public async Task SendNewRequestNotificationAsync(string adminEmail, string requestTitle, int requestId)
        {
            try
            {
                var smtpSettings = GetSmtpSettings();

                using var client = new SmtpClient("smtp.gmail.com", 587);
                client.EnableSsl = true;
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(smtpSettings.Username, smtpSettings.Password);

                var message = new MailMessage
                {
                    From = new MailAddress(smtpSettings.FromEmail, "Copyright Clearance System"),
                    Subject = "New Copyright Clearance Request",
                    Body = $@"
Dear Administrator,

A new copyright clearance request has been submitted and requires your review.

Request Details:
- Title: {requestTitle}
- Request ID: {requestId}
- Submitted: {DateTime.Now:yyyy-MM-dd HH:mm:ss}

Please log in to the system to review this request.

Best regards,
Copyright Clearance System",
                    IsBodyHtml = false
                };

                message.To.Add(adminEmail);

                await client.SendMailAsync(message);
                _logger.LogInformation($"Email successfully sent to {adminEmail}: New request '{requestTitle}' (ID: {requestId})");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send email to {adminEmail} for request '{requestTitle}' (ID: {requestId})");
                throw;
            }
        }

        public async Task SendStatusUpdateNotificationAsync(string userEmail, string requestTitle, string status, string comments)
        {
            try
            {
                var smtpSettings = GetSmtpSettings();

                using var client = new SmtpClient("smtp.gmail.com", 587);
                client.EnableSsl = true;
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(smtpSettings.Username, smtpSettings.Password);

                var message = new MailMessage
                {
                    From = new MailAddress(smtpSettings.FromEmail, "Copyright Clearance System"),
                    Subject = $"Copyright Clearance Request Update - {status}",
                    Body = $@"
Dear User,

Your copyright clearance request has been updated.

Request Details:
- Title: {requestTitle}
- New Status: {status}
- Updated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}

{(string.IsNullOrEmpty(comments) ? "" : $"Comments:\n{comments}\n")}

You can log in to the system to view the full details of your request.

Best regards,
Copyright Clearance System",
                    IsBodyHtml = false
                };

                message.To.Add(userEmail);

                await client.SendMailAsync(message);
                _logger.LogInformation($"Status update email successfully sent to {userEmail}: Request '{requestTitle}' status changed to {status}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send status update email to {userEmail} for request '{requestTitle}'");
                throw;
            }
        }

        private SmtpSettings GetSmtpSettings()
        {
            return new SmtpSettings
            {
                Username = _configuration["EmailSettings:Gmail:Username"],
                Password = _configuration["EmailSettings:Gmail:Password"],
                FromEmail = _configuration["EmailSettings:Gmail:FromEmail"]
            };
        }
    }

    public class SmtpSettings
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string FromEmail { get; set; }
    }
}