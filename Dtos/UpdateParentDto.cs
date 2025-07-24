using System.ComponentModel.DataAnnotations;
using api_school_system.Models;

namespace api_school_system.Dtos;

public class UpdateParentDto
{
    [StringLength(20, ErrorMessage = "El número de teléfono no puede exceder 20 caracteres")]
    public string? PhoneNumber { get; set; }

    [StringLength(200, ErrorMessage = "La dirección no puede exceder 200 caracteres")]
    public string? Address { get; set; }

    public DateTime? DateOfBirth { get; set; }

    public Gender? Gender { get; set; }

    [StringLength(50, ErrorMessage = "La relación no puede exceder 50 caracteres")]
    public string? Relationship { get; set; }

    [StringLength(100, ErrorMessage = "La ocupación no puede exceder 100 caracteres")]
    public string? Occupation { get; set; }

    [StringLength(200, ErrorMessage = "El lugar de trabajo no puede exceder 200 caracteres")]
    public string? Workplace { get; set; }

    [StringLength(20, ErrorMessage = "El teléfono del trabajo no puede exceder 20 caracteres")]
    public string? WorkPhone { get; set; }

    public bool? IsEmergencyContact { get; set; }

    public bool? IsActive { get; set; }
} 