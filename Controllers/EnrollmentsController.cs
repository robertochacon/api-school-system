using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using api_school_system.Data;
using api_school_system.Models;
using System.ComponentModel.DataAnnotations;

namespace api_school_system.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EnrollmentsController : ControllerBase
{
    private readonly AppDbContext _context;

    public EnrollmentsController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Obtener todas las matrículas
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> GetEnrollments([FromQuery] int? studentId, [FromQuery] int? courseId, [FromQuery] int? academicPeriodId, [FromQuery] EnrollmentStatus? status)
    {
        var query = _context.Enrollments
            .Include(e => e.Student)
            .ThenInclude(s => s.User)
            .Include(e => e.Course)
            .ThenInclude(c => c.Grade)
            .Include(e => e.AcademicPeriod)
            .AsQueryable();

        if (studentId.HasValue)
            query = query.Where(e => e.StudentId == studentId.Value);

        if (courseId.HasValue)
            query = query.Where(e => e.CourseId == courseId.Value);

        if (academicPeriodId.HasValue)
            query = query.Where(e => e.AcademicPeriodId == academicPeriodId.Value);

        if (status.HasValue)
            query = query.Where(e => e.Status == status.Value);

        var enrollments = await query
            .Select(e => new {
                id = e.Id,
                student = new {
                    id = e.Student.Id,
                    firstName = e.Student.User.FirstName,
                    lastName = e.Student.User.LastName,
                    studentId = e.Student.StudentId
                },
                course = new {
                    id = e.Course.Id,
                    name = e.Course.Name,
                    grade = e.Course.Grade.Name
                },
                academicPeriod = new {
                    id = e.AcademicPeriod.Id,
                    name = e.AcademicPeriod.Name
                },
                enrollmentDate = e.EnrollmentDate,
                startDate = e.StartDate,
                endDate = e.EndDate,
                status = e.Status,
                notes = e.Notes,
                isActive = e.IsActive,
                createdAt = e.CreatedAt
            })
            .OrderByDescending(e => e.enrollmentDate)
            .ToListAsync();

        return Ok(enrollments);
    }

    /// <summary>
    /// Obtener matrícula por ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetEnrollment(int id)
    {
        var enrollment = await _context.Enrollments
            .Include(e => e.Student)
            .ThenInclude(s => s.User)
            .Include(e => e.Course)
            .ThenInclude(c => c.Grade)
            .Include(e => e.AcademicPeriod)
            .Include(e => e.StudentGrades)
            .ThenInclude(sg => sg.Evaluation)
            .Include(e => e.Attendances)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (enrollment == null)
            return NotFound(new { message = "Matrícula no encontrada" });

        return Ok(new {
            id = enrollment.Id,
            student = new {
                id = enrollment.Student.Id,
                firstName = enrollment.Student.User.FirstName,
                lastName = enrollment.Student.User.LastName,
                studentId = enrollment.Student.StudentId,
                email = enrollment.Student.User.Email
            },
            course = new {
                id = enrollment.Course.Id,
                name = enrollment.Course.Name,
                section = enrollment.Course.Section,
                grade = enrollment.Course.Grade.Name
            },
            academicPeriod = new {
                id = enrollment.AcademicPeriod.Id,
                name = enrollment.AcademicPeriod.Name,
                startDate = enrollment.AcademicPeriod.StartDate,
                endDate = enrollment.AcademicPeriod.EndDate
            },
            enrollmentDate = enrollment.EnrollmentDate,
            startDate = enrollment.StartDate,
            endDate = enrollment.EndDate,
            status = enrollment.Status,
            notes = enrollment.Notes,
            isActive = enrollment.IsActive,
            createdAt = enrollment.CreatedAt,
            updatedAt = enrollment.UpdatedAt,
            grades = enrollment.StudentGrades.Select(sg => new {
                id = sg.Id,
                evaluation = sg.Evaluation.Name,
                score = sg.Score,
                comments = sg.Comments,
                gradedAt = sg.GradedAt
            }),
            attendances = enrollment.Attendances.Select(a => new {
                id = a.Id,
                date = a.Date,
                status = a.Status,
                justification = a.Justification
            })
        });
    }

