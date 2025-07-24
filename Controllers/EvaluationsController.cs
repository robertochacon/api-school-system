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
public class EvaluationsController : ControllerBase
{
    private readonly AppDbContext _context;

    public EvaluationsController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Obtener todas las evaluaciones
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetEvaluations([FromQuery] int? subjectId, [FromQuery] int? academicPeriodId, [FromQuery] EvaluationType? type, [FromQuery] bool? isActive)
    {
        var query = _context.Evaluations
            .Include(e => e.Subject)
            .Include(e => e.AcademicPeriod)
            .Include(e => e.StudentGrades)
            .AsQueryable();

        if (subjectId.HasValue)
            query = query.Where(e => e.SubjectId == subjectId.Value);

        if (academicPeriodId.HasValue)
            query = query.Where(e => e.AcademicPeriodId == academicPeriodId.Value);

        if (type.HasValue)
            query = query.Where(e => e.Type == type.Value);

        if (isActive.HasValue)
            query = query.Where(e => e.IsActive == isActive.Value);

        var evaluations = await query
            .Select(e => new {
                id = e.Id,
                name = e.Name,
                description = e.Description,
                type = e.Type,
                weight = e.Weight,
                maxScore = e.MaxScore,
                dueDate = e.DueDate,
                evaluationDate = e.EvaluationDate,
                subject = new {
                    id = e.Subject.Id,
                    name = e.Subject.Name,
                    code = e.Subject.Code
                },
                academicPeriod = new {
                    id = e.AcademicPeriod.Id,
                    name = e.AcademicPeriod.Name
                },
                isActive = e.IsActive,
                gradesCount = e.StudentGrades.Count,
                averageScore = e.StudentGrades.Any() ? e.StudentGrades.Average(sg => sg.Score) : 0,
                createdAt = e.CreatedAt
            })
            .OrderByDescending(e => e.createdAt)
            .ToListAsync();

        return Ok(evaluations);
    }

    /// <summary>
    /// Obtener evaluación por ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetEvaluation(int id)
    {
        var evaluation = await _context.Evaluations
            .Include(e => e.Subject)
            .Include(e => e.AcademicPeriod)
            .Include(e => e.StudentGrades)
            .ThenInclude(sg => sg.Student)
            .ThenInclude(s => s.User)
            .Include(e => e.StudentGrades)
            .ThenInclude(sg => sg.Teacher)
            .ThenInclude(t => t.User)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (evaluation == null)
            return NotFound(new { message = "Evaluación no encontrada" });

        return Ok(new {
            id = evaluation.Id,
            name = evaluation.Name,
            description = evaluation.Description,
            type = evaluation.Type,
            weight = evaluation.Weight,
            maxScore = evaluation.MaxScore,
            dueDate = evaluation.DueDate,
            evaluationDate = evaluation.EvaluationDate,
            isActive = evaluation.IsActive,
            createdAt = evaluation.CreatedAt,
            updatedAt = evaluation.UpdatedAt,
            subject = new {
                id = evaluation.Subject.Id,
                name = evaluation.Subject.Name,
                code = evaluation.Subject.Code
            },
            academicPeriod = new {
                id = evaluation.AcademicPeriod.Id,
                name = evaluation.AcademicPeriod.Name
            },
            grades = evaluation.StudentGrades.Select(sg => new {
                id = sg.Id,
                student = new {
                    id = sg.Student.Id,
                    firstName = sg.Student.User.FirstName,
                    lastName = sg.Student.User.LastName,
                    studentId = sg.Student.StudentId
                },
                score = sg.Score,
                comments = sg.Comments,
                teacher = new {
                    id = sg.Teacher.Id,
                    firstName = sg.Teacher.User.FirstName,
                    lastName = sg.Teacher.User.LastName
                },
                gradedAt = sg.GradedAt
            }),
            statistics = new {
                totalGrades = evaluation.StudentGrades.Count,
                averageScore = evaluation.StudentGrades.Any() ? evaluation.StudentGrades.Average(sg => sg.Score) : 0,
                highestScore = evaluation.StudentGrades.Any() ? evaluation.StudentGrades.Max(sg => sg.Score) : 0,
                lowestScore = evaluation.StudentGrades.Any() ? evaluation.StudentGrades.Min(sg => sg.Score) : 0
            }
        });
    }

