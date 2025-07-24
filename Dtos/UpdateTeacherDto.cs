using System.ComponentModel.DataAnnotations;
using api_school_system.Models;

namespace api_school_system.Dtos;

public class UpdateTeacherDto
{
    [StringLength(20, ErrorMessage = "El número de teléfono no puede exceder 20 caracteres")]
    public string? PhoneNumber { get; set; }

    [StringLength(200, ErrorMessage = "La dirección no puede exceder 200 caracteres")]
    public string? Address { get; set; }

    public DateTime? DateOfBirth { get; set; }

    public Gender? Gender { get; set; }

    [StringLength(100, ErrorMessage = "La especialización no puede exceder 100 caracteres")]
    public string? Specialization { get; set; }

    [StringLength(100, ErrorMessage = "El grado académico no puede exceder 100 caracteres")]
    public string? Degree { get; set; }

    [StringLength(200, ErrorMessage = "La universidad no puede exceder 200 caracteres")]
    public string? University { get; set; }

    public DateTime? HireDate { get; set; }

    public bool? IsActive { get; set; }
} 