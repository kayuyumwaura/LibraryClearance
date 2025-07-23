namespace LibraryClearance.Services
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;

        public EmailService(ILogger<EmailService> logger)
        {
            _logger = logger;
        }

        public async Task SendNewRequestNotificationAsync(string adminEmail, string requestTitle, int requestId)
        {
            // In a real application, you would implement actual email sending here
            // For demonstration purposes, we'll just log the email
            _logger.LogInformation($"Email sent to {adminEmail}: New copyright clearance request '{requestTitle}' (ID: {requestId}) has been submitted.");

            // Example implementation with SMTP would go here:
            /*
            using var client = new SmtpClient("your-smtp-server.com", 587);
            client.EnableSsl = true;
            client.Credentials = new NetworkCredential("your-email@domain.com", "password");
            
            var message = new MailMessage
            {
                From = new MailAddress("noreply@copyrightclearance.com"),
                Subject = "New Copyright Clearance Request",
                Body = $"A new copyright clearance request '{requestTitle}' has been submitted and requires your review.",
                IsBodyHtml = false
            };
            message.To.Add(adminEmail);
            
            await client.SendMailAsync(message);
            */
        }

        public async Task SendStatusUpdateNotificationAsync(string userEmail, string requestTitle, string status, string comments)
        {
            _logger.LogInformation($"Email sent to {userEmail}: Your copyright clearance request '{requestTitle}' has been {status.ToLower()}. Comments: {comments}");
        }
    }
}