    /// <summary>
    /// Crear nueva evaluación
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> CreateEvaluation([FromBody] CreateEvaluationDto dto)
    {
        var subject = await _context.Subjects.FindAsync(dto.SubjectId);
        if (subject == null)
            return BadRequest(new { message = "Asignatura no encontrada" });

        var academicPeriod = await _context.AcademicPeriods.FindAsync(dto.AcademicPeriodId);
        if (academicPeriod == null)
            return BadRequest(new { message = "Período académico no encontrado" });

        var evaluation = new Evaluation
        {
            SubjectId = dto.SubjectId,
            AcademicPeriodId = dto.AcademicPeriodId,
            Name = dto.Name,
            Description = dto.Description,
            Type = dto.Type,
            Weight = dto.Weight,
            DueDate = dto.DueDate,
            EvaluationDate = dto.EvaluationDate,
            MaxScore = dto.MaxScore,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Evaluations.Add(evaluation);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Evaluación creada exitosamente", evaluationId = evaluation.Id });
    }

    /// <summary>
    /// Actualizar evaluación
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> UpdateEvaluation(int id, [FromBody] UpdateEvaluationDto dto)
    {
        var evaluation = await _context.Evaluations.FindAsync(id);
        if (evaluation == null)
            return NotFound(new { message = "Evaluación no encontrada" });

        evaluation.Name = dto.Name;
        evaluation.Description = dto.Description;
        evaluation.Type = dto.Type;
        evaluation.Weight = dto.Weight;
        evaluation.DueDate = dto.DueDate;
        evaluation.EvaluationDate = dto.EvaluationDate;
        evaluation.MaxScore = dto.MaxScore;
        evaluation.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { message = "Evaluación actualizada exitosamente" });
    }

    /// <summary>
    /// Desactivar evaluación
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> DeactivateEvaluation(int id)
    {
        var evaluation = await _context.Evaluations.FindAsync(id);
        if (evaluation == null)
            return NotFound(new { message = "Evaluación no encontrada" });

        evaluation.IsActive = false;
        evaluation.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { message = "Evaluación desactivada exitosamente" });
    }

    /// <summary>
    /// Registrar calificación
    /// </summary>
    [HttpPost("{id}/grades")]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> RegisterGrade(int id, [FromBody] RegisterGradeDto dto)
    {
        var evaluation = await _context.Evaluations.FindAsync(id);
        if (evaluation == null)
            return NotFound(new { message = "Evaluación no encontrada" });

        var student = await _context.Students.FindAsync(dto.StudentId);
        if (student == null)
            return NotFound(new { message = "Estudiante no encontrado" });

        var teacher = await _context.Teachers.FindAsync(dto.TeacherId);
        if (teacher == null)
            return NotFound(new { message = "Docente no encontrado" });

        var enrollment = await _context.Enrollments
            .FirstOrDefaultAsync(e => e.StudentId == dto.StudentId && e.IsActive);
        if (enrollment == null)
            return BadRequest(new { message = "El estudiante no tiene una matrícula activa" });

        // Verificar si ya existe una calificación para este estudiante en esta evaluación
        var existingGrade = await _context.StudentGrades
            .FirstOrDefaultAsync(sg => sg.StudentId == dto.StudentId && sg.EvaluationId == id);

        if (existingGrade != null)
        {
            // Actualizar calificación existente
            existingGrade.Score = dto.Score;
            existingGrade.Comments = dto.Comments;
            existingGrade.GradedAt = DateTime.UtcNow;
            existingGrade.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            // Crear nueva calificación
            var studentGrade = new StudentGrade
            {
                StudentId = dto.StudentId,
                EnrollmentId = enrollment.Id,
                EvaluationId = id,
                TeacherId = dto.TeacherId,
                Score = dto.Score,
                Comments = dto.Comments,
                GradedAt = DateTime.UtcNow,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.StudentGrades.Add(studentGrade);
        }

        await _context.SaveChangesAsync();

        return Ok(new { message = "Calificación registrada exitosamente" });
    }

    /// <summary>
    /// Obtener calificaciones de una evaluación
    /// </summary>
    [HttpGet("{id}/grades")]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> GetEvaluationGrades(int id)
    {
        var grades = await _context.StudentGrades
            .Include(sg => sg.Student)
            .ThenInclude(s => s.User)
            .Include(sg => sg.Teacher)
            .ThenInclude(t => t.User)
            .Where(sg => sg.EvaluationId == id)
            .Select(sg => new {
                id = sg.Id,
                student = new {
                    id = sg.Student.Id,
                    firstName = sg.Student.User.FirstName,
                    lastName = sg.Student.User.LastName,
                    studentId = sg.Student.StudentId
                },
                score = sg.Score,
                comments = sg.Comments,
                teacher = new {
                    id = sg.Teacher.Id,
                    firstName = sg.Teacher.User.FirstName,
                    lastName = sg.Teacher.User.LastName
                },
                gradedAt = sg.GradedAt
            })
            .OrderBy(g => g.student.lastName)
            .ThenBy(g => g.student.firstName)
            .ToListAsync();

        return Ok(grades);
    }

    /// <summary>
    /// Obtener evaluaciones de una asignatura
    /// </summary>
    [HttpGet("subject/{subjectId}")]
    public async Task<IActionResult> GetSubjectEvaluations(int subjectId)
    {
        var evaluations = await _context.Evaluations
            .Include(e => e.AcademicPeriod)
            .Include(e => e.StudentGrades)
            .Where(e => e.SubjectId == subjectId)
            .Select(e => new {
                id = e.Id,
                name = e.Name,
                type = e.Type,
                weight = e.Weight,
                maxScore = e.MaxScore,
                dueDate = e.DueDate,
                evaluationDate = e.EvaluationDate,
                academicPeriod = new {
                    id = e.AcademicPeriod.Id,
                    name = e.AcademicPeriod.Name
                },
                gradesCount = e.StudentGrades.Count,
                averageScore = e.StudentGrades.Any() ? e.StudentGrades.Average(sg => sg.Score) : 0
            })
            .OrderByDescending(e => e.evaluationDate)
            .ToListAsync();

        return Ok(evaluations);
    }

    /// <summary>
    /// Obtener estadísticas de evaluaciones
    /// </summary>
    [HttpGet("stats")]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> GetEvaluationStats([FromQuery] int? subjectId, [FromQuery] int? academicPeriodId)
    {
        var query = _context.Evaluations.AsQueryable();

        if (subjectId.HasValue)
            query = query.Where(e => e.SubjectId == subjectId.Value);

        if (academicPeriodId.HasValue)
            query = query.Where(e => e.AcademicPeriodId == academicPeriodId.Value);

        var totalEvaluations = await query.CountAsync();
        var activeEvaluations = await query.CountAsync(e => e.IsActive);

        var statsByType = await query
            .GroupBy(e => e.Type)
            .Select(g => new {
                type = g.Key.ToString(),
                count = g.Count()
            })
            .ToListAsync();

        return Ok(new {
            totalEvaluations,
            activeEvaluations,
            inactiveEvaluations = totalEvaluations - activeEvaluations,
            byType = statsByType
        });
    }
}

