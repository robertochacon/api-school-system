using System.ComponentModel.DataAnnotations;
using api_school_system.Models;

namespace api_school_system.Dtos;

public class UpdatePermissionDto
{
    [Required(ErrorMessage = "El nombre del permiso es requerido")]
    [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
    public string Name { get; set; } = string.Empty;

    [StringLength(255, ErrorMessage = "La descripción no puede exceder 255 caracteres")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "El módulo es requerido")]
    [StringLength(50, ErrorMessage = "El módulo no puede exceder 50 caracteres")]
    public string Module { get; set; } = string.Empty;

    [Required(ErrorMessage = "La acción es requerida")]
    [StringLength(50, ErrorMessage = "La acción no puede exceder 50 caracteres")]
    public string Action { get; set; } = string.Empty;
} 