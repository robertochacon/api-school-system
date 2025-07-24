using System.ComponentModel.DataAnnotations;

namespace api_school_system.Dtos;

public class UpdateStudentParentDto
{
    [StringLength(50, ErrorMessage = "La relación no puede exceder 50 caracteres")]
    public string? Relationship { get; set; }

    public bool? IsPrimaryContact { get; set; }

    public bool? IsEmergencyContact { get; set; }
} 