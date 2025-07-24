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
public class SubjectsController : ControllerBase
{
    private readonly AppDbContext _context;

    public SubjectsController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Obtener todas las asignaturas
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetSubjects([FromQuery] string? department, [FromQuery] bool? isActive)
    {
        var query = _context.Subjects
            .Include(s => s.SubjectTeachers)
            .ThenInclude(st => st.Teacher)
            .ThenInclude(t => t.User)
            .Include(s => s.Schedules)
            .AsQueryable();

        if (!string.IsNullOrEmpty(department))
            query = query.Where(s => s.Department == department);

        if (isActive.HasValue)
            query = query.Where(s => s.IsActive == isActive.Value);

        var subjects = await query
            .Select(s => new {
                id = s.Id,
                name = s.Name,
                code = s.Code,
                description = s.Description,
                credits = s.Credits,
                hoursPerWeek = s.HoursPerWeek,
                department = s.Department,
                isActive = s.IsActive,
                teachersCount = s.SubjectTeachers.Count,
                schedulesCount = s.Schedules.Count,
                createdAt = s.CreatedAt
            })
            .ToListAsync();

        return Ok(subjects);
    }

    /// <summary>
    /// Obtener asignatura por ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetSubject(int id)
    {
        var subject = await _context.Subjects
            .Include(s => s.SubjectTeachers)
            .ThenInclude(st => st.Teacher)
            .ThenInclude(t => t.User)
            .Include(s => s.Schedules)
            .ThenInclude(sch => sch.Course)
            .ThenInclude(c => c.Grade)
            .Include(s => s.Evaluations)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (subject == null)
            return NotFound(new { message = "Asignatura no encontrada" });

        return Ok(new {
            id = subject.Id,
            name = subject.Name,
            code = subject.Code,
            description = subject.Description,
            credits = subject.Credits,
            hoursPerWeek = subject.HoursPerWeek,
            department = subject.Department,
            isActive = subject.IsActive,
            createdAt = subject.CreatedAt,
            updatedAt = subject.UpdatedAt,
            teachers = subject.SubjectTeachers.Select(st => new {
                id = st.Teacher.Id,
                firstName = st.Teacher.User.FirstName,
                lastName = st.Teacher.User.LastName,
                specialization = st.Teacher.Specialization
            }),
            schedules = subject.Schedules.Select(s => new {
                id = s.Id,
                course = new {
                    id = s.Course.Id,
                    name = s.Course.Name,
                    grade = s.Course.Grade.Name
                },
                dayOfWeek = s.DayOfWeek,
                startTime = s.StartTime,
                endTime = s.EndTime,
                room = s.Room
            }),
            evaluations = subject.Evaluations.Select(e => new {
                id = e.Id,
                name = e.Name,
                type = e.Type,
                weight = e.Weight,
                dueDate = e.DueDate
            })
        });
    }

    /// <summary>
    /// Crear nueva asignatura
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateSubject([FromBody] CreateSubjectDto dto)
    {
        if (await _context.Subjects.AnyAsync(s => s.Code == dto.Code))
            return BadRequest(new { message = "Ya existe una asignatura con ese código" });

        var subject = new Subject
        {
            Name = dto.Name,
            Code = dto.Code,
            Description = dto.Description,
            Credits = dto.Credits,
            HoursPerWeek = dto.HoursPerWeek,
            Department = dto.Department,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Subjects.Add(subject);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Asignatura creada exitosamente", subjectId = subject.Id });
    }

    /// <summary>
    /// Actualizar asignatura
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateSubject(int id, [FromBody] UpdateSubjectDto dto)
    {
        var subject = await _context.Subjects.FindAsync(id);
        if (subject == null)
            return NotFound(new { message = "Asignatura no encontrada" });

        subject.Name = dto.Name;
        subject.Description = dto.Description;
        subject.Credits = dto.Credits;
        subject.HoursPerWeek = dto.HoursPerWeek;
        subject.Department = dto.Department;
        subject.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { message = "Asignatura actualizada exitosamente" });
    }

