using System.ComponentModel.DataAnnotations;
using api_school_system.Models;

namespace api_school_system.Dtos;

public class CreateEvaluationDto
{
    [Required(ErrorMessage = "El ID de la asignatura es requerido")]
    public int SubjectId { get; set; }

    [Required(ErrorMessage = "El ID del período académico es requerido")]
    public int AcademicPeriodId { get; set; }

    [Required(ErrorMessage = "El nombre de la evaluación es requerido")]
    [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
    public string Name { get; set; } = string.Empty;

    [StringLength(255, ErrorMessage = "La descripción no puede exceder 255 caracteres")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "El tipo de evaluación es requerido")]
    public EvaluationType Type { get; set; }

    [Required(ErrorMessage = "El peso de la evaluación es requerido")]
    [Range(1, 100, ErrorMessage = "El peso debe estar entre 1 y 100")]
    public int Weight { get; set; }

    public DateTime? DueDate { get; set; }

    public DateTime? EvaluationDate { get; set; }

    [Required(ErrorMessage = "La puntuación máxima es requerida")]
    [Range(1, 100, ErrorMessage = "La puntuación máxima debe estar entre 1 y 100")]
    public int MaxScore { get; set; }
} 