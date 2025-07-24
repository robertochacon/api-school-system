using System.ComponentModel.DataAnnotations;
using api_school_system.Models;

namespace api_school_system.Dtos;

public class UpdateEnrollmentDto
{
    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public EnrollmentStatus? Status { get; set; }

    [StringLength(500, ErrorMessage = "Las notas no pueden exceder 500 caracteres")]
    public string? Notes { get; set; }
} 