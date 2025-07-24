using System.ComponentModel.DataAnnotations;

namespace api_school_system.Models
{
    public class StudentGrade
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int StudentId { get; set; }

        [Required]
        public int EnrollmentId { get; set; }

        [Required]
        public int EvaluationId { get; set; }

        [Required]
        public int TeacherId { get; set; }

        [Range(0, 100)]
        public decimal Score { get; set; }

        [StringLength(255)]
        public string? Comments { get; set; }

        public DateTime GradedAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual Student Student { get; set; } = null!;
        public virtual Enrollment Enrollment { get; set; } = null!;
        public virtual Evaluation Evaluation { get; set; } = null!;
        public virtual Teacher Teacher { get; set; } = null!;
    }
} 