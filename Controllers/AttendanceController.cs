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
    public class AttendanceController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AttendanceController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/attendance
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetAttendance(
            [FromQuery] int? studentId = null,
            [FromQuery] int? enrollmentId = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] AttendanceStatus? status = null)
        {
            var query = _context.Attendances
                .Include(a => a.Student)
                    .ThenInclude(s => s.User)
                .Include(a => a.Enrollment)
                    .ThenInclude(e => e.Course)
                .Where(a => a.IsActive);

            if (studentId.HasValue)
                query = query.Where(a => a.StudentId == studentId);
            if (enrollmentId.HasValue)
                query = query.Where(a => a.EnrollmentId == enrollmentId);
            if (startDate.HasValue)
                query = query.Where(a => a.Date >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(a => a.Date <= endDate.Value);
            if (status.HasValue)
                query = query.Where(a => a.Status == status);

            var attendance = await query
                .OrderByDescending(a => a.Date)
                .Select(a => new
                {
                    a.Id,
                    a.StudentId,
                    a.EnrollmentId,
                    a.Date,
                    a.Status,
                    a.Justification,
                    a.Notes,
                    a.IsActive,
                    a.CreatedAt,
                    a.UpdatedAt,
                    Student = new
                    {
                        a.Student.Id,
                        a.Student.User.FirstName,
                        a.Student.User.LastName,
                        a.Student.StudentId
                    },
                    Course = new
                    {
                        a.Enrollment.Course.Id,
                        a.Enrollment.Course.Name,
                        a.Enrollment.Course.Section
                    }
                })
                .ToListAsync();

            return Ok(attendance);
        }

        // GET: api/attendance/5
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetAttendanceRecord(int id)
        {
            var attendance = await _context.Attendances
                .Include(a => a.Student)
                    .ThenInclude(s => s.User)
                .Include(a => a.Enrollment)
                    .ThenInclude(e => e.Course)
                .Where(a => a.Id == id && a.IsActive)
                .Select(a => new
                {
                    a.Id,
                    a.StudentId,
                    a.EnrollmentId,
                    a.Date,
                    a.Status,
                    a.Justification,
                    a.Notes,
                    a.IsActive,
                    a.CreatedAt,
                    a.UpdatedAt,
                    Student = new
                    {
                        a.Student.Id,
                        a.Student.User.FirstName,
                        a.Student.User.LastName,
                        a.Student.User.Email,
                        a.Student.StudentId
                    },
                    Enrollment = new
                    {
                        a.Enrollment.Id,
                        a.Enrollment.Status,
                        Course = new
                        {
                            a.Enrollment.Course.Id,
                            a.Enrollment.Course.Name,
                            a.Enrollment.Course.Section
                        }
                    }
                })
                .FirstOrDefaultAsync();

            if (attendance == null)
            {
                return NotFound("Registro de asistencia no encontrado");
            }

            return Ok(attendance);
        }

        // POST: api/attendance
        [HttpPost]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<ActionResult<Attendance>> CreateAttendance(CreateAttendanceDto dto)
        {
            // Validar que el estudiante existe
            var student = await _context.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == dto.StudentId && s.IsActive);
            if (student == null)
                return BadRequest("Estudiante no encontrado o inactivo");

            // Validar que la matrícula existe y está activa
            var enrollment = await _context.Enrollments
                .Include(e => e.Course)
                .FirstOrDefaultAsync(e => e.Id == dto.EnrollmentId && e.IsActive);
            if (enrollment == null)
                return BadRequest("Matrícula no encontrada o inactiva");

            // Verificar que el estudiante pertenece a la matrícula
            if (enrollment.StudentId != dto.StudentId)
                return BadRequest("El estudiante no pertenece a esta matrícula");

            // Verificar que no existe un registro de asistencia para la misma fecha y matrícula
            var existingAttendance = await _context.Attendances
                .FirstOrDefaultAsync(a => a.EnrollmentId == dto.EnrollmentId && 
                                        a.Date == dto.Date && 
                                        a.IsActive);
            if (existingAttendance != null)
                return BadRequest("Ya existe un registro de asistencia para esta fecha y matrícula");

            var attendance = new Attendance
            {
                StudentId = dto.StudentId,
                EnrollmentId = dto.EnrollmentId,
                Date = dto.Date,
                Status = dto.Status,
                Justification = dto.Justification,
                Notes = dto.Notes,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Attendances.Add(attendance);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAttendanceRecord), new { id = attendance.Id }, attendance);
        }

        // POST: api/attendance/bulk
        [HttpPost("bulk")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<ActionResult<object>> CreateBulkAttendance(CreateBulkAttendanceDto dto)
        {
            var results = new List<object>();
            var errors = new List<string>();

            foreach (var record in dto.Records)
            {
                try
                {
                    // Validar que el estudiante existe
                    var student = await _context.Students
                        .FirstOrDefaultAsync(s => s.Id == record.StudentId && s.IsActive);
                    if (student == null)
                    {
                        errors.Add($"Estudiante ID {record.StudentId} no encontrado o inactivo");
                        continue;
                    }

                    // Verificar que no existe un registro de asistencia para la misma fecha y estudiante
                    var existingAttendance = await _context.Attendances
                        .FirstOrDefaultAsync(a => a.StudentId == record.StudentId && 
                                                a.Date == dto.Date && 
                                                a.IsActive);
                    if (existingAttendance != null)
                    {
                        errors.Add($"Ya existe un registro de asistencia para el estudiante ID {record.StudentId} en la fecha {dto.Date:yyyy-MM-dd}");
                        continue;
                    }

                    // Obtener la matrícula activa del estudiante
                    var enrollment = await _context.Enrollments
                        .FirstOrDefaultAsync(e => e.StudentId == record.StudentId && 
                                                e.IsActive && 
                                                e.Status == EnrollmentStatus.Active);
                    if (enrollment == null)
                    {
                        errors.Add($"No se encontró una matrícula activa para el estudiante ID {record.StudentId}");
                        continue;
                    }

                    var attendance = new Attendance
                    {
                        StudentId = record.StudentId,
                        EnrollmentId = enrollment.Id,
                        Date = dto.Date,
                        Status = record.Status,
                        Justification = record.Justification,
                        Notes = record.Notes,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.Attendances.Add(attendance);
                    results.Add(new { StudentId = record.StudentId, Status = "Created" });
                }
                catch (Exception ex)
                {
                    errors.Add($"Error procesando estudiante ID {record.StudentId}: {ex.Message}");
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

        // PUT: api/attendance/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> UpdateAttendance(int id, UpdateAttendanceDto dto)
        {
            var attendance = await _context.Attendances.FindAsync(id);
            if (attendance == null || !attendance.IsActive)
                return NotFound("Registro de asistencia no encontrado");

            // Actualizar campos
            if (dto.Status.HasValue) attendance.Status = dto.Status.Value;
            if (dto.Justification != null) attendance.Justification = dto.Justification;
            if (dto.Notes != null) attendance.Notes = dto.Notes;

            attendance.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/attendance/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeactivateAttendance(int id)
        {
            var attendance = await _context.Attendances.FindAsync(id);
            if (attendance == null || !attendance.IsActive)
                return NotFound("Registro de asistencia no encontrado");

            attendance.IsActive = false;
            attendance.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/attendance/student/{studentId}
        [HttpGet("student/{studentId}")]
        public async Task<ActionResult<IEnumerable<object>>> GetStudentAttendance(int studentId, 
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            var query = _context.Attendances
                .Include(a => a.Enrollment)
                    .ThenInclude(e => e.Course)
                .Where(a => a.StudentId == studentId && a.IsActive);

            if (startDate.HasValue)
                query = query.Where(a => a.Date >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(a => a.Date <= endDate.Value);

            var attendance = await query
                .OrderByDescending(a => a.Date)
                .Select(a => new
                {
                    a.Id,
                    a.Date,
                    a.Status,
                    a.Justification,
                    a.Notes,
                    Course = new
                    {
                        a.Enrollment.Course.Id,
                        a.Enrollment.Course.Name,
                        a.Enrollment.Course.Section
                    }
                })
                .ToListAsync();

            return Ok(attendance);
        }

        // GET: api/attendance/course/{courseId}
        [HttpGet("course/{courseId}")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<ActionResult<IEnumerable<object>>> GetCourseAttendance(int courseId, 
            [FromQuery] DateTime date)
        {
            var attendance = await _context.Attendances
                .Include(a => a.Student)
                    .ThenInclude(s => s.User)
                .Include(a => a.Enrollment)
                .Where(a => a.Enrollment.CourseId == courseId && 
                           a.Date == date && 
                           a.IsActive)
                .Select(a => new
                {
                    a.Id,
                    a.StudentId,
                    a.Status,
                    a.Justification,
                    a.Notes,
                    Student = new
                    {
                        a.Student.Id,
                        a.Student.User.FirstName,
                        a.Student.User.LastName,
                        a.Student.StudentId
                    }
                })
                .OrderBy(a => a.Student.LastName)
                .ThenBy(a => a.Student.FirstName)
                .ToListAsync();

            return Ok(attendance);
        }

        // GET: api/attendance/report
        [HttpGet("report")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<ActionResult<object>> GetAttendanceReport(
            [FromQuery] int? courseId = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            var query = _context.Attendances
                .Include(a => a.Student)
                    .ThenInclude(s => s.User)
                .Include(a => a.Enrollment)
                    .ThenInclude(e => e.Course)
                .Where(a => a.IsActive);

            if (courseId.HasValue)
                query = query.Where(a => a.Enrollment.CourseId == courseId);
            if (startDate.HasValue)
                query = query.Where(a => a.Date >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(a => a.Date <= endDate.Value);

            var attendanceData = await query.ToListAsync();

            var report = attendanceData
                .GroupBy(a => new { a.StudentId, a.Student.User.FirstName, a.Student.User.LastName })
                .Select(g => new
                {
                    StudentId = g.Key.StudentId,
                    StudentName = $"{g.Key.FirstName} {g.Key.LastName}",
                    StudentCode = g.Key.StudentId,
                    TotalDays = g.Count(),
                    Present = g.Count(a => a.Status == AttendanceStatus.Present),
                    Absent = g.Count(a => a.Status == AttendanceStatus.Absent),
                    Late = g.Count(a => a.Status == AttendanceStatus.Late),
                    Excused = g.Count(a => a.Status == AttendanceStatus.Excused),
                    Tardy = g.Count(a => a.Status == AttendanceStatus.Tardy),
                    AttendanceRate = Math.Round((double)g.Count(a => a.Status == AttendanceStatus.Present) / g.Count() * 100, 2)
                })
                .OrderBy(x => x.StudentName)
                .ToList();

            var summary = new
            {
                TotalStudents = report.Count,
                AverageAttendanceRate = report.Any() ? Math.Round(report.Average(x => x.AttendanceRate), 2) : 0,
                StudentsWithPerfectAttendance = report.Count(x => x.AttendanceRate == 100),
                StudentsBelow80Percent = report.Count(x => x.AttendanceRate < 80)
            };

            return Ok(new
            {
                Summary = summary,
                Details = report
            });
        }

        // GET: api/attendance/stats
        [HttpGet("stats")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<object>> GetAttendanceStats()
        {
            var totalRecords = await _context.Attendances.CountAsync(a => a.IsActive);
            var todayRecords = await _context.Attendances.CountAsync(a => a.Date == DateTime.Today && a.IsActive);
            
            var statusStats = await _context.Attendances
                .Where(a => a.IsActive)
                .GroupBy(a => a.Status)
                .Select(g => new
                {
                    Status = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();

            var monthlyStats = await _context.Attendances
                .Where(a => a.IsActive && a.Date >= DateTime.Today.AddMonths(-6))
                .GroupBy(a => new { a.Date.Year, a.Date.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    TotalRecords = g.Count(),
                    PresentCount = g.Count(a => a.Status == AttendanceStatus.Present),
                    AbsentCount = g.Count(a => a.Status == AttendanceStatus.Absent)
                })
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .ToListAsync();

            return Ok(new
            {
                TotalRecords = totalRecords,
                TodayRecords = todayRecords,
                StatusBreakdown = statusStats,
                MonthlyTrend = monthlyStats
            });
        }
    }

    public class CreateAttendanceDto
    {
        [Required]
        public int StudentId { get; set; }

        [Required]
        public int EnrollmentId { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public AttendanceStatus Status { get; set; }

        [StringLength(500)]
        public string? Justification { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }
    }

    public class CreateBulkAttendanceDto
    {
        [Required]
        public DateTime Date { get; set; }

        [Required]
        public List<BulkAttendanceRecord> Records { get; set; } = new();
    }

    public class BulkAttendanceRecord
    {
        [Required]
        public int StudentId { get; set; }

        [Required]
        public AttendanceStatus Status { get; set; }

        [StringLength(500)]
        public string? Justification { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }
    }

    public class UpdateAttendanceDto
    {
        public AttendanceStatus? Status { get; set; }
        public string? Justification { get; set; }
        public string? Notes { get; set; }
    }
} 