using System.ComponentModel.DataAnnotations;

namespace api_school_system.Models
{
    public class Teacher
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [StringLength(50)]
        public string? EmployeeId { get; set; }

        [StringLength(100)]
        public string? Specialization { get; set; }

        [StringLength(100)]
        public string? Degree { get; set; }

        [StringLength(100)]
        public string? University { get; set; }

        public DateTime? HireDate { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual User User { get; set; } = null!;
        public virtual ICollection<CourseTeacher> CourseTeachers { get; set; } = new List<CourseTeacher>();
        public virtual ICollection<SubjectTeacher> SubjectTeachers { get; set; } = new List<SubjectTeacher>();
        public virtual ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
        public virtual ICollection<Grade> Grades { get; set; } = new List<Grade>();
    }
} 