    /// <summary>
    /// Crear nueva matrícula
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateEnrollment([FromBody] CreateEnrollmentDto dto)
    {
        var student = await _context.Students.FindAsync(dto.StudentId);
        if (student == null)
            return BadRequest(new { message = "Estudiante no encontrado" });

        var course = await _context.Courses.FindAsync(dto.CourseId);
        if (course == null)
            return BadRequest(new { message = "Curso no encontrado" });

        var academicPeriod = await _context.AcademicPeriods.FindAsync(dto.AcademicPeriodId);
        if (academicPeriod == null)
            return BadRequest(new { message = "Período académico no encontrado" });

        // Verificar si ya existe una matrícula activa para este estudiante en este curso y período
        var existingEnrollment = await _context.Enrollments
            .FirstOrDefaultAsync(e => e.StudentId == dto.StudentId && 
                                    e.CourseId == dto.CourseId && 
                                    e.AcademicPeriodId == dto.AcademicPeriodId &&
                                    e.IsActive);

        if (existingEnrollment != null)
            return BadRequest(new { message = "El estudiante ya está matriculado en este curso para este período" });

        var enrollment = new Enrollment
        {
            StudentId = dto.StudentId,
            CourseId = dto.CourseId,
            GradeId = course.GradeId,
            AcademicPeriodId = dto.AcademicPeriodId,
            EnrollmentDate = dto.EnrollmentDate ?? DateTime.UtcNow,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            Status = dto.Status,
            Notes = dto.Notes,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Enrollments.Add(enrollment);

        // Actualizar el contador de estudiantes en el curso
        course.CurrentEnrollment = (course.CurrentEnrollment ?? 0) + 1;

        await _context.SaveChangesAsync();

        return Ok(new { message = "Matrícula creada exitosamente", enrollmentId = enrollment.Id });
    }

    /// <summary>
    /// Actualizar matrícula
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateEnrollment(int id, [FromBody] UpdateEnrollmentDto dto)
    {
        var enrollment = await _context.Enrollments.FindAsync(id);
        if (enrollment == null)
            return NotFound(new { message = "Matrícula no encontrada" });

        enrollment.StartDate = dto.StartDate;
        enrollment.EndDate = dto.EndDate;
        enrollment.Status = dto.Status;
        enrollment.Notes = dto.Notes;
        enrollment.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { message = "Matrícula actualizada exitosamente" });
    }

    /// <summary>
    /// Cancelar matrícula
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CancelEnrollment(int id)
    {
        var enrollment = await _context.Enrollments
            .Include(e => e.Course)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (enrollment == null)
            return NotFound(new { message = "Matrícula no encontrada" });

        enrollment.Status = EnrollmentStatus.Cancelled;
        enrollment.IsActive = false;
        enrollment.UpdatedAt = DateTime.UtcNow;

        // Actualizar el contador de estudiantes en el curso
        if (enrollment.Course.CurrentEnrollment.HasValue && enrollment.Course.CurrentEnrollment.Value > 0)
        {
            enrollment.Course.CurrentEnrollment = enrollment.Course.CurrentEnrollment.Value - 1;
        }

        await _context.SaveChangesAsync();

        return Ok(new { message = "Matrícula cancelada exitosamente" });
    }

