using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using api_school_system.Data;
using api_school_system.Models;
using System.ComponentModel.DataAnnotations;

namespace api_school_system.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ParentsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ParentsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/parents
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetParents(
            [FromQuery] string? searchTerm = null,
            [FromQuery] string? relationship = null,
            [FromQuery] bool? isEmergencyContact = null,
            [FromQuery] bool? isActive = null)
        {
            var query = _context.Parents
                .Include(p => p.User)
                .Where(p => p.IsActive);

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(p => p.User.FirstName.Contains(searchTerm) ||
                                       p.User.LastName.Contains(searchTerm) ||
                                       p.User.Email.Contains(searchTerm) ||
                                       p.ParentId.Contains(searchTerm));
            }
            if (!string.IsNullOrEmpty(relationship))
                query = query.Where(p => p.Relationship.Contains(relationship));
            if (isEmergencyContact.HasValue)
                query = query.Where(p => p.IsEmergencyContact == isEmergencyContact.Value);
            if (isActive.HasValue)
                query = query.Where(p => p.IsActive == isActive.Value);

            var parents = await query
                .OrderBy(p => p.User.LastName)
                .ThenBy(p => p.User.FirstName)
                .Select(p => new
                {
                    p.Id,
                    p.UserId,
                    p.ParentId,
                    p.Relationship,
                    p.Occupation,
                    p.Workplace,
                    p.WorkPhone,
                    p.IsEmergencyContact,
                    p.IsActive,
                    p.CreatedAt,
                    p.UpdatedAt,
                    User = new
                    {
                        p.User.Id,
                        p.User.FirstName,
                        p.User.LastName,
                        p.User.Email,
                        p.User.PhoneNumber,
                        p.User.Address
                    },
                    StudentCount = p.StudentParents.Count(sp => sp.IsActive)
                })
                .ToListAsync();

            return Ok(parents);
        }

        // GET: api/parents/5
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetParent(int id)
        {
            var parent = await _context.Parents
                .Include(p => p.User)
                .Include(p => p.StudentParents)
                    .ThenInclude(sp => sp.Student)
                        .ThenInclude(s => s.User)
                .Where(p => p.Id == id && p.IsActive)
                .Select(p => new
                {
                    p.Id,
                    p.UserId,
                    p.ParentId,
                    p.Relationship,
                    p.Occupation,
                    p.Workplace,
                    p.WorkPhone,
                    p.IsEmergencyContact,
                    p.IsActive,
                    p.CreatedAt,
                    p.UpdatedAt,
                    User = new
                    {
                        p.User.Id,
                        p.User.FirstName,
                        p.User.LastName,
                        p.User.Email,
                        p.User.PhoneNumber,
                        p.User.Address,
                        p.User.DateOfBirth,
                        p.User.Gender,
                        p.User.DocumentType,
                        p.User.DocumentNumber,
                        p.User.Nationality
                    },
                    Students = p.StudentParents
                        .Where(sp => sp.IsActive)
                        .Select(sp => new
                        {
                            sp.Id,
                            sp.Relationship,
                            sp.IsPrimaryContact,
                            sp.IsEmergencyContact,
                            Student = new
                            {
                                sp.Student.Id,
                                sp.Student.StudentId,
                                sp.Student.User.FirstName,
                                sp.Student.User.LastName,
                                sp.Student.User.Email,
                                sp.Student.EnrollmentDate
                            }
                        })
                })
                .FirstOrDefaultAsync();

            if (parent == null)
            {
                return NotFound("Padre/Tutor no encontrado");
            }

            return Ok(parent);
        }

        // POST: api/parents
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Parent>> CreateParent(CreateParentDto dto)
        {
            // Validar que el usuario existe
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == dto.UserId && u.IsActive);
            if (user == null)
                return BadRequest("Usuario no encontrado o inactivo");

            // Verificar que el usuario no sea ya un padre
            var existingParent = await _context.Parents
                .FirstOrDefaultAsync(p => p.UserId == dto.UserId && p.IsActive);
            if (existingParent != null)
                return BadRequest("El usuario ya está registrado como padre/tutor");

            // Verificar que el ParentId sea único
            var existingParentId = await _context.Parents
                .FirstOrDefaultAsync(p => p.ParentId == dto.ParentId && p.IsActive);
            if (existingParentId != null)
                return BadRequest("Ya existe un padre/tutor con este ID");

            var parent = new Parent
            {
                UserId = dto.UserId,
                ParentId = dto.ParentId,
                Relationship = dto.Relationship,
                Occupation = dto.Occupation,
                Workplace = dto.Workplace,
                WorkPhone = dto.WorkPhone,
                IsEmergencyContact = dto.IsEmergencyContact,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Parents.Add(parent);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetParent), new { id = parent.Id }, parent);
        }

        // PUT: api/parents/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateParent(int id, UpdateParentDto dto)
        {
            var parent = await _context.Parents.FindAsync(id);
            if (parent == null || !parent.IsActive)
                return NotFound("Padre/Tutor no encontrado");

            // Verificar que el ParentId sea único (si se está cambiando)
            if (!string.IsNullOrEmpty(dto.ParentId) && dto.ParentId != parent.ParentId)
            {
                var existingParentId = await _context.Parents
                    .FirstOrDefaultAsync(p => p.ParentId == dto.ParentId && p.IsActive);
                if (existingParentId != null)
                    return BadRequest("Ya existe un padre/tutor con este ID");
            }

            // Actualizar campos
            if (!string.IsNullOrEmpty(dto.ParentId)) parent.ParentId = dto.ParentId;
            if (!string.IsNullOrEmpty(dto.Relationship)) parent.Relationship = dto.Relationship;
            if (dto.Occupation != null) parent.Occupation = dto.Occupation;
            if (dto.Workplace != null) parent.Workplace = dto.Workplace;
            if (dto.WorkPhone != null) parent.WorkPhone = dto.WorkPhone;
            if (dto.IsEmergencyContact.HasValue) parent.IsEmergencyContact = dto.IsEmergencyContact.Value;

            parent.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/parents/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeactivateParent(int id)
        {
            var parent = await _context.Parents.FindAsync(id);
            if (parent == null || !parent.IsActive)
                return NotFound("Padre/Tutor no encontrado");

            // Verificar si tiene estudiantes asociados
            var hasStudents = await _context.StudentParents
                .AnyAsync(sp => sp.ParentId == id && sp.IsActive);
            if (hasStudents)
                return BadRequest("No se puede desactivar un padre/tutor que tiene estudiantes asociados");

            parent.IsActive = false;
            parent.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/parents/student/{studentId}
        [HttpGet("student/{studentId}")]
        public async Task<ActionResult<IEnumerable<object>>> GetStudentParents(int studentId)
        {
            var parents = await _context.StudentParents
                .Include(sp => sp.Parent)
                    .ThenInclude(p => p.User)
                .Where(sp => sp.StudentId == studentId && sp.IsActive)
                .OrderBy(sp => sp.IsPrimaryContact)
                .ThenBy(sp => sp.Parent.User.LastName)
                .Select(sp => new
                {
                    sp.Id,
                    sp.Relationship,
                    sp.IsPrimaryContact,
                    sp.IsEmergencyContact,
                    sp.CreatedAt,
                    Parent = new
                    {
                        sp.Parent.Id,
                        sp.Parent.ParentId,
                        sp.Parent.Relationship,
                        sp.Parent.Occupation,
                        sp.Parent.Workplace,
                        sp.Parent.WorkPhone,
                        sp.Parent.IsEmergencyContact,
                        User = new
                        {
                            sp.Parent.User.Id,
                            sp.Parent.User.FirstName,
                            sp.Parent.User.LastName,
                            sp.Parent.User.Email,
                            sp.Parent.User.PhoneNumber,
                            sp.Parent.User.Address
                        }
                    }
                })
                .ToListAsync();

            return Ok(parents);
        }

        // POST: api/parents/student/{studentId}
        [HttpPost("student/{studentId}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<StudentParent>> AssignParentToStudent(int studentId, AssignParentDto dto)
        {
            // Validar que el estudiante existe
            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.Id == studentId && s.IsActive);
            if (student == null)
                return BadRequest("Estudiante no encontrado o inactivo");

            // Validar que el padre existe
            var parent = await _context.Parents
                .FirstOrDefaultAsync(p => p.Id == dto.ParentId && p.IsActive);
            if (parent == null)
                return BadRequest("Padre/Tutor no encontrado o inactivo");

            // Verificar que no existe ya esta relación
            var existingRelation = await _context.StudentParents
                .FirstOrDefaultAsync(sp => sp.StudentId == studentId && 
                                         sp.ParentId == dto.ParentId && 
                                         sp.IsActive);
            if (existingRelation != null)
                return BadRequest("El padre/tutor ya está asignado a este estudiante");

            // Si es contacto primario, desactivar otros contactos primarios del estudiante
            if (dto.IsPrimaryContact)
            {
                var primaryContacts = await _context.StudentParents
                    .Where(sp => sp.StudentId == studentId && 
                               sp.IsPrimaryContact && 
                               sp.IsActive)
                    .ToListAsync();

                foreach (var contact in primaryContacts)
                {
                    contact.IsPrimaryContact = false;
                }
            }

            // Si es contacto de emergencia, desactivar otros contactos de emergencia del estudiante
            if (dto.IsEmergencyContact)
            {
                var emergencyContacts = await _context.StudentParents
                    .Where(sp => sp.StudentId == studentId && 
                               sp.IsEmergencyContact && 
                               sp.IsActive)
                    .ToListAsync();

                foreach (var contact in emergencyContacts)
                {
                    contact.IsEmergencyContact = false;
                }
            }

            var studentParent = new StudentParent
            {
                StudentId = studentId,
                ParentId = dto.ParentId,
                Relationship = dto.Relationship,
                IsPrimaryContact = dto.IsPrimaryContact,
                IsEmergencyContact = dto.IsEmergencyContact,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.StudentParents.Add(studentParent);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetStudentParents), new { studentId }, studentParent);
        }

        // PUT: api/parents/student/{studentParentId}
        [HttpPut("student/{studentParentId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateStudentParent(int studentParentId, UpdateStudentParentDto dto)
        {
            var studentParent = await _context.StudentParents.FindAsync(studentParentId);
            if (studentParent == null || !studentParent.IsActive)
                return NotFound("Relación estudiante-padre no encontrada");

            // Si se está marcando como contacto primario, desactivar otros contactos primarios
            if (dto.IsPrimaryContact == true)
            {
                var primaryContacts = await _context.StudentParents
                    .Where(sp => sp.StudentId == studentParent.StudentId && 
                               sp.Id != studentParentId &&
                               sp.IsPrimaryContact && 
                               sp.IsActive)
                    .ToListAsync();

                foreach (var contact in primaryContacts)
                {
                    contact.IsPrimaryContact = false;
                }
            }

            // Si se está marcando como contacto de emergencia, desactivar otros contactos de emergencia
            if (dto.IsEmergencyContact == true)
            {
                var emergencyContacts = await _context.StudentParents
                    .Where(sp => sp.StudentId == studentParent.StudentId && 
                               sp.Id != studentParentId &&
                               sp.IsEmergencyContact && 
                               sp.IsActive)
                    .ToListAsync();

                foreach (var contact in emergencyContacts)
                {
                    contact.IsEmergencyContact = false;
                }
            }

            // Actualizar campos
            if (!string.IsNullOrEmpty(dto.Relationship)) studentParent.Relationship = dto.Relationship;
            if (dto.IsPrimaryContact.HasValue) studentParent.IsPrimaryContact = dto.IsPrimaryContact.Value;
            if (dto.IsEmergencyContact.HasValue) studentParent.IsEmergencyContact = dto.IsEmergencyContact.Value;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/parents/student/{studentParentId}
        [HttpDelete("student/{studentParentId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RemoveParentFromStudent(int studentParentId)
        {
            var studentParent = await _context.StudentParents.FindAsync(studentParentId);
            if (studentParent == null || !studentParent.IsActive)
                return NotFound("Relación estudiante-padre no encontrada");

            studentParent.IsActive = false;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/parents/emergency-contacts
        [HttpGet("emergency-contacts")]
        public async Task<ActionResult<IEnumerable<object>>> GetEmergencyContacts()
        {
            var emergencyContacts = await _context.Parents
                .Include(p => p.User)
                .Include(p => p.StudentParents)
                    .ThenInclude(sp => sp.Student)
                        .ThenInclude(s => s.User)
                .Where(p => p.IsEmergencyContact && p.IsActive)
                .OrderBy(p => p.User.LastName)
                .ThenBy(p => p.User.FirstName)
                .Select(p => new
                {
                    p.Id,
                    p.ParentId,
                    p.Relationship,
                    p.WorkPhone,
                    User = new
                    {
                        p.User.Id,
                        p.User.FirstName,
                        p.User.LastName,
                        p.User.Email,
                        p.User.PhoneNumber
                    },
                    Students = p.StudentParents
                        .Where(sp => sp.IsActive && sp.IsEmergencyContact)
                        .Select(sp => new
                        {
                            sp.Student.Id,
                            sp.Student.StudentId,
                            sp.Student.User.FirstName,
                            sp.Student.User.LastName
                        })
                })
                .ToListAsync();

            return Ok(emergencyContacts);
        }

        // GET: api/parents/stats
        [HttpGet("stats")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<object>> GetParentStats()
        {
            var totalParents = await _context.Parents.CountAsync(p => p.IsActive);
            var emergencyContacts = await _context.Parents.CountAsync(p => p.IsEmergencyContact && p.IsActive);
            var parentsWithStudents = await _context.Parents
                .CountAsync(p => p.StudentParents.Any(sp => sp.IsActive) && p.IsActive);

            var parentsByRelationship = await _context.Parents
                .Where(p => p.IsActive)
                .GroupBy(p => p.Relationship)
                .Select(g => new
                {
                    Relationship = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .ToListAsync();

            var topParentsByStudentCount = await _context.Parents
                .Include(p => p.User)
                .Where(p => p.IsActive)
                .Select(p => new
                {
                    p.Id,
                    p.ParentId,
                    ParentName = $"{p.User.FirstName} {p.User.LastName}",
                    StudentCount = p.StudentParents.Count(sp => sp.IsActive)
                })
                .Where(p => p.StudentCount > 0)
                .OrderByDescending(p => p.StudentCount)
                .Take(10)
                .ToListAsync();

            return Ok(new
            {
                TotalParents = totalParents,
                EmergencyContacts = emergencyContacts,
                ParentsWithStudents = parentsWithStudents,
                ParentsByRelationship = parentsByRelationship,
                TopParentsByStudentCount = topParentsByStudentCount
            });
        }
    }

    public class CreateParentDto
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        [StringLength(20)]
        public string ParentId { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Relationship { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Occupation { get; set; }

        [StringLength(200)]
        public string? Workplace { get; set; }

        [StringLength(20)]
        public string? WorkPhone { get; set; }

        public bool IsEmergencyContact { get; set; } = false;
    }

    public class UpdateParentDto
    {
        [StringLength(20)]
        public string? ParentId { get; set; }

        [StringLength(50)]
        public string? Relationship { get; set; }

        [StringLength(100)]
        public string? Occupation { get; set; }

        [StringLength(200)]
        public string? Workplace { get; set; }

        [StringLength(20)]
        public string? WorkPhone { get; set; }

        public bool? IsEmergencyContact { get; set; }
    }

    public class AssignParentDto
    {
        [Required]
        public int ParentId { get; set; }

        [Required]
        [StringLength(50)]
        public string Relationship { get; set; } = string.Empty;

        public bool IsPrimaryContact { get; set; } = false;
        public bool IsEmergencyContact { get; set; } = false;
    }

    public class UpdateStudentParentDto
    {
        [StringLength(50)]
        public string? Relationship { get; set; }
        public bool? IsPrimaryContact { get; set; }
        public bool? IsEmergencyContact { get; set; }
    }
} 