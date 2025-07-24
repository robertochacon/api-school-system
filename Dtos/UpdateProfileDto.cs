using System.ComponentModel.DataAnnotations;
using api_school_system.Models;

namespace api_school_system.Dtos;

public class UpdateProfileDto
{
    [Required(ErrorMessage = "El nombre es requerido")]
    [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "El apellido es requerido")]
    [StringLength(100, ErrorMessage = "El apellido no puede exceder 100 caracteres")]
    public string LastName { get; set; } = string.Empty;

    [StringLength(20, ErrorMessage = "El número de teléfono no puede exceder 20 caracteres")]
    public string? PhoneNumber { get; set; }

    [StringLength(255, ErrorMessage = "La dirección no puede exceder 255 caracteres")]
    public string? Address { get; set; }

    public DateTime? DateOfBirth { get; set; }

    public Gender? Gender { get; set; }
} 