using RentalManagementSystem.Models;

namespace RentalManagementSystem.DTOs
{
    public class NotificationRequest
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public NotificationType Type { get; set; }
        public string TargetUrl { get; set; }
    }
}