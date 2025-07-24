using System.ComponentModel.DataAnnotations;

namespace api_school_system.Dtos;

public class UpdateScheduleDto
{
    [Required(ErrorMessage = "El día de la semana es requerido")]
    public Models.DayOfWeek DayOfWeek { get; set; }

    [Required(ErrorMessage = "La hora de inicio es requerida")]
    public TimeSpan StartTime { get; set; }

    [Required(ErrorMessage = "La hora de fin es requerida")]
    public TimeSpan EndTime { get; set; }

    [StringLength(50, ErrorMessage = "El aula no puede exceder 50 caracteres")]
    public string? Room { get; set; }

    [StringLength(255, ErrorMessage = "Las notas no pueden exceder 255 caracteres")]
    public string? Notes { get; set; }
} 