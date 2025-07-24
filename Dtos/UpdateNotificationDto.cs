using System.ComponentModel.DataAnnotations;
using api_school_system.Models;

namespace api_school_system.Dtos;

public class UpdateNotificationDto
{
    [StringLength(200, ErrorMessage = "El título no puede exceder 200 caracteres")]
    public string? Title { get; set; }

    [StringLength(1000, ErrorMessage = "El mensaje no puede exceder 1000 caracteres")]
    public string? Message { get; set; }

    public NotificationType? Type { get; set; }

    public NotificationStatus? Status { get; set; }

    [StringLength(50, ErrorMessage = "El tipo de entidad relacionada no puede exceder 50 caracteres")]
    public string? RelatedEntityType { get; set; }

    public int? RelatedEntityId { get; set; }
} 