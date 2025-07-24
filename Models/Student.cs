using System.ComponentModel.DataAnnotations;

namespace api_school_system.Models
{
    public class Student
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [StringLength(50)]
        public string? StudentId { get; set; }

        public DateTime? EnrollmentDate { get; set; }

        [StringLength(100)]
        public string? PreviousSchool { get; set; }

        [StringLength(255)]
        public string? EmergencyContact { get; set; }

        [StringLength(20)]
        public string? EmergencyPhone { get; set; }

        [StringLength(255)]
        public string? MedicalConditions { get; set; }

        [StringLength(255)]
        public string? Allergies { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual User User { get; set; } = null!;
        public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
        public virtual ICollection<Grade> Grades { get; set; } = new List<Grade>();
        public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
        public virtual ICollection<StudentParent> StudentParents { get; set; } = new List<StudentParent>();
    }
} 