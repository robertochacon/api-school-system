using System.ComponentModel.DataAnnotations;

namespace api_school_system.Dtos;

public class UpdateSubjectDto
{
    [Required(ErrorMessage = "El nombre de la asignatura es requerido")]
    [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "El código de la asignatura es requerido")]
    [StringLength(20, ErrorMessage = "El código no puede exceder 20 caracteres")]
    public string Code { get; set; } = string.Empty;

    [StringLength(255, ErrorMessage = "La descripción no puede exceder 255 caracteres")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Los créditos son requeridos")]
    [Range(1, 10, ErrorMessage = "Los créditos deben estar entre 1 y 10")]
    public int Credits { get; set; }

    [Required(ErrorMessage = "Las horas por semana son requeridas")]
    [Range(1, 20, ErrorMessage = "Las horas por semana deben estar entre 1 y 20")]
    public int HoursPerWeek { get; set; }

    [StringLength(100, ErrorMessage = "El departamento no puede exceder 100 caracteres")]
    public string? Department { get; set; }
} 