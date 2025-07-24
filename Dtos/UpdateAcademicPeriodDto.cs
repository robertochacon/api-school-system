using System.ComponentModel.DataAnnotations;
using api_school_system.Models;

namespace api_school_system.Dtos;

public class UpdateAcademicPeriodDto
{
    [Required(ErrorMessage = "El nombre del período es requerido")]
    [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "El código del período es requerido")]
    [StringLength(20, ErrorMessage = "El código no puede exceder 20 caracteres")]
    public string Code { get; set; } = string.Empty;

    [StringLength(255, ErrorMessage = "La descripción no puede exceder 255 caracteres")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "La fecha de inicio es requerida")]
    public DateTime StartDate { get; set; }

    [Required(ErrorMessage = "La fecha de fin es requerida")]
    public DateTime EndDate { get; set; }

    public PeriodStatus? Status { get; set; }
} 