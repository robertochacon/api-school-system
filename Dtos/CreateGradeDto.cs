using System.ComponentModel.DataAnnotations;

namespace api_school_system.Dtos;

public class CreateGradeDto
{
    [Required(ErrorMessage = "El nombre del grado es requerido")]
    [StringLength(50, ErrorMessage = "El nombre no puede exceder 50 caracteres")]
    public string Name { get; set; } = string.Empty;

    [StringLength(255, ErrorMessage = "La descripción no puede exceder 255 caracteres")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "El nivel es requerido")]
    [Range(1, 12, ErrorMessage = "El nivel debe estar entre 1 y 12")]
    public int Level { get; set; }

    [StringLength(10, ErrorMessage = "El código no puede exceder 10 caracteres")]
    public string? Code { get; set; }

    [Range(1, 1000, ErrorMessage = "La capacidad debe estar entre 1 y 1000")]
    public int? Capacity { get; set; }
} 