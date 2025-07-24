using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using api_school_system.Data;
using api_school_system.Models;

namespace api_school_system.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TeachersController : ControllerBase
{
    private readonly AppDbContext _context;

    public TeachersController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Obtener todos los docentes
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetTeachers([FromQuery] bool? isActive)
    {
        var query = _context.Teachers
            .Include(t => t.User)
            .Include(t => t.CourseTeachers)
            .ThenInclude(ct => ct.Course)
            .AsQueryable();

        if (isActive.HasValue)
            query = query.Where(t => t.IsActive == isActive.Value);

        var teachers = await query
            .Select(t => new {
                id = t.Id,
                employeeId = t.EmployeeId,
                firstName = t.User.FirstName,
                lastName = t.User.LastName,
                email = t.User.Email,
                phoneNumber = t.User.PhoneNumber,
                specialization = t.Specialization,
                degree = t.Degree,
                university = t.University,
                hireDate = t.HireDate,
                isActive = t.IsActive,
                coursesCount = t.CourseTeachers.Count,
                createdAt = t.CreatedAt
            })
            .ToListAsync();

        return Ok(teachers);
    }

    /// <summary>
    /// Obtener docente por ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetTeacher(int id)
    {
        var teacher = await _context.Teachers
            .Include(t => t.User)
            .Include(t => t.CourseTeachers)
            .ThenInclude(ct => ct.Course)
            .ThenInclude(c => c.Grade)
            .Include(t => t.SubjectTeachers)
            .ThenInclude(st => st.Subject)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (teacher == null)
            return NotFound(new { message = "Docente no encontrado" });

        return Ok(new {
            id = teacher.Id,
            employeeId = teacher.EmployeeId,
            user = new {
                id = teacher.User.Id,
                firstName = teacher.User.FirstName,
                lastName = teacher.User.LastName,
                email = teacher.User.Email,
                phoneNumber = teacher.User.PhoneNumber,
                address = teacher.User.Address,
                dateOfBirth = teacher.User.DateOfBirth,
                gender = teacher.User.Gender
            },
            specialization = teacher.Specialization,
            degree = teacher.Degree,
            university = teacher.University,
            hireDate = teacher.HireDate,
            isActive = teacher.IsActive,
            courses = teacher.CourseTeachers.Select(ct => new {
                id = ct.Course.Id,
                name = ct.Course.Name,
                grade = ct.Course.Grade.Name,
                role = ct.Role
            }),
            subjects = teacher.SubjectTeachers.Select(st => new {
                id = st.Subject.Id,
                name = st.Subject.Name,
                code = st.Subject.Code,
                department = st.Subject.Department
            })
        });
    }

    /// <summary>
    /// Crear nuevo docente
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateTeacher([FromBody] CreateTeacherDto dto)
    {
        // Create user first
        var user = new User
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            Username = dto.Username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = UserRole.Teacher,
            PhoneNumber = dto.PhoneNumber,
            Address = dto.Address,
            DateOfBirth = dto.DateOfBirth,
            Gender = dto.Gender,
            DocumentType = dto.DocumentType,
            DocumentNumber = dto.DocumentNumber,
            Nationality = dto.Nationality,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Create teacher
        var teacher = new Teacher
        {
            UserId = user.Id,
            EmployeeId = dto.EmployeeId,
            Specialization = dto.Specialization,
            Degree = dto.Degree,
            University = dto.University,
            HireDate = dto.HireDate ?? DateTime.UtcNow,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Teachers.Add(teacher);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Docente creado exitosamente", teacherId = teacher.Id });
    }

    /// <summary>
    /// Actualizar docente
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateTeacher(int id, [FromBody] UpdateTeacherDto dto)
    {
        var teacher = await _context.Teachers
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (teacher == null)
            return NotFound(new { message = "Docente no encontrado" });

        // Update user
        teacher.User.FirstName = dto.FirstName;
        teacher.User.LastName = dto.LastName;
        teacher.User.PhoneNumber = dto.PhoneNumber;
        teacher.User.Address = dto.Address;
        teacher.User.UpdatedAt = DateTime.UtcNow;

        // Update teacher
        teacher.Specialization = dto.Specialization;
        teacher.Degree = dto.Degree;
        teacher.University = dto.University;
        teacher.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { message = "Docente actualizado exitosamente" });
    }

    /// <summary>
    /// Desactivar docente
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeactivateTeacher(int id)
    {
        var teacher = await _context.Teachers.FindAsync(id);
        if (teacher == null)
            return NotFound(new { message = "Docente no encontrado" });

        teacher.IsActive = false;
        teacher.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { message = "Docente desactivado exitosamente" });
    }

    /// <summary>
    /// Obtener carga académica del docente
    /// </summary>
    [HttpGet("{id}/workload")]
    public async Task<IActionResult> GetTeacherWorkload(int id)
    {
        var teacher = await _context.Teachers
            .Include(t => t.CourseTeachers)
            .ThenInclude(ct => ct.Course)
            .ThenInclude(c => c.Grade)
            .Include(t => t.SubjectTeachers)
            .ThenInclude(st => st.Subject)
            .Include(t => t.Schedules)
            .ThenInclude(s => s.Course)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (teacher == null)
            return NotFound(new { message = "Docente no encontrado" });

        return Ok(new {
            teacherId = teacher.Id,
            courses = teacher.CourseTeachers.Select(ct => new {
                id = ct.Course.Id,
                name = ct.Course.Name,
                grade = ct.Course.Grade.Name,
                role = ct.Role
            }),
            subjects = teacher.SubjectTeachers.Select(st => new {
                id = st.Subject.Id,
                name = st.Subject.Name,
                code = st.Subject.Code,
                credits = st.Subject.Credits,
                hoursPerWeek = st.Subject.HoursPerWeek
            }),
            schedules = teacher.Schedules.Select(s => new {
                id = s.Id,
                course = s.Course.Name,
                subject = s.Subject.Name,
                dayOfWeek = s.DayOfWeek,
                startTime = s.StartTime,
                endTime = s.EndTime,
                room = s.Room
            })
        });
    }
}

public class CreateTeacherDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public Gender? Gender { get; set; }
    public string? DocumentType { get; set; }
    public string? DocumentNumber { get; set; }
    public string? Nationality { get; set; }
    public string? EmployeeId { get; set; }
    public string? Specialization { get; set; }
    public string? Degree { get; set; }
    public string? University { get; set; }
    public DateTime? HireDate { get; set; }
}

public class UpdateTeacherDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public string? Specialization { get; set; }
    public string? Degree { get; set; }
    public string? University { get; set; }
} 