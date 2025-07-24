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
public class CoursesController : ControllerBase
{
    private readonly AppDbContext _context;

    public CoursesController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Obtener todos los cursos
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetCourses([FromQuery] int? gradeId, [FromQuery] bool? isActive)
    {
        var query = _context.Courses
            .Include(c => c.Grade)
            .Include(c => c.CourseTeachers)
            .ThenInclude(ct => ct.Teacher)
            .ThenInclude(t => t.User)
            .Include(c => c.Enrollments)
            .AsQueryable();

        if (gradeId.HasValue)
            query = query.Where(c => c.GradeId == gradeId.Value);

        if (isActive.HasValue)
            query = query.Where(c => c.IsActive == isActive.Value);

        var courses = await query
            .Select(c => new {
                id = c.Id,
                name = c.Name,
                section = c.Section,
                description = c.Description,
                grade = new {
                    id = c.Grade.Id,
                    name = c.Grade.Name,
                    level = c.Grade.Level
                },
                capacity = c.Capacity,
                currentEnrollment = c.CurrentEnrollment,
                isActive = c.IsActive,
                teachersCount = c.CourseTeachers.Count,
                studentsCount = c.Enrollments.Count,
                createdAt = c.CreatedAt
            })
            .ToListAsync();

        return Ok(courses);
    }

    /// <summary>
    /// Obtener curso por ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetCourse(int id)
    {
        var course = await _context.Courses
            .Include(c => c.Grade)
            .Include(c => c.CourseTeachers)
            .ThenInclude(ct => ct.Teacher)
            .ThenInclude(t => t.User)
            .Include(c => c.Enrollments)
            .ThenInclude(e => e.Student)
            .ThenInclude(s => s.User)
            .Include(c => c.Schedules)
            .ThenInclude(s => s.Subject)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (course == null)
            return NotFound(new { message = "Curso no encontrado" });

        return Ok(new {
            id = course.Id,
            name = course.Name,
            section = course.Section,
            description = course.Description,
            grade = new {
                id = course.Grade.Id,
                name = course.Grade.Name,
                level = course.Grade.Level
            },
            capacity = course.Capacity,
            currentEnrollment = course.CurrentEnrollment,
            isActive = course.IsActive,
            createdAt = course.CreatedAt,
            updatedAt = course.UpdatedAt,
            teachers = course.CourseTeachers.Select(ct => new {
                id = ct.Teacher.Id,
                firstName = ct.Teacher.User.FirstName,
                lastName = ct.Teacher.User.LastName,
                role = ct.Role
            }),
            students = course.Enrollments.Select(e => new {
                id = e.Student.Id,
                firstName = e.Student.User.FirstName,
                lastName = e.Student.User.LastName,
                studentId = e.Student.StudentId,
                status = e.Status,
                enrollmentDate = e.EnrollmentDate
            }),
            schedules = course.Schedules.Select(s => new {
                id = s.Id,
                subject = s.Subject.Name,
                dayOfWeek = s.DayOfWeek,
                startTime = s.StartTime,
                endTime = s.EndTime,
                room = s.Room
            })
        });
    }

    /// <summary>
    /// Crear nuevo curso
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateCourse([FromBody] CreateCourseDto dto)
    {
        var grade = await _context.Grades.FindAsync(dto.GradeId);
        if (grade == null)
            return BadRequest(new { message = "Grado no encontrado" });

        var course = new Course
        {
            GradeId = dto.GradeId,
            Name = dto.Name,
            Section = dto.Section,
            Description = dto.Description,
            Capacity = dto.Capacity,
            CurrentEnrollment = 0,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Courses.Add(course);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Curso creado exitosamente", courseId = course.Id });
    }

    /// <summary>
    /// Actualizar curso
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateCourse(int id, [FromBody] UpdateCourseDto dto)
    {
        var course = await _context.Courses.FindAsync(id);
        if (course == null)
            return NotFound(new { message = "Curso no encontrado" });

        course.Name = dto.Name;
        course.Section = dto.Section;
        course.Description = dto.Description;
        course.Capacity = dto.Capacity;
        course.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { message = "Curso actualizado exitosamente" });
    }

