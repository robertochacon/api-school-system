using System.ComponentModel.DataAnnotations;

namespace api_school_system.Dtos;

public class AssignTeacherDto
{
    [Required(ErrorMessage = "El ID del docente es requerido")]
    public int TeacherId { get; set; }

    [StringLength(100, ErrorMessage = "El rol no puede exceder 100 caracteres")]
    public string? Role { get; set; }
} 