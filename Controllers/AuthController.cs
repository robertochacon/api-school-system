using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using api_school_system.Data;
using api_school_system.Dtos;
using api_school_system.Helpers;
using api_school_system.Models;

namespace api_school_system.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly JwtHelper _jwtHelper;

    public AuthController(AppDbContext context, JwtHelper jwtHelper)
    {
        _context = context;
        _jwtHelper = jwtHelper;
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginDto dto)
    {
        var user = _context.Users.FirstOrDefault(u => u.Username == dto.Username);
        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return Unauthorized();

        var token = _jwtHelper.GenerateToken(user);
        return Ok(new { token });
    }
} 