using System.ComponentModel.DataAnnotations;

namespace api_school_system.Models
{
    public enum NotificationType
    {
        Message,
        Announcement,
        Grade,
        Attendance,
        Event,
        System
    }

    public enum NotificationStatus
    {
        Unread,
        Read,
        Archived
    }

    public class Notification
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int SenderId { get; set; }

        public int? ReceiverId { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        public string Message { get; set; } = string.Empty;

        [Required]
        public NotificationType Type { get; set; }

        public NotificationStatus Status { get; set; } = NotificationStatus.Unread;

        public DateTime? ReadAt { get; set; }

        [StringLength(255)]
        public string? RelatedEntityType { get; set; }

        public int? RelatedEntityId { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual User Sender { get; set; } = null!;
        public virtual User? Receiver { get; set; }
    }
} 