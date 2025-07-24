using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using api_school_system.Data;
using api_school_system.Dtos;
using api_school_system.Helpers;
using api_school_system.Models;

namespace api_school_system.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly JwtHelper _jwtHelper;

    public AuthController(AppDbContext context, JwtHelper jwtHelper)
    {
        _context = context;
        _jwtHelper = jwtHelper;
    }

    /// <summary>
    /// Autenticar usuario y obtener token JWT
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var user = await _context.Users
            .Include(u => u.UserPermissions)
            .ThenInclude(up => up.Permission)
            .FirstOrDefaultAsync(u => u.Username == dto.Username && u.IsActive);

        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return Unauthorized(new { message = "Credenciales inválidas" });

        // Update last login
        user.LastLoginAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        var token = _jwtHelper.GenerateToken(user);
        var permissions = user.UserPermissions
            .Where(up => up.IsActive)
            .Select(up => up.Permission.Name)
            .ToList();

        return Ok(new { 
            token, 
            user = new {
                id = user.Id,
                username = user.Username,
                email = user.Email,
                firstName = user.FirstName,
                lastName = user.LastName,
                role = user.Role,
                permissions
            }
        });
    }

    /// <summary>
    /// Registrar nuevo usuario
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        if (await _context.Users.AnyAsync(u => u.Username == dto.Username))
            return BadRequest(new { message = "El nombre de usuario ya existe" });

        if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
            return BadRequest(new { message = "El email ya está registrado" });

        var user = new User
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            Username = dto.Username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = dto.Role,
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

        return Ok(new { message = "Usuario registrado exitosamente", userId = user.Id });
    }

    /// <summary>
    /// Cambiar contraseña
    /// </summary>
    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        var userId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
        var user = await _context.Users.FindAsync(userId);

        if (user == null)
            return NotFound(new { message = "Usuario no encontrado" });

        if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
            return BadRequest(new { message = "Contraseña actual incorrecta" });

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { message = "Contraseña cambiada exitosamente" });
    }

    /// <summary>
    /// Obtener perfil del usuario autenticado
    /// </summary>
    [Authorize]
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var userId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
        var user = await _context.Users
            .Include(u => u.Teacher)
            .Include(u => u.Student)
            .Include(u => u.Parent)
            .FirstOrDefaultAsync(u => u.Id == userId);

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
            teacher = user.Teacher,
            student = user.Student,
            parent = user.Parent
        });
    }

    /// <summary>
    /// Actualizar perfil del usuario
    /// </summary>
    [Authorize]
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
    {
        var userId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
        var user = await _context.Users.FindAsync(userId);

        if (user == null)
            return NotFound(new { message = "Usuario no encontrado" });

        user.FirstName = dto.FirstName;
        user.LastName = dto.LastName;
        user.PhoneNumber = dto.PhoneNumber;
        user.Address = dto.Address;
        user.DateOfBirth = dto.DateOfBirth;
        user.Gender = dto.Gender;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { message = "Perfil actualizado exitosamente" });
    }
} 