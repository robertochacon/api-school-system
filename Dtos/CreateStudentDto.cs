using System.ComponentModel.DataAnnotations;
using api_school_system.Models;

namespace api_school_system.Dtos;

public class CreateStudentDto
{
    // User information
    [Required(ErrorMessage = "El nombre de usuario es requerido")]
    [StringLength(50, ErrorMessage = "El nombre de usuario no puede exceder 50 caracteres")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es requerida")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener entre 6 y 100 caracteres")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre es requerido")]
    [StringLength(50, ErrorMessage = "El nombre no puede exceder 50 caracteres")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "El apellido es requerido")]
    [StringLength(50, ErrorMessage = "El apellido no puede exceder 50 caracteres")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "El email es requerido")]
    [EmailAddress(ErrorMessage = "El formato del email no es válido")]
    [StringLength(100, ErrorMessage = "El email no puede exceder 100 caracteres")]
    public string Email { get; set; } = string.Empty;

    [StringLength(20, ErrorMessage = "El número de teléfono no puede exceder 20 caracteres")]
    public string? PhoneNumber { get; set; }

    [StringLength(200, ErrorMessage = "La dirección no puede exceder 200 caracteres")]
    public string? Address { get; set; }

    public DateTime? DateOfBirth { get; set; }

    public Gender Gender { get; set; }

    // Student specific information
    [Required(ErrorMessage = "El ID del estudiante es requerido")]
    [StringLength(20, ErrorMessage = "El ID del estudiante no puede exceder 20 caracteres")]
    public string StudentId { get; set; } = string.Empty;

    public DateTime? EnrollmentDate { get; set; }

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
} 