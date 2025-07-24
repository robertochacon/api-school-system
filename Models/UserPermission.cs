using System.ComponentModel.DataAnnotations;

namespace api_school_system.Models
{
    public class UserPermission
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int PermissionId { get; set; }

        public DateTime GrantedAt { get; set; } = DateTime.UtcNow;

        public int? GrantedByUserId { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual User User { get; set; } = null!;
        public virtual Permission Permission { get; set; } = null!;
        public virtual User? GrantedByUser { get; set; }
    }
} 