using System.ComponentModel.DataAnnotations;

namespace api_school_system.Models
{
    public enum AttendanceStatus
    {
        Present,
        Absent,
        Late,
        Excused,
        Tardy
    }

    public class Attendance
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int StudentId { get; set; }

        [Required]
        public int EnrollmentId { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public AttendanceStatus Status { get; set; }

        [StringLength(255)]
        public string? Justification { get; set; }

        [StringLength(255)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual Student Student { get; set; } = null!;
        public virtual Enrollment Enrollment { get; set; } = null!;
    }
} 