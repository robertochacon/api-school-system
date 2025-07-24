using System.ComponentModel.DataAnnotations;
using api_school_system.Models;

namespace api_school_system.Dtos;

public class CreateNotificationDto
{
    [Required(ErrorMessage = "El ID del remitente es requerido")]
    public int SenderId { get; set; }

    [Required(ErrorMessage = "El ID del destinatario es requerido")]
    public int ReceiverId { get; set; }

    [Required(ErrorMessage = "El título es requerido")]
    [StringLength(200, ErrorMessage = "El título no puede exceder 200 caracteres")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "El mensaje es requerido")]
    [StringLength(1000, ErrorMessage = "El mensaje no puede exceder 1000 caracteres")]
    public string Message { get; set; } = string.Empty;

    [Required(ErrorMessage = "El tipo de notificación es requerido")]
    public NotificationType Type { get; set; }

    public NotificationStatus Status { get; set; } = NotificationStatus.Unread;

    [StringLength(50, ErrorMessage = "El tipo de entidad relacionada no puede exceder 50 caracteres")]
    public string? RelatedEntityType { get; set; }

    public int? RelatedEntityId { get; set; }
} 