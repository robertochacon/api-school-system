using System.ComponentModel.DataAnnotations;

namespace api_school_system.Models
{
    public enum EvaluationType
    {
        Exam,
        Quiz,
        Homework,
        Project,
        Participation,
        Final
    }

    public class Evaluation
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int SubjectId { get; set; }

        [Required]
        public int AcademicPeriodId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(255)]
        public string? Description { get; set; }

        [Required]
        public EvaluationType Type { get; set; }

        [Range(0, 100)]
        public decimal Weight { get; set; }

        public DateTime? DueDate { get; set; }

        public DateTime? EvaluationDate { get; set; }

        [Range(0, 100)]
        public decimal MaxScore { get; set; } = 100;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual Subject Subject { get; set; } = null!;
        public virtual AcademicPeriod AcademicPeriod { get; set; } = null!;
        public virtual ICollection<StudentGrade> StudentGrades { get; set; } = new List<StudentGrade>();
    }
} 