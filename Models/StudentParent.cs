using System.ComponentModel.DataAnnotations;

namespace api_school_system.Models
{
    public class StudentParent
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int StudentId { get; set; }

        [Required]
        public int ParentId { get; set; }

        [StringLength(100)]
        public string? Relationship { get; set; }

        public bool IsPrimaryContact { get; set; } = false;

        public bool IsEmergencyContact { get; set; } = false;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual Student Student { get; set; } = null!;
        public virtual Parent Parent { get; set; } = null!;
    }
} 