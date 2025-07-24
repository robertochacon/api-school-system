using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using api_school_system.Data;
using api_school_system.Models;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace api_school_system.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public NotificationsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/notifications
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetNotifications(
            [FromQuery] NotificationType? type = null,
            [FromQuery] NotificationStatus? status = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] int? senderId = null,
            [FromQuery] int? receiverId = null)
        {
            var currentUserId = GetCurrentUserId();
            var isAdmin = User.IsInRole("Admin");

            var query = _context.Notifications
                .Include(n => n.Sender)
                .Include(n => n.Receiver)
                .Where(n => n.IsActive);

            // Si no es admin, solo puede ver sus propias notificaciones
            if (!isAdmin)
            {
                query = query.Where(n => n.ReceiverId == currentUserId);
            }

            if (type.HasValue)
                query = query.Where(n => n.Type == type);
            if (status.HasValue)
                query = query.Where(n => n.Status == status);
            if (startDate.HasValue)
                query = query.Where(n => n.CreatedAt >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(n => n.CreatedAt <= endDate.Value);
            if (senderId.HasValue)
                query = query.Where(n => n.SenderId == senderId);
            if (receiverId.HasValue && isAdmin)
                query = query.Where(n => n.ReceiverId == receiverId);

            var notifications = await query
                .OrderByDescending(n => n.CreatedAt)
                .Select(n => new
                {
                    n.Id,
                    n.SenderId,
                    n.ReceiverId,
                    n.Title,
                    n.Message,
                    n.Type,
                    n.Status,
                    n.ReadAt,
                    n.RelatedEntityType,
                    n.RelatedEntityId,
                    n.IsActive,
                    n.CreatedAt,
                    n.UpdatedAt,
                    Sender = n.Sender != null ? new
                    {
                        n.Sender.Id,
                        n.Sender.FirstName,
                        n.Sender.LastName,
                        n.Sender.Email
                    } : null,
                    Receiver = n.Receiver != null ? new
                    {
                        n.Receiver.Id,
                        n.Receiver.FirstName,
                        n.Receiver.LastName,
                        n.Receiver.Email
                    } : null
                })
                .ToListAsync();

            return Ok(notifications);
        }

        // GET: api/notifications/5
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetNotification(int id)
        {
            var currentUserId = GetCurrentUserId();
            var isAdmin = User.IsInRole("Admin");

            var notification = await _context.Notifications
                .Include(n => n.Sender)
                .Include(n => n.Receiver)
                .Where(n => n.Id == id && n.IsActive)
                .Select(n => new
                {
                    n.Id,
                    n.SenderId,
                    n.ReceiverId,
                    n.Title,
                    n.Message,
                    n.Type,
                    n.Status,
                    n.ReadAt,
                    n.RelatedEntityType,
                    n.RelatedEntityId,
                    n.IsActive,
                    n.CreatedAt,
                    n.UpdatedAt,
                    Sender = n.Sender != null ? new
                    {
                        n.Sender.Id,
                        n.Sender.FirstName,
                        n.Sender.LastName,
                        n.Sender.Email
                    } : null,
                    Receiver = n.Receiver != null ? new
                    {
                        n.Receiver.Id,
                        n.Receiver.FirstName,
                        n.Receiver.LastName,
                        n.Receiver.Email
                    } : null
                })
                .FirstOrDefaultAsync();

            if (notification == null)
            {
                return NotFound("Notificación no encontrada");
            }

            // Verificar que el usuario puede ver esta notificación
            if (!isAdmin && notification.ReceiverId != currentUserId)
            {
                return Forbid();
            }

            return Ok(notification);
        }

        // POST: api/notifications
        [HttpPost]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<ActionResult<Notification>> CreateNotification(CreateNotificationDto dto)
        {
            // Validar que el remitente existe
            var sender = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == dto.SenderId && u.IsActive);
            if (sender == null)
                return BadRequest("Remitente no encontrado o inactivo");

            // Validar que el destinatario existe
            var receiver = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == dto.ReceiverId && u.IsActive);
            if (receiver == null)
                return BadRequest("Destinatario no encontrado o inactivo");

            var notification = new Notification
            {
                SenderId = dto.SenderId,
                ReceiverId = dto.ReceiverId,
                Title = dto.Title,
                Message = dto.Message,
                Type = dto.Type,
                Status = NotificationStatus.Unread,
                RelatedEntityType = dto.RelatedEntityType,
                RelatedEntityId = dto.RelatedEntityId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetNotification), new { id = notification.Id }, notification);
        }

        // POST: api/notifications/bulk
        [HttpPost("bulk")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<ActionResult<object>> CreateBulkNotifications(CreateBulkNotificationDto dto)
        {
            // Validar que el remitente existe
            var sender = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == dto.SenderId && u.IsActive);
            if (sender == null)
                return BadRequest("Remitente no encontrado o inactivo");

            var results = new List<object>();
            var errors = new List<string>();

            foreach (var receiverId in dto.ReceiverIds)
            {
                try
                {
                    // Validar que el destinatario existe
                    var receiver = await _context.Users
                        .FirstOrDefaultAsync(u => u.Id == receiverId && u.IsActive);
                    if (receiver == null)
                    {
                        errors.Add($"Destinatario ID {receiverId} no encontrado o inactivo");
                        continue;
                    }

                    var notification = new Notification
                    {
                        SenderId = dto.SenderId,
                        ReceiverId = receiverId,
                        Title = dto.Title,
                        Message = dto.Message,
                        Type = dto.Type,
                        Status = NotificationStatus.Unread,
                        RelatedEntityType = dto.RelatedEntityType,
                        RelatedEntityId = dto.RelatedEntityId,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.Notifications.Add(notification);
                    results.Add(new { ReceiverId = receiverId, Status = "Created" });
                }
                catch (Exception ex)
                {
                    errors.Add($"Error procesando destinatario ID {receiverId}: {ex.Message}");
                }
            }

            if (results.Any())
            {
                await _context.SaveChangesAsync();
            }

            return Ok(new
            {
                Created = results.Count,
                Errors = errors,
                Results = results
            });
        }

        // PUT: api/notifications/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> UpdateNotification(int id, UpdateNotificationDto dto)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification == null || !notification.IsActive)
                return NotFound("Notificación no encontrada");

            // Actualizar campos
            if (!string.IsNullOrEmpty(dto.Title)) notification.Title = dto.Title;
            if (dto.Message != null) notification.Message = dto.Message;
            if (dto.Type.HasValue) notification.Type = dto.Type.Value;
            if (dto.Status.HasValue) notification.Status = dto.Status.Value;
            if (dto.RelatedEntityType != null) notification.RelatedEntityType = dto.RelatedEntityType;
            if (dto.RelatedEntityId.HasValue) notification.RelatedEntityId = dto.RelatedEntityId.Value;

            notification.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/notifications/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeactivateNotification(int id)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification == null || !notification.IsActive)
                return NotFound("Notificación no encontrada");

            notification.IsActive = false;
            notification.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // PUT: api/notifications/5/read
        [HttpPut("{id}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var currentUserId = GetCurrentUserId();
            var isAdmin = User.IsInRole("Admin");

            var notification = await _context.Notifications.FindAsync(id);
            if (notification == null || !notification.IsActive)
                return NotFound("Notificación no encontrada");

            // Verificar que el usuario puede marcar esta notificación como leída
            if (!isAdmin && notification.ReceiverId != currentUserId)
            {
                return Forbid();
            }

            notification.Status = NotificationStatus.Read;
            notification.ReadAt = DateTime.UtcNow;
            notification.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // PUT: api/notifications/read-all
        [HttpPut("read-all")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var currentUserId = GetCurrentUserId();

            var unreadNotifications = await _context.Notifications
                .Where(n => n.ReceiverId == currentUserId && 
                           n.Status == NotificationStatus.Unread && 
                           n.IsActive)
                .ToListAsync();

            foreach (var notification in unreadNotifications)
            {
                notification.Status = NotificationStatus.Read;
                notification.ReadAt = DateTime.UtcNow;
                notification.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return Ok(new { MarkedAsRead = unreadNotifications.Count });
        }

        // GET: api/notifications/unread
        [HttpGet("unread")]
        public async Task<ActionResult<IEnumerable<object>>> GetUnreadNotifications()
        {
            var currentUserId = GetCurrentUserId();

            var notifications = await _context.Notifications
                .Include(n => n.Sender)
                .Where(n => n.ReceiverId == currentUserId && 
                           n.Status == NotificationStatus.Unread && 
                           n.IsActive)
                .OrderByDescending(n => n.CreatedAt)
                .Select(n => new
                {
                    n.Id,
                    n.Title,
                    n.Message,
                    n.Type,
                    n.CreatedAt,
                    Sender = n.Sender != null ? new
                    {
                        n.Sender.Id,
                        n.Sender.FirstName,
                        n.Sender.LastName
                    } : null
                })
                .ToListAsync();

            return Ok(notifications);
        }

        // GET: api/notifications/unread-count
        [HttpGet("unread-count")]
        public async Task<ActionResult<object>> GetUnreadCount()
        {
            var currentUserId = GetCurrentUserId();

            var count = await _context.Notifications
                .CountAsync(n => n.ReceiverId == currentUserId && 
                               n.Status == NotificationStatus.Unread && 
                               n.IsActive);

            return Ok(new { UnreadCount = count });
        }

        // GET: api/notifications/my
        [HttpGet("my")]
        public async Task<ActionResult<IEnumerable<object>>> GetMyNotifications(
            [FromQuery] NotificationType? type = null,
            [FromQuery] NotificationStatus? status = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var currentUserId = GetCurrentUserId();

            var query = _context.Notifications
                .Include(n => n.Sender)
                .Where(n => n.ReceiverId == currentUserId && n.IsActive);

            if (type.HasValue)
                query = query.Where(n => n.Type == type);
            if (status.HasValue)
                query = query.Where(n => n.Status == status);

            var totalCount = await query.CountAsync();

            var notifications = await query
                .OrderByDescending(n => n.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(n => new
                {
                    n.Id,
                    n.Title,
                    n.Message,
                    n.Type,
                    n.Status,
                    n.ReadAt,
                    n.RelatedEntityType,
                    n.RelatedEntityId,
                    n.CreatedAt,
                    Sender = n.Sender != null ? new
                    {
                        n.Sender.Id,
                        n.Sender.FirstName,
                        n.Sender.LastName
                    } : null
                })
                .ToListAsync();

            return Ok(new
            {
                Notifications = notifications,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            });
        }

        // GET: api/notifications/sent
        [HttpGet("sent")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<ActionResult<IEnumerable<object>>> GetSentNotifications(
            [FromQuery] NotificationType? type = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            var currentUserId = GetCurrentUserId();

            var query = _context.Notifications
                .Include(n => n.Receiver)
                .Where(n => n.SenderId == currentUserId && n.IsActive);

            if (type.HasValue)
                query = query.Where(n => n.Type == type);
            if (startDate.HasValue)
                query = query.Where(n => n.CreatedAt >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(n => n.CreatedAt <= endDate.Value);

            var notifications = await query
                .OrderByDescending(n => n.CreatedAt)
                .Select(n => new
                {
                    n.Id,
                    n.Title,
                    n.Message,
                    n.Type,
                    n.Status,
                    n.CreatedAt,
                    Receiver = n.Receiver != null ? new
                    {
                        n.Receiver.Id,
                        n.Receiver.FirstName,
                        n.Receiver.LastName,
                        n.Receiver.Email
                    } : null
                })
                .ToListAsync();

            return Ok(notifications);
        }

        // GET: api/notifications/stats
        [HttpGet("stats")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<object>> GetNotificationStats()
        {
            var totalNotifications = await _context.Notifications.CountAsync(n => n.IsActive);
            var unreadNotifications = await _context.Notifications.CountAsync(n => n.Status == NotificationStatus.Unread && n.IsActive);
            var todayNotifications = await _context.Notifications.CountAsync(n => n.CreatedAt >= DateTime.Today && n.IsActive);

            var notificationsByType = await _context.Notifications
                .Where(n => n.IsActive)
                .GroupBy(n => n.Type)
                .Select(g => new
                {
                    Type = g.Key,
                    Count = g.Count(),
                    UnreadCount = g.Count(n => n.Status == NotificationStatus.Unread)
                })
                .ToListAsync();

            var notificationsByStatus = await _context.Notifications
                .Where(n => n.IsActive)
                .GroupBy(n => n.Status)
                .Select(g => new
                {
                    Status = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();

            var monthlyNotifications = await _context.Notifications
                .Where(n => n.IsActive && n.CreatedAt >= DateTime.Today.AddMonths(-6))
                .GroupBy(n => new { n.CreatedAt.Year, n.CreatedAt.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Count = g.Count()
                })
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .ToListAsync();

            return Ok(new
            {
                TotalNotifications = totalNotifications,
                UnreadNotifications = unreadNotifications,
                TodayNotifications = todayNotifications,
                NotificationsByType = notificationsByType,
                NotificationsByStatus = notificationsByStatus,
                MonthlyTrend = monthlyNotifications
            });
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            return userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
        }
    }

    public class CreateNotificationDto
    {
        [Required]
        public int SenderId { get; set; }

        [Required]
        public int ReceiverId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        public string Message { get; set; } = string.Empty;

        [Required]
        public NotificationType Type { get; set; }

        [StringLength(100)]
        public string? RelatedEntityType { get; set; }

        public int? RelatedEntityId { get; set; }
    }

    public class CreateBulkNotificationDto
    {
        [Required]
        public int SenderId { get; set; }

        [Required]
        public List<int> ReceiverIds { get; set; } = new();

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        public string Message { get; set; } = string.Empty;

        [Required]
        public NotificationType Type { get; set; }

        [StringLength(100)]
        public string? RelatedEntityType { get; set; }

        public int? RelatedEntityId { get; set; }
    }

    public class UpdateNotificationDto
    {
        [StringLength(200)]
        public string? Title { get; set; }

        [StringLength(1000)]
        public string? Message { get; set; }

        public NotificationType? Type { get; set; }
        public NotificationStatus? Status { get; set; }
        public string? RelatedEntityType { get; set; }
        public int? RelatedEntityId { get; set; }
    }
} 