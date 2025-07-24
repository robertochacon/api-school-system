using System.ComponentModel.DataAnnotations;
using api_school_system.Models;

namespace api_school_system.Dtos;

public class AssignPermissionDto
{
    [Required(ErrorMessage = "El ID del permiso es requerido")]
    public int PermissionId { get; set; }
} 