using System.ComponentModel.DataAnnotations;

namespace api_school_system.Models
{
    public class Parent
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [StringLength(50)]
        public string? ParentId { get; set; }

        [StringLength(100)]
        public string? Relationship { get; set; }

        [StringLength(100)]
        public string? Occupation { get; set; }

        [StringLength(100)]
        public string? Workplace { get; set; }

        [StringLength(20)]
        public string? WorkPhone { get; set; }

        public bool IsEmergencyContact { get; set; } = false;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual User User { get; set; } = null!;
        public virtual ICollection<StudentParent> StudentParents { get; set; } = new List<StudentParent>();
    }
} 