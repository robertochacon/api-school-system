using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using api_school_system.Data;
using api_school_system.Dtos;
using api_school_system.Models;

namespace api_school_system.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StudentsController : ControllerBase
{
    private readonly AppDbContext _context;

    public StudentsController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Obtener todos los estudiantes
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> GetStudents([FromQuery] bool? isActive)
    {
        var query = _context.Students
            .Include(s => s.User)
            .Include(s => s.Enrollments)
            .ThenInclude(e => e.Course)
            .AsQueryable();

        if (isActive.HasValue)
            query = query.Where(s => s.IsActive == isActive.Value);

        var students = await query
            .Select(s => new {
                id = s.Id,
                studentId = s.StudentId,
                firstName = s.User.FirstName,
                lastName = s.User.LastName,
                email = s.User.Email,
                phoneNumber = s.User.PhoneNumber,
                enrollmentDate = s.EnrollmentDate,
                emergencyContact = s.EmergencyContact,
                emergencyPhone = s.EmergencyPhone,
                isActive = s.IsActive,
                enrollments = s.Enrollments.Count,
                createdAt = s.CreatedAt
            })
            .ToListAsync();

        return Ok(students);
    }

    /// <summary>
    /// Obtener estudiante por ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetStudent(int id)
    {
        var student = await _context.Students
            .Include(s => s.User)
            .Include(s => s.Enrollments)
            .ThenInclude(e => e.Course)
            .ThenInclude(c => c.Grade)
            .Include(s => s.StudentParents)
            .ThenInclude(sp => sp.Parent)
            .ThenInclude(p => p.User)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (student == null)
            return NotFound(new { message = "Estudiante no encontrado" });

        return Ok(new {
            id = student.Id,
            studentId = student.StudentId,
            user = new {
                id = student.User.Id,
                firstName = student.User.FirstName,
                lastName = student.User.LastName,
                email = student.User.Email,
                phoneNumber = student.User.PhoneNumber,
                address = student.User.Address,
                dateOfBirth = student.User.DateOfBirth,
                gender = student.User.Gender
            },
            enrollmentDate = student.EnrollmentDate,
            previousSchool = student.PreviousSchool,
            emergencyContact = student.EmergencyContact,
            emergencyPhone = student.EmergencyPhone,
            medicalConditions = student.MedicalConditions,
            allergies = student.Allergies,
            isActive = student.IsActive,
            enrollments = student.Enrollments.Select(e => new {
                id = e.Id,
                course = e.Course.Name,
                grade = e.Grade.Name,
                status = e.Status,
                enrollmentDate = e.EnrollmentDate
            }),
            parents = student.StudentParents.Select(sp => new {
                id = sp.Parent.Id,
                firstName = sp.Parent.User.FirstName,
                lastName = sp.Parent.User.LastName,
                relationship = sp.Relationship,
                isPrimaryContact = sp.IsPrimaryContact,
                isEmergencyContact = sp.IsEmergencyContact
            })
        });
    }

    /// <summary>
    /// Crear nuevo estudiante
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateStudent([FromBody] CreateStudentDto dto)
    {
        // Create user first
        var user = new User
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            Username = dto.Username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = UserRole.Student,
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

        // Create student
        var student = new Student
        {
            UserId = user.Id,
            StudentId = dto.StudentId,
            EnrollmentDate = dto.EnrollmentDate ?? DateTime.UtcNow,
            PreviousSchool = dto.PreviousSchool,
            EmergencyContact = dto.EmergencyContact,
            EmergencyPhone = dto.EmergencyPhone,
            MedicalConditions = dto.MedicalConditions,
            Allergies = dto.Allergies,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Students.Add(student);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Estudiante creado exitosamente", studentId = student.Id });
    }

    /// <summary>
    /// Actualizar estudiante
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateStudent(int id, [FromBody] UpdateStudentDto dto)
    {
        var student = await _context.Students
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (student == null)
            return NotFound(new { message = "Estudiante no encontrado" });

        // Update user
        student.User.FirstName = dto.FirstName;
        student.User.LastName = dto.LastName;
        student.User.PhoneNumber = dto.PhoneNumber;
        student.User.Address = dto.Address;
        student.User.UpdatedAt = DateTime.UtcNow;

        // Update student
        student.EmergencyContact = dto.EmergencyContact;
        student.EmergencyPhone = dto.EmergencyPhone;
        student.MedicalConditions = dto.MedicalConditions;
        student.Allergies = dto.Allergies;
        student.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { message = "Estudiante actualizado exitosamente" });
    }

    /// <summary>
    /// Desactivar estudiante
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeactivateStudent(int id)
    {
        var student = await _context.Students.FindAsync(id);
        if (student == null)
            return NotFound(new { message = "Estudiante no encontrado" });

        student.IsActive = false;
        student.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { message = "Estudiante desactivado exitosamente" });
    }
}

public class CreateStudentDto
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
    public string? StudentId { get; set; }
    public DateTime? EnrollmentDate { get; set; }
    public string? PreviousSchool { get; set; }
    public string? EmergencyContact { get; set; }
    public string? EmergencyPhone { get; set; }
    public string? MedicalConditions { get; set; }
    public string? Allergies { get; set; }
}

public class UpdateStudentDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public string? EmergencyContact { get; set; }
    public string? EmergencyPhone { get; set; }
    public string? MedicalConditions { get; set; }
    public string? Allergies { get; set; }
} 