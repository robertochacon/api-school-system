using System.ComponentModel.DataAnnotations;
using api_school_system.Models;

namespace api_school_system.Dtos;

public class UpdateStudentDto
{
    [StringLength(20, ErrorMessage = "El número de teléfono no puede exceder 20 caracteres")]
    public string? PhoneNumber { get; set; }

    [StringLength(200, ErrorMessage = "La dirección no puede exceder 200 caracteres")]
    public string? Address { get; set; }

    public DateTime? DateOfBirth { get; set; }

    public Gender? Gender { get; set; }

    [StringLength(200, ErrorMessage = "La escuela anterior no puede exceder 200 caracteres")]
    public string? PreviousSchool { get; set; }

    [StringLength(100, ErrorMessage = "El contacto de emergencia no puede exceder 100 caracteres")]
    public string? EmergencyContact { get; set; }

    [StringLength(20, ErrorMessage = "El teléfono de emergencia no puede exceder 20 caracteres")]
    public string? EmergencyPhone { get; set; }

    [StringLength(500, ErrorMessage = "Las condiciones médicas no pueden exceder 500 caracteres")]
    public string? MedicalConditions { get; set; }

    [StringLength(500, ErrorMessage = "Las alergias no pueden exceder 500 caracteres")]
    public string? Allergies { get; set; }

    public bool? IsActive { get; set; }
} 