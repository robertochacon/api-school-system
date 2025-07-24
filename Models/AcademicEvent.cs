using System.ComponentModel.DataAnnotations;

namespace api_school_system.Models
{
    public enum EventType
    {
        Holiday,
        Exam,
        Meeting,
        Activity,
        Deadline,
        Other
    }

    public class AcademicEvent
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int AcademicPeriodId { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        public EventType Type { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public bool IsAllDay { get; set; } = false;

        [StringLength(100)]
        public string? Location { get; set; }

        [StringLength(255)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual AcademicPeriod AcademicPeriod { get; set; } = null!;
    }
} 