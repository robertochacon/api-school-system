using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using api_school_system.Data;
using api_school_system.Models;
using System.ComponentModel.DataAnnotations;

namespace api_school_system.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AcademicEventsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AcademicEventsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/academicevents
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetAcademicEvents(
            [FromQuery] int? academicPeriodId = null,
            [FromQuery] EventType? type = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] bool? isAllDay = null)
        {
            var query = _context.AcademicEvents
                .Include(e => e.AcademicPeriod)
                .Where(e => e.IsActive);

            if (academicPeriodId.HasValue)
                query = query.Where(e => e.AcademicPeriodId == academicPeriodId);
            if (type.HasValue)
                query = query.Where(e => e.Type == type);
            if (startDate.HasValue)
                query = query.Where(e => e.StartDate >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(e => e.EndDate <= endDate.Value);
            if (isAllDay.HasValue)
                query = query.Where(e => e.IsAllDay == isAllDay.Value);

            var events = await query
                .OrderBy(e => e.StartDate)
                .Select(e => new
                {
                    e.Id,
                    e.AcademicPeriodId,
                    e.Title,
                    e.Description,
                    e.Type,
                    e.StartDate,
                    e.EndDate,
                    e.IsAllDay,
                    e.Location,
                    e.Notes,
                    e.IsActive,
                    e.CreatedAt,
                    e.UpdatedAt,
                    AcademicPeriod = new
                    {
                        e.AcademicPeriod.Id,
                        e.AcademicPeriod.Name,
                        e.AcademicPeriod.Code
                    }
                })
                .ToListAsync();

            return Ok(events);
        }

        // GET: api/academicevents/5
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetAcademicEvent(int id)
        {
            var academicEvent = await _context.AcademicEvents
                .Include(e => e.AcademicPeriod)
                .Where(e => e.Id == id && e.IsActive)
                .Select(e => new
                {
                    e.Id,
                    e.AcademicPeriodId,
                    e.Title,
                    e.Description,
                    e.Type,
                    e.StartDate,
                    e.EndDate,
                    e.IsAllDay,
                    e.Location,
                    e.Notes,
                    e.IsActive,
                    e.CreatedAt,
                    e.UpdatedAt,
                    AcademicPeriod = new
                    {
                        e.AcademicPeriod.Id,
                        e.AcademicPeriod.Name,
                        e.AcademicPeriod.Code,
                        e.AcademicPeriod.Description
                    }
                })
                .FirstOrDefaultAsync();

            if (academicEvent == null)
            {
                return NotFound("Evento académico no encontrado");
            }

            return Ok(academicEvent);
        }

        // POST: api/academicevents
        [HttpPost]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<ActionResult<AcademicEvent>> CreateAcademicEvent(CreateAcademicEventDto dto)
        {
            // Validar que el período académico existe
            var academicPeriod = await _context.AcademicPeriods
                .FirstOrDefaultAsync(p => p.Id == dto.AcademicPeriodId && p.IsActive);
            if (academicPeriod == null)
                return BadRequest("Período académico no encontrado o inactivo");

            // Validar que las fechas sean coherentes
            if (dto.StartDate >= dto.EndDate)
                return BadRequest("La fecha de inicio debe ser anterior a la fecha de fin");

            // Verificar que el evento esté dentro del período académico
            if (dto.StartDate < academicPeriod.StartDate || dto.EndDate > academicPeriod.EndDate)
                return BadRequest("El evento debe estar dentro del período académico");

            // Verificar conflictos de eventos en el mismo período y fechas
            var conflictingEvent = await _context.AcademicEvents
                .Where(e => e.AcademicPeriodId == dto.AcademicPeriodId && 
                           e.IsActive &&
                           ((e.StartDate <= dto.StartDate && e.EndDate > dto.StartDate) ||
                            (e.StartDate < dto.EndDate && e.EndDate >= dto.EndDate) ||
                            (e.StartDate >= dto.StartDate && e.EndDate <= dto.EndDate)))
                .FirstOrDefaultAsync();

            if (conflictingEvent != null)
                return BadRequest("Existe un evento que se solapa con las fechas especificadas");

            var academicEvent = new AcademicEvent
            {
                AcademicPeriodId = dto.AcademicPeriodId,
                Title = dto.Title,
                Description = dto.Description,
                Type = dto.Type,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                IsAllDay = dto.IsAllDay,
                Location = dto.Location,
                Notes = dto.Notes,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.AcademicEvents.Add(academicEvent);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAcademicEvent), new { id = academicEvent.Id }, academicEvent);
        }

        // PUT: api/academicevents/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> UpdateAcademicEvent(int id, UpdateAcademicEventDto dto)
        {
            var academicEvent = await _context.AcademicEvents.FindAsync(id);
            if (academicEvent == null || !academicEvent.IsActive)
                return NotFound("Evento académico no encontrado");

            // Validar que el período académico existe (si se está cambiando)
            if (dto.AcademicPeriodId.HasValue)
            {
                var academicPeriod = await _context.AcademicPeriods
                    .FirstOrDefaultAsync(p => p.Id == dto.AcademicPeriodId.Value && p.IsActive);
                if (academicPeriod == null)
                    return BadRequest("Período académico no encontrado o inactivo");
            }

            // Validar que las fechas sean coherentes
            var startDate = dto.StartDate ?? academicEvent.StartDate;
            var endDate = dto.EndDate ?? academicEvent.EndDate;
            if (startDate >= endDate)
                return BadRequest("La fecha de inicio debe ser anterior a la fecha de fin");

            // Verificar que el evento esté dentro del período académico
            var periodId = dto.AcademicPeriodId ?? academicEvent.AcademicPeriodId;
            var period = await _context.AcademicPeriods.FindAsync(periodId);
            if (startDate < period.StartDate || endDate > period.EndDate)
                return BadRequest("El evento debe estar dentro del período académico");

            // Verificar conflictos de eventos (excluyendo el evento actual)
            var conflictingEvent = await _context.AcademicEvents
                .Where(e => e.Id != id &&
                           e.AcademicPeriodId == periodId && 
                           e.IsActive &&
                           ((e.StartDate <= startDate && e.EndDate > startDate) ||
                            (e.StartDate < endDate && e.EndDate >= endDate) ||
                            (e.StartDate >= startDate && e.EndDate <= endDate)))
                .FirstOrDefaultAsync();

            if (conflictingEvent != null)
                return BadRequest("Existe un evento que se solapa con las fechas especificadas");

            // Actualizar campos
            if (dto.AcademicPeriodId.HasValue) academicEvent.AcademicPeriodId = dto.AcademicPeriodId.Value;
            if (!string.IsNullOrEmpty(dto.Title)) academicEvent.Title = dto.Title;
            if (dto.Description != null) academicEvent.Description = dto.Description;
            if (dto.Type.HasValue) academicEvent.Type = dto.Type.Value;
            if (dto.StartDate.HasValue) academicEvent.StartDate = dto.StartDate.Value;
            if (dto.EndDate.HasValue) academicEvent.EndDate = dto.EndDate.Value;
            if (dto.IsAllDay.HasValue) academicEvent.IsAllDay = dto.IsAllDay.Value;
            if (dto.Location != null) academicEvent.Location = dto.Location;
            if (dto.Notes != null) academicEvent.Notes = dto.Notes;

            academicEvent.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/academicevents/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeactivateAcademicEvent(int id)
        {
            var academicEvent = await _context.AcademicEvents.FindAsync(id);
            if (academicEvent == null || !academicEvent.IsActive)
                return NotFound("Evento académico no encontrado");

            academicEvent.IsActive = false;
            academicEvent.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/academicevents/calendar
        [HttpGet("calendar")]
        public async Task<ActionResult<IEnumerable<object>>> GetCalendarEvents(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            [FromQuery] int? academicPeriodId = null)
        {
            var query = _context.AcademicEvents
                .Include(e => e.AcademicPeriod)
                .Where(e => e.IsActive &&
                           ((e.StartDate >= startDate && e.StartDate <= endDate) ||
                            (e.EndDate >= startDate && e.EndDate <= endDate) ||
                            (e.StartDate <= startDate && e.EndDate >= endDate)));

            if (academicPeriodId.HasValue)
                query = query.Where(e => e.AcademicPeriodId == academicPeriodId);

            var events = await query
                .OrderBy(e => e.StartDate)
                .Select(e => new
                {
                    e.Id,
                    e.Title,
                    e.Description,
                    e.Type,
                    e.StartDate,
                    e.EndDate,
                    e.IsAllDay,
                    e.Location,
                    e.Notes,
                    AcademicPeriod = new
                    {
                        e.AcademicPeriod.Id,
                        e.AcademicPeriod.Name,
                        e.AcademicPeriod.Code
                    }
                })
                .ToListAsync();

            return Ok(events);
        }

        // GET: api/academicevents/upcoming
        [HttpGet("upcoming")]
        public async Task<ActionResult<IEnumerable<object>>> GetUpcomingEvents(
            [FromQuery] int days = 30,
            [FromQuery] int? academicPeriodId = null)
        {
            var startDate = DateTime.Today;
            var endDate = startDate.AddDays(days);

            var query = _context.AcademicEvents
                .Include(e => e.AcademicPeriod)
                .Where(e => e.IsActive && e.StartDate >= startDate && e.StartDate <= endDate);

            if (academicPeriodId.HasValue)
                query = query.Where(e => e.AcademicPeriodId == academicPeriodId);

            var events = await query
                .OrderBy(e => e.StartDate)
                .Select(e => new
                {
                    e.Id,
                    e.Title,
                    e.Description,
                    e.Type,
                    e.StartDate,
                    e.EndDate,
                    e.IsAllDay,
                    e.Location,
                    DaysUntil = (e.StartDate - startDate).Days,
                    AcademicPeriod = new
                    {
                        e.AcademicPeriod.Id,
                        e.AcademicPeriod.Name,
                        e.AcademicPeriod.Code
                    }
                })
                .Take(20)
                .ToListAsync();

            return Ok(events);
        }

        // GET: api/academicevents/today
        [HttpGet("today")]
        public async Task<ActionResult<IEnumerable<object>>> GetTodayEvents(
            [FromQuery] int? academicPeriodId = null)
        {
            var today = DateTime.Today;

            var query = _context.AcademicEvents
                .Include(e => e.AcademicPeriod)
                .Where(e => e.IsActive &&
                           e.StartDate <= today && e.EndDate >= today);

            if (academicPeriodId.HasValue)
                query = query.Where(e => e.AcademicPeriodId == academicPeriodId);

            var events = await query
                .OrderBy(e => e.StartDate)
                .Select(e => new
                {
                    e.Id,
                    e.Title,
                    e.Description,
                    e.Type,
                    e.StartDate,
                    e.EndDate,
                    e.IsAllDay,
                    e.Location,
                    e.Notes,
                    AcademicPeriod = new
                    {
                        e.AcademicPeriod.Id,
                        e.AcademicPeriod.Name,
                        e.AcademicPeriod.Code
                    }
                })
                .ToListAsync();

            return Ok(events);
        }

        // GET: api/academicevents/period/{academicPeriodId}
        [HttpGet("period/{academicPeriodId}")]
        public async Task<ActionResult<IEnumerable<object>>> GetPeriodEvents(int academicPeriodId)
        {
            var events = await _context.AcademicEvents
                .Include(e => e.AcademicPeriod)
                .Where(e => e.AcademicPeriodId == academicPeriodId && e.IsActive)
                .OrderBy(e => e.StartDate)
                .Select(e => new
                {
                    e.Id,
                    e.Title,
                    e.Description,
                    e.Type,
                    e.StartDate,
                    e.EndDate,
                    e.IsAllDay,
                    e.Location,
                    e.Notes,
                    e.CreatedAt,
                    e.UpdatedAt
                })
                .ToListAsync();

            return Ok(events);
        }

        // GET: api/academicevents/type/{type}
        [HttpGet("type/{type}")]
        public async Task<ActionResult<IEnumerable<object>>> GetEventsByType(EventType type,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            var query = _context.AcademicEvents
                .Include(e => e.AcademicPeriod)
                .Where(e => e.Type == type && e.IsActive);

            if (startDate.HasValue)
                query = query.Where(e => e.StartDate >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(e => e.EndDate <= endDate.Value);

            var events = await query
                .OrderBy(e => e.StartDate)
                .Select(e => new
                {
                    e.Id,
                    e.Title,
                    e.Description,
                    e.StartDate,
                    e.EndDate,
                    e.IsAllDay,
                    e.Location,
                    e.Notes,
                    AcademicPeriod = new
                    {
                        e.AcademicPeriod.Id,
                        e.AcademicPeriod.Name,
                        e.AcademicPeriod.Code
                    }
                })
                .ToListAsync();

            return Ok(events);
        }

        // GET: api/academicevents/stats
        [HttpGet("stats")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<object>> GetAcademicEventStats()
        {
            var totalEvents = await _context.AcademicEvents.CountAsync(e => e.IsActive);
            var todayEvents = await _context.AcademicEvents.CountAsync(e => e.IsActive && 
                                                                          e.StartDate <= DateTime.Today && 
                                                                          e.EndDate >= DateTime.Today);

            var eventsByType = await _context.AcademicEvents
                .Where(e => e.IsActive)
                .GroupBy(e => e.Type)
                .Select(g => new
                {
                    Type = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();

            var eventsByPeriod = await _context.AcademicEvents
                .Include(e => e.AcademicPeriod)
                .Where(e => e.IsActive)
                .GroupBy(e => new { e.AcademicPeriodId, e.AcademicPeriod.Name, e.AcademicPeriod.Code })
                .Select(g => new
                {
                    PeriodId = g.Key.AcademicPeriodId,
                    PeriodName = g.Key.Name,
                    PeriodCode = g.Key.Code,
                    EventCount = g.Count()
                })
                .OrderByDescending(x => x.EventCount)
                .Take(10)
                .ToListAsync();

            var monthlyEvents = await _context.AcademicEvents
                .Where(e => e.IsActive && e.StartDate >= DateTime.Today.AddMonths(-6))
                .GroupBy(e => new { e.StartDate.Year, e.StartDate.Month })
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
                TotalEvents = totalEvents,
                TodayEvents = todayEvents,
                EventsByType = eventsByType,
                EventsByPeriod = eventsByPeriod,
                MonthlyTrend = monthlyEvents
            });
        }
    }

    public class CreateAcademicEventDto
    {
        [Required]
        public int AcademicPeriodId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        public EventType Type { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        public bool IsAllDay { get; set; } = false;

        [StringLength(200)]
        public string? Location { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }
    }

    public class UpdateAcademicEventDto
    {
        public int? AcademicPeriodId { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public EventType? Type { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool? IsAllDay { get; set; }
        public string? Location { get; set; }
        public string? Notes { get; set; }
    }
} 