public class CreateEvaluationDto
{
    [Required(ErrorMessage = "El ID de la asignatura es requerido")]
    public int SubjectId { get; set; }

    [Required(ErrorMessage = "El ID del período académico es requerido")]
    public int AcademicPeriodId { get; set; }

    [Required(ErrorMessage = "El nombre de la evaluación es requerido")]
    [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
    public string Name { get; set; } = string.Empty;

    [StringLength(255, ErrorMessage = "La descripción no puede exceder 255 caracteres")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "El tipo de evaluación es requerido")]
    public EvaluationType Type { get; set; }

    [Required(ErrorMessage = "El peso es requerido")]
    [Range(0, 100, ErrorMessage = "El peso debe estar entre 0 y 100")]
    public decimal Weight { get; set; }

    public DateTime? DueDate { get; set; }

    public DateTime? EvaluationDate { get; set; }

    [Required(ErrorMessage = "La puntuación máxima es requerida")]
    [Range(0, 100, ErrorMessage = "La puntuación máxima debe estar entre 0 y 100")]
    public decimal MaxScore { get; set; } = 100;
}

public class UpdateEvaluationDto
{
    [Required(ErrorMessage = "El nombre de la evaluación es requerido")]
    [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
    public string Name { get; set; } = string.Empty;

    [StringLength(255, ErrorMessage = "La descripción no puede exceder 255 caracteres")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "El tipo de evaluación es requerido")]
    public EvaluationType Type { get; set; }

    [Required(ErrorMessage = "El peso es requerido")]
    [Range(0, 100, ErrorMessage = "El peso debe estar entre 0 y 100")]
    public decimal Weight { get; set; }

    public DateTime? DueDate { get; set; }

    public DateTime? EvaluationDate { get; set; }

    [Required(ErrorMessage = "La puntuación máxima es requerida")]
    [Range(0, 100, ErrorMessage = "La puntuación máxima debe estar entre 0 y 100")]
    public decimal MaxScore { get; set; }
}

public class RegisterGradeDto
{
    [Required(ErrorMessage = "El ID del estudiante es requerido")]
    public int StudentId { get; set; }

    [Required(ErrorMessage = "El ID del docente es requerido")]
    public int TeacherId { get; set; }

    [Required(ErrorMessage = "La calificación es requerida")]
    [Range(0, 100, ErrorMessage = "La calificación debe estar entre 0 y 100")]
    public decimal Score { get; set; }

    [StringLength(255, ErrorMessage = "Los comentarios no pueden exceder 255 caracteres")]
    public string? Comments { get; set; }
} 