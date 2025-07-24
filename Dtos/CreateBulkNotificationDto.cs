using System.ComponentModel.DataAnnotations;
using api_school_system.Models;

namespace api_school_system.Dtos;

public class CreateBulkNotificationDto
{
    [Required(ErrorMessage = "El ID del remitente es requerido")]
    public int SenderId { get; set; }

    [Required(ErrorMessage = "Los IDs de destinatarios son requeridos")]
    public List<int> ReceiverIds { get; set; } = new();

    [Required(ErrorMessage = "El título es requerido")]
    [StringLength(200, ErrorMessage = "El título no puede exceder 200 caracteres")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "El mensaje es requerido")]
    [StringLength(1000, ErrorMessage = "El mensaje no puede exceder 1000 caracteres")]
    public string Message { get; set; } = string.Empty;

    [Required(ErrorMessage = "El tipo de notificación es requerido")]
    public NotificationType Type { get; set; }

    [StringLength(50, ErrorMessage = "El tipo de entidad relacionada no puede exceder 50 caracteres")]
    public string? RelatedEntityType { get; set; }

    public int? RelatedEntityId { get; set; }
} 