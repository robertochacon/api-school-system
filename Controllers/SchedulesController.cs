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
    public class SchedulesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SchedulesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/schedules
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetSchedules(
            [FromQuery] int? courseId = null,
            [FromQuery] int? subjectId = null,
            [FromQuery] int? teacherId = null,
            [FromQuery] Models.DayOfWeek? dayOfWeek = null)
        {
            var query = _context.Schedules
                .Include(s => s.Course)
                .Include(s => s.Subject)
                .Include(s => s.Teacher)
                    .ThenInclude(t => t.User)
                .Where(s => s.IsActive);

            if (courseId.HasValue)
                query = query.Where(s => s.CourseId == courseId);
            if (subjectId.HasValue)
                query = query.Where(s => s.SubjectId == subjectId);
            if (teacherId.HasValue)
                query = query.Where(s => s.TeacherId == teacherId);
            if (dayOfWeek.HasValue)
                query = query.Where(s => s.DayOfWeek == dayOfWeek);

            var schedules = await query
                .OrderBy(s => s.DayOfWeek)
                .ThenBy(s => s.StartTime)
                .Select(s => new
                {
                    s.Id,
                    s.DayOfWeek,
                    s.StartTime,
                    s.EndTime,
                    s.Room,
                    s.Notes,
                    s.IsActive,
                    s.CreatedAt,
                    s.UpdatedAt,
                    Course = new
                    {
                        s.Course.Id,
                        s.Course.Name,
                        s.Course.Section
                    },
                    Subject = new
                    {
                        s.Subject.Id,
                        s.Subject.Name,
                        s.Subject.Code
                    },
                    Teacher = new
                    {
                        s.Teacher.Id,
                        s.Teacher.User.FirstName,
                        s.Teacher.User.LastName,
                        s.Teacher.User.Email
                    }
                })
                .ToListAsync();

            return Ok(schedules);
        }

        // GET: api/schedules/5
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetSchedule(int id)
        {
            var schedule = await _context.Schedules
                .Include(s => s.Course)
                .Include(s => s.Subject)
                .Include(s => s.Teacher)
                    .ThenInclude(t => t.User)
                .Where(s => s.Id == id && s.IsActive)
                .Select(s => new
                {
                    s.Id,
                    s.CourseId,
                    s.SubjectId,
                    s.TeacherId,
                    s.DayOfWeek,
                    s.StartTime,
                    s.EndTime,
                    s.Room,
                    s.Notes,
                    s.IsActive,
                    s.CreatedAt,
                    s.UpdatedAt,
                    Course = new
                    {
                        s.Course.Id,
                        s.Course.Name,
                        s.Course.Section,
                        s.Course.Description
                    },
                    Subject = new
                    {
                        s.Subject.Id,
                        s.Subject.Name,
                        s.Subject.Code,
                        s.Subject.Description
                    },
                    Teacher = new
                    {
                        s.Teacher.Id,
                        s.Teacher.User.FirstName,
                        s.Teacher.User.LastName,
                        s.Teacher.User.Email,
                        s.Teacher.Specialization
                    }
                })
                .FirstOrDefaultAsync();

            if (schedule == null)
            {
                return NotFound("Horario no encontrado");
            }

            return Ok(schedule);
        }

        // POST: api/schedules
        [HttpPost]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<ActionResult<Schedule>> CreateSchedule(CreateScheduleDto dto)
        {
            // Validar que el curso existe
            var course = await _context.Courses.FindAsync(dto.CourseId);
            if (course == null || !course.IsActive)
                return BadRequest("Curso no encontrado o inactivo");

            // Validar que la materia existe
            var subject = await _context.Subjects.FindAsync(dto.SubjectId);
            if (subject == null || !subject.IsActive)
                return BadRequest("Materia no encontrada o inactiva");

            // Validar que el profesor existe
            var teacher = await _context.Teachers
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Id == dto.TeacherId && t.IsActive);
            if (teacher == null)
                return BadRequest("Profesor no encontrado o inactivo");

            // Verificar conflictos de horario para el profesor
            var teacherConflict = await _context.Schedules
                .Where(s => s.TeacherId == dto.TeacherId && 
                           s.DayOfWeek == dto.DayOfWeek &&
                           s.IsActive &&
                           ((s.StartTime <= dto.StartTime && s.EndTime > dto.StartTime) ||
                            (s.StartTime < dto.EndTime && s.EndTime >= dto.EndTime) ||
                            (s.StartTime >= dto.StartTime && s.EndTime <= dto.EndTime)))
                .FirstOrDefaultAsync();

            if (teacherConflict != null)
                return BadRequest("El profesor ya tiene una clase programada en este horario");

            // Verificar conflictos de horario para el curso
            var courseConflict = await _context.Schedules
                .Where(s => s.CourseId == dto.CourseId && 
                           s.DayOfWeek == dto.DayOfWeek &&
                           s.IsActive &&
                           ((s.StartTime <= dto.StartTime && s.EndTime > dto.StartTime) ||
                            (s.StartTime < dto.EndTime && s.EndTime >= dto.EndTime) ||
                            (s.StartTime >= dto.StartTime && s.EndTime <= dto.EndTime)))
                .FirstOrDefaultAsync();

            if (courseConflict != null)
                return BadRequest("El curso ya tiene una clase programada en este horario");

            var schedule = new Schedule
            {
                CourseId = dto.CourseId,
                SubjectId = dto.SubjectId,
                TeacherId = dto.TeacherId,
                DayOfWeek = dto.DayOfWeek,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                Room = dto.Room,
                Notes = dto.Notes,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Schedules.Add(schedule);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetSchedule), new { id = schedule.Id }, schedule);
        }

        // PUT: api/schedules/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> UpdateSchedule(int id, UpdateScheduleDto dto)
        {
            var schedule = await _context.Schedules.FindAsync(id);
            if (schedule == null || !schedule.IsActive)
                return NotFound("Horario no encontrado");

            // Validar que el curso existe
            if (dto.CourseId.HasValue)
            {
                var course = await _context.Courses.FindAsync(dto.CourseId.Value);
                if (course == null || !course.IsActive)
                    return BadRequest("Curso no encontrado o inactivo");
            }

            // Validar que la materia existe
            if (dto.SubjectId.HasValue)
            {
                var subject = await _context.Subjects.FindAsync(dto.SubjectId.Value);
                if (subject == null || !subject.IsActive)
                    return BadRequest("Materia no encontrada o inactiva");
            }

            // Validar que el profesor existe
            if (dto.TeacherId.HasValue)
            {
                var teacher = await _context.Teachers
                    .FirstOrDefaultAsync(t => t.Id == dto.TeacherId.Value && t.IsActive);
                if (teacher == null)
                    return BadRequest("Profesor no encontrado o inactivo");
            }

            // Verificar conflictos de horario (excluyendo el horario actual)
            if (dto.TeacherId.HasValue || dto.DayOfWeek.HasValue || dto.StartTime.HasValue || dto.EndTime.HasValue)
            {
                var teacherId = dto.TeacherId ?? schedule.TeacherId;
                var courseId = dto.CourseId ?? schedule.CourseId;
                var dayOfWeek = dto.DayOfWeek ?? schedule.DayOfWeek;
                var startTime = dto.StartTime ?? schedule.StartTime;
                var endTime = dto.EndTime ?? schedule.EndTime;

                // Verificar conflictos para el profesor
                var teacherConflict = await _context.Schedules
                    .Where(s => s.Id != id &&
                               s.TeacherId == teacherId && 
                               s.DayOfWeek == dayOfWeek &&
                               s.IsActive &&
                               ((s.StartTime <= startTime && s.EndTime > startTime) ||
                                (s.StartTime < endTime && s.EndTime >= endTime) ||
                                (s.StartTime >= startTime && s.EndTime <= endTime)))
                    .FirstOrDefaultAsync();

                if (teacherConflict != null)
                    return BadRequest("El profesor ya tiene una clase programada en este horario");

                // Verificar conflictos para el curso
                var courseConflict = await _context.Schedules
                    .Where(s => s.Id != id &&
                               s.CourseId == courseId && 
                               s.DayOfWeek == dayOfWeek &&
                               s.IsActive &&
                               ((s.StartTime <= startTime && s.EndTime > startTime) ||
                                (s.StartTime < endTime && s.EndTime >= endTime) ||
                                (s.StartTime >= startTime && s.EndTime <= endTime)))
                    .FirstOrDefaultAsync();

                if (courseConflict != null)
                    return BadRequest("El curso ya tiene una clase programada en este horario");
            }

            // Actualizar campos
            if (dto.CourseId.HasValue) schedule.CourseId = dto.CourseId.Value;
            if (dto.SubjectId.HasValue) schedule.SubjectId = dto.SubjectId.Value;
            if (dto.TeacherId.HasValue) schedule.TeacherId = dto.TeacherId.Value;
            if (dto.DayOfWeek.HasValue) schedule.DayOfWeek = dto.DayOfWeek.Value;
            if (dto.StartTime.HasValue) schedule.StartTime = dto.StartTime.Value;
            if (dto.EndTime.HasValue) schedule.EndTime = dto.EndTime.Value;
            if (dto.Room != null) schedule.Room = dto.Room;
            if (dto.Notes != null) schedule.Notes = dto.Notes;

            schedule.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/schedules/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeactivateSchedule(int id)
        {
            var schedule = await _context.Schedules.FindAsync(id);
            if (schedule == null || !schedule.IsActive)
                return NotFound("Horario no encontrado");

            schedule.IsActive = false;
            schedule.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/schedules/teacher/{teacherId}
        [HttpGet("teacher/{teacherId}")]
        public async Task<ActionResult<IEnumerable<object>>> GetTeacherSchedule(int teacherId)
        {
            var schedules = await _context.Schedules
                .Include(s => s.Course)
                .Include(s => s.Subject)
                .Where(s => s.TeacherId == teacherId && s.IsActive)
                .OrderBy(s => s.DayOfWeek)
                .ThenBy(s => s.StartTime)
                .Select(s => new
                {
                    s.Id,
                    s.DayOfWeek,
                    s.StartTime,
                    s.EndTime,
                    s.Room,
                    s.Notes,
                    Course = new
                    {
                        s.Course.Id,
                        s.Course.Name,
                        s.Course.Section
                    },
                    Subject = new
                    {
                        s.Subject.Id,
                        s.Subject.Name,
                        s.Subject.Code
                    }
                })
                .ToListAsync();

            return Ok(schedules);
        }

        // GET: api/schedules/course/{courseId}
        [HttpGet("course/{courseId}")]
        public async Task<ActionResult<IEnumerable<object>>> GetCourseSchedule(int courseId)
        {
            var schedules = await _context.Schedules
                .Include(s => s.Subject)
                .Include(s => s.Teacher)
                    .ThenInclude(t => t.User)
                .Where(s => s.CourseId == courseId && s.IsActive)
                .OrderBy(s => s.DayOfWeek)
                .ThenBy(s => s.StartTime)
                .Select(s => new
                {
                    s.Id,
                    s.DayOfWeek,
                    s.StartTime,
                    s.EndTime,
                    s.Room,
                    s.Notes,
                    Subject = new
                    {
                        s.Subject.Id,
                        s.Subject.Name,
                        s.Subject.Code
                    },
                    Teacher = new
                    {
                        s.Teacher.Id,
                        s.Teacher.User.FirstName,
                        s.Teacher.User.LastName
                    }
                })
                .ToListAsync();

            return Ok(schedules);
        }

        // GET: api/schedules/stats
        [HttpGet("stats")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<object>> GetScheduleStats()
        {
            var totalSchedules = await _context.Schedules.CountAsync(s => s.IsActive);
            var schedulesByDay = await _context.Schedules
                .Where(s => s.IsActive)
                .GroupBy(s => s.DayOfWeek)
                .Select(g => new
                {
                    DayOfWeek = g.Key,
                    Count = g.Count()
                })
                .OrderBy(x => x.DayOfWeek)
                .ToListAsync();

            var schedulesByTeacher = await _context.Schedules
                .Include(s => s.Teacher)
                    .ThenInclude(t => t.User)
                .Where(s => s.IsActive)
                .GroupBy(s => new { s.TeacherId, s.Teacher.User.FirstName, s.Teacher.User.LastName })
                .Select(g => new
                {
                    TeacherId = g.Key.TeacherId,
                    TeacherName = $"{g.Key.FirstName} {g.Key.LastName}",
                    ScheduleCount = g.Count()
                })
                .OrderByDescending(x => x.ScheduleCount)
                .Take(10)
                .ToListAsync();

            return Ok(new
            {
                TotalSchedules = totalSchedules,
                SchedulesByDay = schedulesByDay,
                TopTeachers = schedulesByTeacher
            });
        }
    }

    public class CreateScheduleDto
    {
        [Required]
        public int CourseId { get; set; }

        [Required]
        public int SubjectId { get; set; }

        [Required]
        public int TeacherId { get; set; }

        [Required]
        public Models.DayOfWeek DayOfWeek { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }

        [Required]
        [StringLength(50)]
        public string Room { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Notes { get; set; }
    }

    public class UpdateScheduleDto
    {
        public int? CourseId { get; set; }
        public int? SubjectId { get; set; }
        public int? TeacherId { get; set; }
        public Models.DayOfWeek? DayOfWeek { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public string? Room { get; set; }
        public string? Notes { get; set; }
    }
} 