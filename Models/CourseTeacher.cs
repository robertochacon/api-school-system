using System.ComponentModel.DataAnnotations;

namespace api_school_system.Models
{
    public class CourseTeacher
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CourseId { get; set; }

        [Required]
        public int TeacherId { get; set; }

        [StringLength(100)]
        public string? Role { get; set; } // Principal, Assistant, etc.

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual Course Course { get; set; } = null!;
        public virtual Teacher Teacher { get; set; } = null!;
    }
} 