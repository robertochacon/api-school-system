using System.ComponentModel.DataAnnotations;

namespace api_school_system.Models
{
    public class Course
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int GradeId { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;

        [StringLength(10)]
        public string? Section { get; set; }

        [StringLength(255)]
        public string? Description { get; set; }

        public int? Capacity { get; set; }

        public int? CurrentEnrollment { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual Grade Grade { get; set; } = null!;
        public virtual ICollection<CourseTeacher> CourseTeachers { get; set; } = new List<CourseTeacher>();
        public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
        public virtual ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
    }
} 