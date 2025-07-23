namespace LibraryClearance.Services
{
    public interface IEmailService
    {
        Task SendNewRequestNotificationAsync(string adminEmail, string requestTitle, int requestId);
        Task SendStatusUpdateNotificationAsync(string userEmail, string requestTitle, string status, string comments);
    }
}
