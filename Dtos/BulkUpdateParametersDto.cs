using System.ComponentModel.DataAnnotations;
using api_school_system.Models;

namespace api_school_system.Dtos;

public class BulkUpdateParametersDto
{
    [Required(ErrorMessage = "Los parámetros son requeridos")]
    public List<ParameterUpdate> Parameters { get; set; } = new();
}

public class ParameterUpdate
{
    [Required(ErrorMessage = "El ID del parámetro es requerido")]
    public int ParameterId { get; set; }

    [Required(ErrorMessage = "El valor del parámetro es requerido")]
    [StringLength(500, ErrorMessage = "El valor no puede exceder 500 caracteres")]
    public string Value { get; set; } = string.Empty;
} 