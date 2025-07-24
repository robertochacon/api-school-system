using System.ComponentModel.DataAnnotations;

namespace api_school_system.Models
{
    public enum EnrollmentStatus
    {
        Pending,
        Active,
        Completed,
        Cancelled,
        Suspended
    }

    public class Enrollment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int StudentId { get; set; }

        [Required]
        public int CourseId { get; set; }

        [Required]
        public int GradeId { get; set; }

        [Required]
        public int AcademicPeriodId { get; set; }

        public DateTime EnrollmentDate { get; set; } = DateTime.UtcNow;

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public EnrollmentStatus Status { get; set; } = EnrollmentStatus.Pending;

        [StringLength(255)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual Student Student { get; set; } = null!;
        public virtual Course Course { get; set; } = null!;
        public virtual Grade Grade { get; set; } = null!;
        public virtual AcademicPeriod AcademicPeriod { get; set; } = null!;
        public virtual ICollection<StudentGrade> StudentGrades { get; set; } = new List<StudentGrade>();
        public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
    }
} 