using System.ComponentModel.DataAnnotations;

namespace api_school_system.Dtos;

public class UpdateCourseDto
{
    [Required(ErrorMessage = "El nombre del curso es requerido")]
    [StringLength(50, ErrorMessage = "El nombre no puede exceder 50 caracteres")]
    public string Name { get; set; } = string.Empty;

    [StringLength(10, ErrorMessage = "La sección no puede exceder 10 caracteres")]
    public string? Section { get; set; }

    [StringLength(255, ErrorMessage = "La descripción no puede exceder 255 caracteres")]
    public string? Description { get; set; }

    [Range(1, 1000, ErrorMessage = "La capacidad debe estar entre 1 y 1000")]
    public int? Capacity { get; set; }
} 