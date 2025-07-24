using System.ComponentModel.DataAnnotations;

namespace api_school_system.Dtos;

public class AssignSubjectTeacherDto
{
    [Required(ErrorMessage = "El ID del docente es requerido")]
    public int TeacherId { get; set; }
} 