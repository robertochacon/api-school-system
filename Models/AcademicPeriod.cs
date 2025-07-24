using System.ComponentModel.DataAnnotations;

namespace api_school_system.Models
{
    public enum PeriodStatus
    {
        Planning,
        Active,
        Completed,
        Cancelled
    }

    public class AcademicPeriod
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(10)]
        public string? Code { get; set; }

        [StringLength(255)]
        public string? Description { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public PeriodStatus Status { get; set; } = PeriodStatus.Planning;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
        public virtual ICollection<Evaluation> Evaluations { get; set; } = new List<Evaluation>();
        public virtual ICollection<AcademicEvent> AcademicEvents { get; set; } = new List<AcademicEvent>();
    }
} 