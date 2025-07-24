using System.ComponentModel.DataAnnotations;
using api_school_system.Models;

namespace api_school_system.Dtos;

public class BulkAssignPermissionDto
{
    [Required(ErrorMessage = "El ID del usuario es requerido")]
    public int UserId { get; set; }

    [Required(ErrorMessage = "Los IDs de permisos son requeridos")]
    public List<int> PermissionIds { get; set; } = new();
} 