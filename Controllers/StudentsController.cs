using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using api_school_system.Data;
using api_school_system.Dtos;
using api_school_system.Models;

namespace api_school_system.Controllers;

[ApiController]
[Route("students")]
public class StudentsController : ControllerBase
{
    private readonly AppDbContext _context;

    public StudentsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [Authorize(Roles = "admin,docente")]
    public IActionResult GetStudents() => Ok(_context.Students.ToList());

    [HttpPost]
    [Authorize(Roles = "admin")]
    public IActionResult Create([FromBody] RegisterStudentDto dto)
    {
        var student = new Student { FullName = dto.FullName };
        _context.Students.Add(student);
        _context.SaveChanges();
        return Ok(student);
    }
} 