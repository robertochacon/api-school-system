using System.ComponentModel.DataAnnotations;

namespace api_school_system.Dtos;

public class RegisterGradeDto
{
    [Required(ErrorMessage = "El ID del estudiante es requerido")]
    public int StudentId { get; set; }

    [Required(ErrorMessage = "El ID de la matrícula es requerido")]
    public int EnrollmentId { get; set; }

    [Required(ErrorMessage = "El ID de la evaluación es requerido")]
    public int EvaluationId { get; set; }

    [Required(ErrorMessage = "El ID del docente es requerido")]
    public int TeacherId { get; set; }

    [Required(ErrorMessage = "La puntuación es requerida")]
    [Range(0, 100, ErrorMessage = "La puntuación debe estar entre 0 y 100")]
    public decimal Score { get; set; }

    [StringLength(500, ErrorMessage = "Los comentarios no pueden exceder 500 caracteres")]
    public string? Comments { get; set; }
} 