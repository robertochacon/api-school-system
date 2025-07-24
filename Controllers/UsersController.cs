using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using api_school_system.Data;
using api_school_system.Models;

namespace api_school_system.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _context;

    public UsersController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Obtener todos los usuarios
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetUsers([FromQuery] string? role, [FromQuery] bool? isActive)
    {
        var query = _context.Users
            .Include(u => u.Teacher)
            .Include(u => u.Student)
            .Include(u => u.Parent)
            .AsQueryable();

        if (!string.IsNullOrEmpty(role))
            query = query.Where(u => u.Role.ToString() == role);

        if (isActive.HasValue)
            query = query.Where(u => u.IsActive == isActive.Value);

        var users = await query
            .Select(u => new {
                id = u.Id,
                firstName = u.FirstName,
                lastName = u.LastName,
                email = u.Email,
                username = u.Username,
                role = u.Role,
                phoneNumber = u.PhoneNumber,
                isActive = u.IsActive,
                createdAt = u.CreatedAt,
                lastLoginAt = u.LastLoginAt,
                teacher = u.Teacher != null ? new { u.Teacher.EmployeeId, u.Teacher.Specialization } : null,
                student = u.Student != null ? new { u.Student.StudentId } : null,
                parent = u.Parent != null ? new { u.Parent.ParentId, u.Parent.Relationship } : null
            })
            .ToListAsync();

        return Ok(users);
    }

    /// <summary>
    /// Obtener usuario por ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(int id)
    {
        var user = await _context.Users
            .Include(u => u.Teacher)
            .Include(u => u.Student)
            .Include(u => u.Parent)
            .Include(u => u.UserPermissions)
            .ThenInclude(up => up.Permission)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
            return NotFound(new { message = "Usuario no encontrado" });

        return Ok(new {
            id = user.Id,
            firstName = user.FirstName,
            lastName = user.LastName,
            email = user.Email,
            username = user.Username,
            role = user.Role,
            phoneNumber = user.PhoneNumber,
            address = user.Address,
            dateOfBirth = user.DateOfBirth,
            gender = user.Gender,
            profilePicture = user.ProfilePicture,
            documentType = user.DocumentType,
            documentNumber = user.DocumentNumber,
            nationality = user.Nationality,
            isActive = user.IsActive,
            createdAt = user.CreatedAt,
            updatedAt = user.UpdatedAt,
            lastLoginAt = user.LastLoginAt,
            teacher = user.Teacher,
            student = user.Student,
            parent = user.Parent,
            permissions = user.UserPermissions
                .Where(up => up.IsActive)
                .Select(up => up.Permission.Name)
                .ToList()
        });
    }

    /// <summary>
    /// Actualizar usuario
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDto dto)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
            return NotFound(new { message = "Usuario no encontrado" });

        user.FirstName = dto.FirstName;
        user.LastName = dto.LastName;
        user.Email = dto.Email;
        user.PhoneNumber = dto.PhoneNumber;
        user.Address = dto.Address;
        user.DateOfBirth = dto.DateOfBirth;
        user.Gender = dto.Gender;
        user.DocumentType = dto.DocumentType;
        user.DocumentNumber = dto.DocumentNumber;
        user.Nationality = dto.Nationality;
        user.IsActive = dto.IsActive;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { message = "Usuario actualizado exitosamente" });
    }

    /// <summary>
    /// Desactivar usuario
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeactivateUser(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
            return NotFound(new { message = "Usuario no encontrado" });

        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { message = "Usuario desactivado exitosamente" });
    }

    /// <summary>
    /// Obtener estadísticas de usuarios
    /// </summary>
    [HttpGet("stats")]
    public async Task<IActionResult> GetUserStats()
    {
        var stats = await _context.Users
            .GroupBy(u => u.Role)
            .Select(g => new {
                role = g.Key.ToString(),
                count = g.Count(),
                activeCount = g.Count(u => u.IsActive)
            })
            .ToListAsync();

        var totalUsers = await _context.Users.CountAsync();
        var activeUsers = await _context.Users.CountAsync(u => u.IsActive);

        return Ok(new {
            totalUsers,
            activeUsers,
            inactiveUsers = totalUsers - activeUsers,
            byRole = stats
        });
    }
}

public class UpdateUserDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public Gender? Gender { get; set; }
    public string? DocumentType { get; set; }
    public string? DocumentNumber { get; set; }
    public string? Nationality { get; set; }
    public bool IsActive { get; set; } = true;
} 