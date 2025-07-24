using System.ComponentModel.DataAnnotations;

namespace api_school_system.Dtos;

public class AssignParentDto
{
    [Required(ErrorMessage = "El ID del padre es requerido")]
    public int ParentId { get; set; }

    [StringLength(50, ErrorMessage = "La relación no puede exceder 50 caracteres")]
    public string? Relationship { get; set; }

    public bool IsPrimaryContact { get; set; } = false;

    public bool IsEmergencyContact { get; set; } = false;
} 