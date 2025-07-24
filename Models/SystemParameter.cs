using System.ComponentModel.DataAnnotations;

namespace api_school_system.Models
{
    public class SystemParameter
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        [StringLength(1000)]
        public string Value { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Category { get; set; } = string.Empty;

        [Required]
        public ParameterDataType DataType { get; set; }

        public bool IsEditable { get; set; } = true;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }

    public enum ParameterDataType
    {
        String,
        Integer,
        Decimal,
        Boolean,
        DateTime,
        Json
    }
} 