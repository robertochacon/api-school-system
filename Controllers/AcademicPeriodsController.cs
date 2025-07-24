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
    public class AcademicPeriodsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AcademicPeriodsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/academicperiods
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetAcademicPeriods(
            [FromQuery] PeriodStatus? status = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            var query = _context.AcademicPeriods.Where(p => p.IsActive);

            if (status.HasValue)
                query = query.Where(p => p.Status == status);
            if (startDate.HasValue)
                query = query.Where(p => p.StartDate >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(p => p.EndDate <= endDate.Value);

            var periods = await query
                .OrderByDescending(p => p.StartDate)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Code,
                    p.Description,
                    p.StartDate,
                    p.EndDate,
                    p.Status,
                    p.IsActive,
                    p.CreatedAt,
                    p.UpdatedAt,
                    EnrollmentCount = p.Enrollments.Count(e => e.IsActive),
                    EvaluationCount = p.Evaluations.Count(e => e.IsActive),
                    EventCount = p.AcademicEvents.Count(e => e.IsActive)
                })
                .ToListAsync();

            return Ok(periods);
        }

        // GET: api/academicperiods/5
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetAcademicPeriod(int id)
        {
            var period = await _context.AcademicPeriods
                .Where(p => p.Id == id && p.IsActive)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Code,
                    p.Description,
                    p.StartDate,
                    p.EndDate,
                    p.Status,
                    p.IsActive,
                    p.CreatedAt,
                    p.UpdatedAt,
                    EnrollmentCount = p.Enrollments.Count(e => e.IsActive),
                    EvaluationCount = p.Evaluations.Count(e => e.IsActive),
                    EventCount = p.AcademicEvents.Count(e => e.IsActive),
                    Enrollments = p.Enrollments
                        .Where(e => e.IsActive)
                        .Take(10)
                        .Select(e => new
                        {
                            e.Id,
                            e.Status,
                            Student = new
                            {
                                e.Student.User.FirstName,
                                e.Student.User.LastName,
                                e.Student.StudentId
                            },
                            Course = new
                            {
                                e.Course.Name,
                                e.Course.Section
                            }
                        }),
                    Evaluations = p.Evaluations
                        .Where(e => e.IsActive)
                        .Take(10)
                        .Select(e => new
                        {
                            e.Id,
                            e.Name,
                            e.Type,
                            e.EvaluationDate,
                            Subject = new
                            {
                                e.Subject.Name,
                                e.Subject.Code
                            }
                        }),
                    Events = p.AcademicEvents
                        .Where(e => e.IsActive)
                        .Take(10)
                        .Select(e => new
                        {
                            e.Id,
                            e.Title,
                            e.Type,
                            e.StartDate,
                            e.EndDate
                        })
                })
                .FirstOrDefaultAsync();

            if (period == null)
            {
                return NotFound("Período académico no encontrado");
            }

            return Ok(period);
        }

        // POST: api/academicperiods
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<AcademicPeriod>> CreateAcademicPeriod(CreateAcademicPeriodDto dto)
        {
            // Validar que el código sea único
            var existingPeriod = await _context.AcademicPeriods
                .FirstOrDefaultAsync(p => p.Code == dto.Code && p.IsActive);
            if (existingPeriod != null)
                return BadRequest("Ya existe un período académico con este código");

            // Validar que las fechas sean coherentes
            if (dto.StartDate >= dto.EndDate)
                return BadRequest("La fecha de inicio debe ser anterior a la fecha de fin");

            // Verificar si hay períodos que se solapan
            var overlappingPeriod = await _context.AcademicPeriods
                .Where(p => p.IsActive &&
                           ((p.StartDate <= dto.StartDate && p.EndDate > dto.StartDate) ||
                            (p.StartDate < dto.EndDate && p.EndDate >= dto.EndDate) ||
                            (p.StartDate >= dto.StartDate && p.EndDate <= dto.EndDate)))
                .FirstOrDefaultAsync();

            if (overlappingPeriod != null)
                return BadRequest("Existe un período académico que se solapa con las fechas especificadas");

            var period = new AcademicPeriod
            {
                Name = dto.Name,
                Code = dto.Code,
                Description = dto.Description,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                Status = dto.Status,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.AcademicPeriods.Add(period);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAcademicPeriod), new { id = period.Id }, period);
        }

        // PUT: api/academicperiods/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateAcademicPeriod(int id, UpdateAcademicPeriodDto dto)
        {
            var period = await _context.AcademicPeriods.FindAsync(id);
            if (period == null || !period.IsActive)
                return NotFound("Período académico no encontrado");

            // Validar que el código sea único (si se está cambiando)
            if (!string.IsNullOrEmpty(dto.Code) && dto.Code != period.Code)
            {
                var existingPeriod = await _context.AcademicPeriods
                    .FirstOrDefaultAsync(p => p.Code == dto.Code && p.IsActive);
                if (existingPeriod != null)
                    return BadRequest("Ya existe un período académico con este código");
            }

            // Validar que las fechas sean coherentes
            var startDate = dto.StartDate ?? period.StartDate;
            var endDate = dto.EndDate ?? period.EndDate;
            if (startDate >= endDate)
                return BadRequest("La fecha de inicio debe ser anterior a la fecha de fin");

            // Verificar si hay períodos que se solapan (excluyendo el período actual)
            var overlappingPeriod = await _context.AcademicPeriods
                .Where(p => p.Id != id && p.IsActive &&
                           ((p.StartDate <= startDate && p.EndDate > startDate) ||
                            (p.StartDate < endDate && p.EndDate >= endDate) ||
                            (p.StartDate >= startDate && p.EndDate <= endDate)))
                .FirstOrDefaultAsync();

            if (overlappingPeriod != null)
                return BadRequest("Existe un período académico que se solapa con las fechas especificadas");

            // Actualizar campos
            if (!string.IsNullOrEmpty(dto.Name)) period.Name = dto.Name;
            if (!string.IsNullOrEmpty(dto.Code)) period.Code = dto.Code;
            if (dto.Description != null) period.Description = dto.Description;
            if (dto.StartDate.HasValue) period.StartDate = dto.StartDate.Value;
            if (dto.EndDate.HasValue) period.EndDate = dto.EndDate.Value;
            if (dto.Status.HasValue) period.Status = dto.Status.Value;

            period.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/academicperiods/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeactivateAcademicPeriod(int id)
        {
            var period = await _context.AcademicPeriods.FindAsync(id);
            if (period == null || !period.IsActive)
                return NotFound("Período académico no encontrado");

            // Verificar si hay matrículas activas en este período
            var activeEnrollments = await _context.Enrollments
                .CountAsync(e => e.AcademicPeriodId == id && e.IsActive && e.Status == EnrollmentStatus.Active);
            if (activeEnrollments > 0)
                return BadRequest("No se puede desactivar un período académico que tiene matrículas activas");

            period.IsActive = false;
            period.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/academicperiods/current
        [HttpGet("current")]
        public async Task<ActionResult<object>> GetCurrentPeriod()
        {
            var currentDate = DateTime.Today;
            var currentPeriod = await _context.AcademicPeriods
                .Where(p => p.IsActive && 
                           p.StartDate <= currentDate && 
                           p.EndDate >= currentDate)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Code,
                    p.Description,
                    p.StartDate,
                    p.EndDate,
                    p.Status,
                    DaysRemaining = (p.EndDate - currentDate).Days,
                    EnrollmentCount = p.Enrollments.Count(e => e.IsActive),
                    EvaluationCount = p.Evaluations.Count(e => e.IsActive)
                })
                .FirstOrDefaultAsync();

            if (currentPeriod == null)
            {
                return NotFound("No hay un período académico activo en la fecha actual");
            }

            return Ok(currentPeriod);
        }

        // GET: api/academicperiods/upcoming
        [HttpGet("upcoming")]
        public async Task<ActionResult<IEnumerable<object>>> GetUpcomingPeriods()
        {
            var currentDate = DateTime.Today;
            var upcomingPeriods = await _context.AcademicPeriods
                .Where(p => p.IsActive && p.StartDate > currentDate)
                .OrderBy(p => p.StartDate)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Code,
                    p.Description,
                    p.StartDate,
                    p.EndDate,
                    p.Status,
                    DaysUntilStart = (p.StartDate - currentDate).Days,
                    Duration = (p.EndDate - p.StartDate).Days
                })
                .Take(5)
                .ToListAsync();

            return Ok(upcomingPeriods);
        }

        // GET: api/academicperiods/active
        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<object>>> GetActivePeriods()
        {
            var activePeriods = await _context.AcademicPeriods
                .Where(p => p.IsActive && p.Status == PeriodStatus.Active)
                .OrderByDescending(p => p.StartDate)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Code,
                    p.Description,
                    p.StartDate,
                    p.EndDate,
                    EnrollmentCount = p.Enrollments.Count(e => e.IsActive),
                    EvaluationCount = p.Evaluations.Count(e => e.IsActive)
                })
                .ToListAsync();

            return Ok(activePeriods);
        }

        // GET: api/academicperiods/{id}/enrollments
        [HttpGet("{id}/enrollments")]
        public async Task<ActionResult<IEnumerable<object>>> GetPeriodEnrollments(int id)
        {
            var enrollments = await _context.Enrollments
                .Include(e => e.Student)
                    .ThenInclude(s => s.User)
                .Include(e => e.Course)
                .Include(e => e.Grade)
                .Where(e => e.AcademicPeriodId == id && e.IsActive)
                .OrderBy(e => e.Student.User.LastName)
                .ThenBy(e => e.Student.User.FirstName)
                .Select(e => new
                {
                    e.Id,
                    e.EnrollmentDate,
                    e.Status,
                    e.Notes,
                    Student = new
                    {
                        e.Student.Id,
                        e.Student.User.FirstName,
                        e.Student.User.LastName,
                        e.Student.StudentId
                    },
                    Course = new
                    {
                        e.Course.Id,
                        e.Course.Name,
                        e.Course.Section
                    },
                    Grade = new
                    {
                        e.Grade.Id,
                        e.Grade.Name,
                        e.Grade.Level
                    }
                })
                .ToListAsync();

            return Ok(enrollments);
        }

        // GET: api/academicperiods/{id}/evaluations
        [HttpGet("{id}/evaluations")]
        public async Task<ActionResult<IEnumerable<object>>> GetPeriodEvaluations(int id)
        {
            var evaluations = await _context.Evaluations
                .Include(e => e.Subject)
                .Where(e => e.AcademicPeriodId == id && e.IsActive)
                .OrderBy(e => e.EvaluationDate)
                .Select(e => new
                {
                    e.Id,
                    e.Name,
                    e.Description,
                    e.Type,
                    e.Weight,
                    e.DueDate,
                    e.EvaluationDate,
                    e.MaxScore,
                    Subject = new
                    {
                        e.Subject.Id,
                        e.Subject.Name,
                        e.Subject.Code
                    }
                })
                .ToListAsync();

            return Ok(evaluations);
        }

        // GET: api/academicperiods/{id}/events
        [HttpGet("{id}/events")]
        public async Task<ActionResult<IEnumerable<object>>> GetPeriodEvents(int id)
        {
            var events = await _context.AcademicEvents
                .Where(e => e.AcademicPeriodId == id && e.IsActive)
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
                    e.Notes
                })
                .ToListAsync();

            return Ok(events);
        }

        // GET: api/academicperiods/stats
        [HttpGet("stats")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<object>> GetAcademicPeriodStats()
        {
            var totalPeriods = await _context.AcademicPeriods.CountAsync(p => p.IsActive);
            var activePeriods = await _context.AcademicPeriods.CountAsync(p => p.IsActive && p.Status == PeriodStatus.Active);
            var completedPeriods = await _context.AcademicPeriods.CountAsync(p => p.IsActive && p.Status == PeriodStatus.Completed);

            var currentDate = DateTime.Today;
            var currentPeriod = await _context.AcademicPeriods
                .Where(p => p.IsActive && p.StartDate <= currentDate && p.EndDate >= currentDate)
                .FirstOrDefaultAsync();

            var periodsByYear = await _context.AcademicPeriods
                .Where(p => p.IsActive)
                .GroupBy(p => p.StartDate.Year)
                .Select(g => new
                {
                    Year = g.Key,
                    Count = g.Count(),
                    TotalEnrollments = g.Sum(p => p.Enrollments.Count(e => e.IsActive)),
                    TotalEvaluations = g.Sum(p => p.Evaluations.Count(e => e.IsActive))
                })
                .OrderByDescending(x => x.Year)
                .ToListAsync();

            return Ok(new
            {
                TotalPeriods = totalPeriods,
                ActivePeriods = activePeriods,
                CompletedPeriods = completedPeriods,
                CurrentPeriod = currentPeriod != null ? new
                {
                    currentPeriod.Id,
                    currentPeriod.Name,
                    currentPeriod.Code,
                    DaysRemaining = (currentPeriod.EndDate - currentDate).Days
                } : null,
                PeriodsByYear = periodsByYear
            });
        }
    }

    public class CreateAcademicPeriodDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Code { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        public PeriodStatus Status { get; set; }
    }

    public class UpdateAcademicPeriodDto
    {
        [StringLength(100)]
        public string? Name { get; set; }

        [StringLength(20)]
        public string? Code { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public PeriodStatus? Status { get; set; }
    }
} 