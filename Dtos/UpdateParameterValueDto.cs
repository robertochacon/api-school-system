using System.ComponentModel.DataAnnotations;
using api_school_system.Models;

namespace api_school_system.Dtos;

public class UpdateParameterValueDto
{
    [Required(ErrorMessage = "El valor del parámetro es requerido")]
    [StringLength(500, ErrorMessage = "El valor no puede exceder 500 caracteres")]
    public string Value { get; set; } = string.Empty;
} 