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
public class GradesController : ControllerBase
{
    private readonly AppDbContext _context;

    public GradesController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Obtener todos los grados académicos
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetGrades([FromQuery] bool? isActive)
    {
        var query = _context.Grades
            .Include(g => g.Courses)
            .Include(g => g.Enrollments)
            .AsQueryable();

        if (isActive.HasValue)
            query = query.Where(g => g.IsActive == isActive.Value);

        var grades = await query
            .Select(g => new {
                id = g.Id,
                name = g.Name,
                description = g.Description,
                level = g.Level,
                code = g.Code,
                capacity = g.Capacity,
                isActive = g.IsActive,
                coursesCount = g.Courses.Count,
                enrollmentsCount = g.Enrollments.Count,
                createdAt = g.CreatedAt
            })
            .OrderBy(g => g.level)
            .ToListAsync();

        return Ok(grades);
    }

    /// <summary>
    /// Obtener grado por ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetGrade(int id)
    {
        var grade = await _context.Grades
            .Include(g => g.Courses)
            .ThenInclude(c => c.CourseTeachers)
            .ThenInclude(ct => ct.Teacher)
            .ThenInclude(t => t.User)
            .Include(g => g.Enrollments)
            .ThenInclude(e => e.Student)
            .ThenInclude(s => s.User)
            .FirstOrDefaultAsync(g => g.Id == id);

        if (grade == null)
            return NotFound(new { message = "Grado no encontrado" });

        return Ok(new {
            id = grade.Id,
            name = grade.Name,
            description = grade.Description,
            level = grade.Level,
            code = grade.Code,
            capacity = grade.Capacity,
            isActive = grade.IsActive,
            createdAt = grade.CreatedAt,
            updatedAt = grade.UpdatedAt,
            courses = grade.Courses.Select(c => new {
                id = c.Id,
                name = c.Name,
                section = c.Section,
                description = c.Description,
                capacity = c.Capacity,
                currentEnrollment = c.CurrentEnrollment,
                teachers = c.CourseTeachers.Select(ct => new {
                    id = ct.Teacher.Id,
                    firstName = ct.Teacher.User.FirstName,
                    lastName = ct.Teacher.User.LastName,
                    role = ct.Role
                })
            }),
            enrollments = grade.Enrollments.Select(e => new {
                id = e.Id,
                student = new {
                    id = e.Student.Id,
                    firstName = e.Student.User.FirstName,
                    lastName = e.Student.User.LastName,
                    studentId = e.Student.StudentId
                },
                status = e.Status,
                enrollmentDate = e.EnrollmentDate
            })
        });
    }

    /// <summary>
    /// Crear nuevo grado
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateGrade([FromBody] CreateGradeDto dto)
    {
        if (await _context.Grades.AnyAsync(g => g.Name == dto.Name))
            return BadRequest(new { message = "Ya existe un grado con ese nombre" });

        var grade = new Grade
        {
            Name = dto.Name,
            Description = dto.Description,
            Level = dto.Level,
            Code = dto.Code,
            Capacity = dto.Capacity,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Grades.Add(grade);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Grado creado exitosamente", gradeId = grade.Id });
    }

    /// <summary>
    /// Actualizar grado
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateGrade(int id, [FromBody] UpdateGradeDto dto)
    {
        var grade = await _context.Grades.FindAsync(id);
        if (grade == null)
            return NotFound(new { message = "Grado no encontrado" });

        grade.Name = dto.Name;
        grade.Description = dto.Description;
        grade.Level = dto.Level;
        grade.Code = dto.Code;
        grade.Capacity = dto.Capacity;
        grade.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { message = "Grado actualizado exitosamente" });
    }

    /// <summary>
    /// Desactivar grado
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeactivateGrade(int id)
    {
        var grade = await _context.Grades.FindAsync(id);
        if (grade == null)
            return NotFound(new { message = "Grado no encontrado" });

        grade.IsActive = false;
        grade.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { message = "Grado desactivado exitosamente" });
    }

    /// <summary>
    /// Obtener estadísticas del grado
    /// </summary>
    [HttpGet("{id}/stats")]
    public async Task<IActionResult> GetGradeStats(int id)
    {
        var grade = await _context.Grades
            .Include(g => g.Courses)
            .Include(g => g.Enrollments)
            .ThenInclude(e => e.Student)
            .FirstOrDefaultAsync(g => g.Id == id);

        if (grade == null)
            return NotFound(new { message = "Grado no encontrado" });

        var totalStudents = grade.Enrollments.Count;
        var activeStudents = grade.Enrollments.Count(e => e.Status == EnrollmentStatus.Active);
        var totalCourses = grade.Courses.Count;

        return Ok(new {
            gradeId = grade.Id,
            gradeName = grade.Name,
            totalStudents,
            activeStudents,
            inactiveStudents = totalStudents - activeStudents,
            totalCourses,
            capacity = grade.Capacity,
            enrollmentRate = grade.Capacity.HasValue ? (double)totalStudents / grade.Capacity.Value * 100 : 0
        });
    }
}

public class CreateGradeDto
{
    [Required(ErrorMessage = "El nombre del grado es requerido")]
    [StringLength(50, ErrorMessage = "El nombre no puede exceder 50 caracteres")]
    public string Name { get; set; } = string.Empty;

    [StringLength(255, ErrorMessage = "La descripción no puede exceder 255 caracteres")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "El nivel es requerido")]
    [Range(1, 12, ErrorMessage = "El nivel debe estar entre 1 y 12")]
    public int Level { get; set; }

    [StringLength(10, ErrorMessage = "El código no puede exceder 10 caracteres")]
    public string? Code { get; set; }

    [Range(1, 1000, ErrorMessage = "La capacidad debe estar entre 1 y 1000")]
    public int? Capacity { get; set; }
}

public class UpdateGradeDto
{
    [Required(ErrorMessage = "El nombre del grado es requerido")]
    [StringLength(50, ErrorMessage = "El nombre no puede exceder 50 caracteres")]
    public string Name { get; set; } = string.Empty;

    [StringLength(255, ErrorMessage = "La descripción no puede exceder 255 caracteres")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "El nivel es requerido")]
    [Range(1, 12, ErrorMessage = "El nivel debe estar entre 1 y 12")]
    public int Level { get; set; }

    [StringLength(10, ErrorMessage = "El código no puede exceder 10 caracteres")]
    public string? Code { get; set; }

    [Range(1, 1000, ErrorMessage = "La capacidad debe estar entre 1 y 1000")]
    public int? Capacity { get; set; }
} 