    /// <summary>
    /// Obtener matrículas de un estudiante
    /// </summary>
    [HttpGet("student/{studentId}")]
    public async Task<IActionResult> GetStudentEnrollments(int studentId)
    {
        var enrollments = await _context.Enrollments
            .Include(e => e.Course)
            .ThenInclude(c => c.Grade)
            .Include(e => e.AcademicPeriod)
            .Include(e => e.StudentGrades)
            .Where(e => e.StudentId == studentId)
            .Select(e => new {
                id = e.Id,
                course = new {
                    id = e.Course.Id,
                    name = e.Course.Name,
                    grade = e.Course.Grade.Name
                },
                academicPeriod = new {
                    id = e.AcademicPeriod.Id,
                    name = e.AcademicPeriod.Name
                },
                enrollmentDate = e.EnrollmentDate,
                status = e.Status,
                gradesCount = e.StudentGrades.Count,
                averageScore = e.StudentGrades.Any() ? e.StudentGrades.Average(sg => sg.Score) : 0
            })
            .OrderByDescending(e => e.enrollmentDate)
            .ToListAsync();

        return Ok(enrollments);
    }

    /// <summary>
    /// Obtener matrículas de un curso
    /// </summary>
    [HttpGet("course/{courseId}")]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> GetCourseEnrollments(int courseId)
    {
        var enrollments = await _context.Enrollments
            .Include(e => e.Student)
            .ThenInclude(s => s.User)
            .Include(e => e.AcademicPeriod)
            .Where(e => e.CourseId == courseId)
            .Select(e => new {
                id = e.Id,
                student = new {
                    id = e.Student.Id,
                    firstName = e.Student.User.FirstName,
                    lastName = e.Student.User.LastName,
                    studentId = e.Student.StudentId
                },
                academicPeriod = new {
                    id = e.AcademicPeriod.Id,
                    name = e.AcademicPeriod.Name
                },
                enrollmentDate = e.EnrollmentDate,
                status = e.Status
            })
            .OrderBy(e => e.student.lastName)
            .ThenBy(e => e.student.firstName)
            .ToListAsync();

        return Ok(enrollments);
    }

    /// <summary>
    /// Obtener estadísticas de matrículas
    /// </summary>
    [HttpGet("stats")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetEnrollmentStats([FromQuery] int? academicPeriodId)
    {
        var query = _context.Enrollments.AsQueryable();

        if (academicPeriodId.HasValue)
            query = query.Where(e => e.AcademicPeriodId == academicPeriodId.Value);

        var totalEnrollments = await query.CountAsync();
        var activeEnrollments = await query.CountAsync(e => e.Status == EnrollmentStatus.Active);
        var pendingEnrollments = await query.CountAsync(e => e.Status == EnrollmentStatus.Pending);
        var completedEnrollments = await query.CountAsync(e => e.Status == EnrollmentStatus.Completed);
        var cancelledEnrollments = await query.CountAsync(e => e.Status == EnrollmentStatus.Cancelled);

        var statsByStatus = await query
            .GroupBy(e => e.Status)
            .Select(g => new {
                status = g.Key.ToString(),
                count = g.Count()
            })
            .ToListAsync();

        return Ok(new {
            totalEnrollments,
            activeEnrollments,
            pendingEnrollments,
            completedEnrollments,
            cancelledEnrollments,
            byStatus = statsByStatus
        });
    }
}

public class CreateEnrollmentDto
{
    [Required(ErrorMessage = "El ID del estudiante es requerido")]
    public int StudentId { get; set; }

    [Required(ErrorMessage = "El ID del curso es requerido")]
    public int CourseId { get; set; }

    [Required(ErrorMessage = "El ID del período académico es requerido")]
    public int AcademicPeriodId { get; set; }

    public DateTime? EnrollmentDate { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public EnrollmentStatus Status { get; set; } = EnrollmentStatus.Pending;

    [StringLength(255, ErrorMessage = "Las notas no pueden exceder 255 caracteres")]
    public string? Notes { get; set; }
}

public class UpdateEnrollmentDto
{
    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public EnrollmentStatus Status { get; set; }

    [StringLength(255, ErrorMessage = "Las notas no pueden exceder 255 caracteres")]
    public string? Notes { get; set; }
} 