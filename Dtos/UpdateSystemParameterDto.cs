using System.ComponentModel.DataAnnotations;
using api_school_system.Models;

namespace api_school_system.Dtos;

public class UpdateSystemParameterDto
{
    [Required(ErrorMessage = "El nombre del parámetro es requerido")]
    [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
    public string Name { get; set; } = string.Empty;

    [StringLength(255, ErrorMessage = "La descripción no puede exceder 255 caracteres")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "El valor del parámetro es requerido")]
    [StringLength(500, ErrorMessage = "El valor no puede exceder 500 caracteres")]
    public string Value { get; set; } = string.Empty;

    [StringLength(50, ErrorMessage = "La categoría no puede exceder 50 caracteres")]
    public string? Category { get; set; }

    [Required(ErrorMessage = "El tipo de dato es requerido")]
    public ParameterDataType DataType { get; set; }

    public bool? IsEditable { get; set; }
} 