    /// <summary>
    /// Desactivar asignatura
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeactivateSubject(int id)
    {
        var subject = await _context.Subjects.FindAsync(id);
        if (subject == null)
            return NotFound(new { message = "Asignatura no encontrada" });

        subject.IsActive = false;
        subject.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { message = "Asignatura desactivada exitosamente" });
    }

    /// <summary>
    /// Asignar docente a la asignatura
    /// </summary>
    [HttpPost("{id}/teachers")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AssignTeacher(int id, [FromBody] AssignSubjectTeacherDto dto)
    {
        var subject = await _context.Subjects.FindAsync(id);
        if (subject == null)
            return NotFound(new { message = "Asignatura no encontrada" });

        var teacher = await _context.Teachers.FindAsync(dto.TeacherId);
        if (teacher == null)
            return NotFound(new { message = "Docente no encontrado" });

        var existingAssignment = await _context.SubjectTeachers
            .FirstOrDefaultAsync(st => st.SubjectId == id && st.TeacherId == dto.TeacherId);

        if (existingAssignment != null)
            return BadRequest(new { message = "El docente ya está asignado a esta asignatura" });

        var subjectTeacher = new SubjectTeacher
        {
            SubjectId = id,
            TeacherId = dto.TeacherId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.SubjectTeachers.Add(subjectTeacher);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Docente asignado exitosamente" });
    }

    /// <summary>
    /// Remover docente de la asignatura
    /// </summary>
    [HttpDelete("{id}/teachers/{teacherId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> RemoveTeacher(int id, int teacherId)
    {
        var subjectTeacher = await _context.SubjectTeachers
            .FirstOrDefaultAsync(st => st.SubjectId == id && st.TeacherId == teacherId);

        if (subjectTeacher == null)
            return NotFound(new { message = "Asignación no encontrada" });

        subjectTeacher.IsActive = false;
        subjectTeacher.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { message = "Docente removido exitosamente" });
    }

    /// <summary>
    /// Obtener departamentos disponibles
    /// </summary>
    [HttpGet("departments")]
    public async Task<IActionResult> GetDepartments()
    {
        var departments = await _context.Subjects
            .Where(s => s.IsActive && !string.IsNullOrEmpty(s.Department))
            .Select(s => s.Department)
            .Distinct()
            .OrderBy(d => d)
            .ToListAsync();

        return Ok(departments);
    }

    /// <summary>
    /// Obtener estadísticas de la asignatura
    /// </summary>
    [HttpGet("{id}/stats")]
    public async Task<IActionResult> GetSubjectStats(int id)
    {
        var subject = await _context.Subjects
            .Include(s => s.SubjectTeachers)
            .Include(s => s.Schedules)
            .Include(s => s.Evaluations)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (subject == null)
            return NotFound(new { message = "Asignatura no encontrada" });

        var totalTeachers = subject.SubjectTeachers.Count;
        var totalSchedules = subject.Schedules.Count;
        var totalEvaluations = subject.Evaluations.Count;

        return Ok(new {
            subjectId = subject.Id,
            subjectName = subject.Name,
            totalTeachers,
            totalSchedules,
            totalEvaluations,
            credits = subject.Credits,
            hoursPerWeek = subject.HoursPerWeek,
            department = subject.Department
        });
    }
}

public class CreateSubjectDto
{
    [Required(ErrorMessage = "El nombre de la asignatura es requerido")]
    [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "El código de la asignatura es requerido")]
    [StringLength(10, ErrorMessage = "El código no puede exceder 10 caracteres")]
    public string Code { get; set; } = string.Empty;

    [StringLength(255, ErrorMessage = "La descripción no puede exceder 255 caracteres")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Los créditos son requeridos")]
    [Range(1, 20, ErrorMessage = "Los créditos deben estar entre 1 y 20")]
    public int Credits { get; set; }

    [Required(ErrorMessage = "Las horas por semana son requeridas")]
    [Range(1, 40, ErrorMessage = "Las horas por semana deben estar entre 1 y 40")]
    public int HoursPerWeek { get; set; }

    [StringLength(100, ErrorMessage = "El departamento no puede exceder 100 caracteres")]
    public string? Department { get; set; }
}

public class UpdateSubjectDto
{
    [Required(ErrorMessage = "El nombre de la asignatura es requerido")]
    [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
    public string Name { get; set; } = string.Empty;

    [StringLength(255, ErrorMessage = "La descripción no puede exceder 255 caracteres")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Los créditos son requeridos")]
    [Range(1, 20, ErrorMessage = "Los créditos deben estar entre 1 y 20")]
    public int Credits { get; set; }

    [Required(ErrorMessage = "Las horas por semana son requeridas")]
    [Range(1, 40, ErrorMessage = "Las horas por semana deben estar entre 1 y 40")]
    public int HoursPerWeek { get; set; }

    [StringLength(100, ErrorMessage = "El departamento no puede exceder 100 caracteres")]
    public string? Department { get; set; }
}

public class AssignSubjectTeacherDto
{
    [Required(ErrorMessage = "El ID del docente es requerido")]
    public int TeacherId { get; set; }
} 