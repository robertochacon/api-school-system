using System.ComponentModel.DataAnnotations;
using api_school_system.Models;

namespace api_school_system.Dtos;

public class CreateEnrollmentDto
{
    [Required(ErrorMessage = "El ID del estudiante es requerido")]
    public int StudentId { get; set; }

    [Required(ErrorMessage = "El ID del curso es requerido")]
    public int CourseId { get; set; }

    [Required(ErrorMessage = "El ID del grado es requerido")]
    public int GradeId { get; set; }

    [Required(ErrorMessage = "El ID del período académico es requerido")]
    public int AcademicPeriodId { get; set; }

    [Required(ErrorMessage = "La fecha de matrícula es requerida")]
    public DateTime EnrollmentDate { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public EnrollmentStatus Status { get; set; } = EnrollmentStatus.Pending;

    [StringLength(500, ErrorMessage = "Las notas no pueden exceder 500 caracteres")]
    public string? Notes { get; set; }
} 