    /// <summary>
    /// Desactivar curso
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeactivateCourse(int id)
    {
        var course = await _context.Courses.FindAsync(id);
        if (course == null)
            return NotFound(new { message = "Curso no encontrado" });

        course.IsActive = false;
        course.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { message = "Curso desactivado exitosamente" });
    }

    /// <summary>
    /// Asignar docente al curso
    /// </summary>
    [HttpPost("{id}/teachers")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AssignTeacher(int id, [FromBody] AssignTeacherDto dto)
    {
        var course = await _context.Courses.FindAsync(id);
        if (course == null)
            return NotFound(new { message = "Curso no encontrado" });

        var teacher = await _context.Teachers.FindAsync(dto.TeacherId);
        if (teacher == null)
            return NotFound(new { message = "Docente no encontrado" });

        var existingAssignment = await _context.CourseTeachers
            .FirstOrDefaultAsync(ct => ct.CourseId == id && ct.TeacherId == dto.TeacherId);

        if (existingAssignment != null)
            return BadRequest(new { message = "El docente ya está asignado a este curso" });

        var courseTeacher = new CourseTeacher
        {
            CourseId = id,
            TeacherId = dto.TeacherId,
            Role = dto.Role,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.CourseTeachers.Add(courseTeacher);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Docente asignado exitosamente" });
    }

    /// <summary>
    /// Remover docente del curso
    /// </summary>
    [HttpDelete("{id}/teachers/{teacherId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> RemoveTeacher(int id, int teacherId)
    {
        var courseTeacher = await _context.CourseTeachers
            .FirstOrDefaultAsync(ct => ct.CourseId == id && ct.TeacherId == teacherId);

        if (courseTeacher == null)
            return NotFound(new { message = "Asignación no encontrada" });

        courseTeacher.IsActive = false;
        courseTeacher.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { message = "Docente removido exitosamente" });
    }

    /// <summary>
    /// Obtener estadísticas del curso
    /// </summary>
    [HttpGet("{id}/stats")]
    public async Task<IActionResult> GetCourseStats(int id)
    {
        var course = await _context.Courses
            .Include(c => c.Enrollments)
            .Include(c => c.CourseTeachers)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (course == null)
            return NotFound(new { message = "Curso no encontrado" });

        var totalStudents = course.Enrollments.Count;
        var activeStudents = course.Enrollments.Count(e => e.Status == EnrollmentStatus.Active);
        var totalTeachers = course.CourseTeachers.Count;

        return Ok(new {
            courseId = course.Id,
            courseName = course.Name,
            totalStudents,
            activeStudents,
            inactiveStudents = totalStudents - activeStudents,
            totalTeachers,
            capacity = course.Capacity,
            enrollmentRate = course.Capacity.HasValue ? (double)totalStudents / course.Capacity.Value * 100 : 0
        });
    }
}

public class CreateCourseDto
{
    [Required(ErrorMessage = "El ID del grado es requerido")]
    public int GradeId { get; set; }

    [Required(ErrorMessage = "El nombre del curso es requerido")]
    [StringLength(50, ErrorMessage = "El nombre no puede exceder 50 caracteres")]
    public string Name { get; set; } = string.Empty;

    [StringLength(10, ErrorMessage = "La sección no puede exceder 10 caracteres")]
    public string? Section { get; set; }

    [StringLength(255, ErrorMessage = "La descripción no puede exceder 255 caracteres")]
    public string? Description { get; set; }

    [Range(1, 1000, ErrorMessage = "La capacidad debe estar entre 1 y 1000")]
    public int? Capacity { get; set; }
}

public class UpdateCourseDto
{
    [Required(ErrorMessage = "El nombre del curso es requerido")]
    [StringLength(50, ErrorMessage = "El nombre no puede exceder 50 caracteres")]
    public string Name { get; set; } = string.Empty;

    [StringLength(10, ErrorMessage = "La sección no puede exceder 10 caracteres")]
    public string? Section { get; set; }

    [StringLength(255, ErrorMessage = "La descripción no puede exceder 255 caracteres")]
    public string? Description { get; set; }

    [Range(1, 1000, ErrorMessage = "La capacidad debe estar entre 1 y 1000")]
    public int? Capacity { get; set; }
}

public class AssignTeacherDto
{
    [Required(ErrorMessage = "El ID del docente es requerido")]
    public int TeacherId { get; set; }

    [StringLength(100, ErrorMessage = "El rol no puede exceder 100 caracteres")]
    public string? Role { get; set; }
} 