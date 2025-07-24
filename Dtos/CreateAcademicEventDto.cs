using System.ComponentModel.DataAnnotations;
using api_school_system.Models;

namespace api_school_system.Dtos;

public class CreateAcademicEventDto
{
    [Required(ErrorMessage = "El ID del período académico es requerido")]
    public int AcademicPeriodId { get; set; }

    [Required(ErrorMessage = "El título del evento es requerido")]
    [StringLength(200, ErrorMessage = "El título no puede exceder 200 caracteres")]
    public string Title { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "El tipo de evento es requerido")]
    public EventType Type { get; set; }

    [Required(ErrorMessage = "La fecha de inicio es requerida")]
    public DateTime StartDate { get; set; }

    [Required(ErrorMessage = "La fecha de fin es requerida")]
    public DateTime EndDate { get; set; }

    public bool IsAllDay { get; set; } = false;

    [StringLength(100, ErrorMessage = "La ubicación no puede exceder 100 caracteres")]
    public string? Location { get; set; }

    [StringLength(500, ErrorMessage = "Las notas no pueden exceder 500 caracteres")]
    public string? Notes { get; set; }
} 