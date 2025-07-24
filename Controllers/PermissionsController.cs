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
    [Authorize(Roles = "Admin")]
    public class PermissionsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PermissionsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/permissions
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetPermissions(
            [FromQuery] string? module = null,
            [FromQuery] string? action = null,
            [FromQuery] bool? isActive = null)
        {
            var query = _context.Permissions.Where(p => p.IsActive);

            if (!string.IsNullOrEmpty(module))
                query = query.Where(p => p.Module.Contains(module));
            if (!string.IsNullOrEmpty(action))
                query = query.Where(p => p.Action.Contains(action));
            if (isActive.HasValue)
                query = query.Where(p => p.IsActive == isActive.Value);

            var permissions = await query
                .OrderBy(p => p.Module)
                .ThenBy(p => p.Action)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Description,
                    p.Module,
                    p.Action,
                    p.IsActive,
                    p.CreatedAt,
                    p.UpdatedAt,
                    UserCount = p.UserPermissions.Count(up => up.IsActive)
                })
                .ToListAsync();

            return Ok(permissions);
        }

        // GET: api/permissions/5
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetPermission(int id)
        {
            var permission = await _context.Permissions
                .Where(p => p.Id == id && p.IsActive)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Description,
                    p.Module,
                    p.Action,
                    p.IsActive,
                    p.CreatedAt,
                    p.UpdatedAt,
                    Users = p.UserPermissions
                        .Where(up => up.IsActive)
                        .Select(up => new
                        {
                            up.Id,
                            up.GrantedAt,
                            up.GrantedByUserId,
                            up.IsActive,
                            User = new
                            {
                                up.User.Id,
                                up.User.FirstName,
                                up.User.LastName,
                                up.User.Email,
                                up.User.Username
                            }
                        })
                })
                .FirstOrDefaultAsync();

            if (permission == null)
            {
                return NotFound("Permiso no encontrado");
            }

            return Ok(permission);
        }

        // POST: api/permissions
        [HttpPost]
        public async Task<ActionResult<Permission>> CreatePermission(CreatePermissionDto dto)
        {
            // Verificar que el nombre sea único
            var existingPermission = await _context.Permissions
                .FirstOrDefaultAsync(p => p.Name == dto.Name && p.IsActive);
            if (existingPermission != null)
                return BadRequest("Ya existe un permiso con este nombre");

            // Verificar que la combinación módulo-acción sea única
            var existingModuleAction = await _context.Permissions
                .FirstOrDefaultAsync(p => p.Module == dto.Module && 
                                        p.Action == dto.Action && 
                                        p.IsActive);
            if (existingModuleAction != null)
                return BadRequest("Ya existe un permiso con esta combinación de módulo y acción");

            var permission = new Permission
            {
                Name = dto.Name,
                Description = dto.Description,
                Module = dto.Module,
                Action = dto.Action,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Permissions.Add(permission);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPermission), new { id = permission.Id }, permission);
        }

        // PUT: api/permissions/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePermission(int id, UpdatePermissionDto dto)
        {
            var permission = await _context.Permissions.FindAsync(id);
            if (permission == null || !permission.IsActive)
                return NotFound("Permiso no encontrado");

            // Verificar que el nombre sea único (si se está cambiando)
            if (!string.IsNullOrEmpty(dto.Name) && dto.Name != permission.Name)
            {
                var existingPermission = await _context.Permissions
                    .FirstOrDefaultAsync(p => p.Name == dto.Name && p.IsActive);
                if (existingPermission != null)
                    return BadRequest("Ya existe un permiso con este nombre");
            }

            // Verificar que la combinación módulo-acción sea única (si se está cambiando)
            if ((!string.IsNullOrEmpty(dto.Module) && dto.Module != permission.Module) ||
                (!string.IsNullOrEmpty(dto.Action) && dto.Action != permission.Action))
            {
                var module = dto.Module ?? permission.Module;
                var action = dto.Action ?? permission.Action;
                var existingModuleAction = await _context.Permissions
                    .Where(p => p.Id != id && p.IsActive)
                    .FirstOrDefaultAsync(p => p.Module == module && p.Action == action);
                if (existingModuleAction != null)
                    return BadRequest("Ya existe un permiso con esta combinación de módulo y acción");
            }

            // Actualizar campos
            if (!string.IsNullOrEmpty(dto.Name)) permission.Name = dto.Name;
            if (dto.Description != null) permission.Description = dto.Description;
            if (!string.IsNullOrEmpty(dto.Module)) permission.Module = dto.Module;
            if (!string.IsNullOrEmpty(dto.Action)) permission.Action = dto.Action;

            permission.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/permissions/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeactivatePermission(int id)
        {
            var permission = await _context.Permissions.FindAsync(id);
            if (permission == null || !permission.IsActive)
                return NotFound("Permiso no encontrado");

            // Verificar si hay usuarios con este permiso
            var hasUsers = await _context.UserPermissions
                .AnyAsync(up => up.PermissionId == id && up.IsActive);
            if (hasUsers)
                return BadRequest("No se puede desactivar un permiso que está asignado a usuarios");

            permission.IsActive = false;
            permission.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/permissions/modules
        [HttpGet("modules")]
        public async Task<ActionResult<IEnumerable<string>>> GetModules()
        {
            var modules = await _context.Permissions
                .Where(p => p.IsActive)
                .Select(p => p.Module)
                .Distinct()
                .OrderBy(m => m)
                .ToListAsync();

            return Ok(modules);
        }

        // GET: api/permissions/actions
        [HttpGet("actions")]
        public async Task<ActionResult<IEnumerable<string>>> GetActions()
        {
            var actions = await _context.Permissions
                .Where(p => p.IsActive)
                .Select(p => p.Action)
                .Distinct()
                .OrderBy(a => a)
                .ToListAsync();

            return Ok(actions);
        }

        // GET: api/permissions/module/{module}
        [HttpGet("module/{module}")]
        public async Task<ActionResult<IEnumerable<object>>> GetPermissionsByModule(string module)
        {
            var permissions = await _context.Permissions
                .Where(p => p.Module == module && p.IsActive)
                .OrderBy(p => p.Action)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Description,
                    p.Action,
                    p.IsActive,
                    UserCount = p.UserPermissions.Count(up => up.IsActive)
                })
                .ToListAsync();

            return Ok(permissions);
        }

        // POST: api/permissions/assign
        [HttpPost("assign")]
        public async Task<ActionResult<object>> AssignPermission(AssignPermissionDto dto)
        {
            // Validar que el usuario existe
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == dto.UserId && u.IsActive);
            if (user == null)
                return BadRequest("Usuario no encontrado o inactivo");

            // Validar que el permiso existe
            var permission = await _context.Permissions
                .FirstOrDefaultAsync(p => p.Id == dto.PermissionId && p.IsActive);
            if (permission == null)
                return BadRequest("Permiso no encontrado o inactivo");

            // Verificar que no existe ya esta asignación
            var existingAssignment = await _context.UserPermissions
                .FirstOrDefaultAsync(up => up.UserId == dto.UserId && 
                                         up.PermissionId == dto.PermissionId && 
                                         up.IsActive);
            if (existingAssignment != null)
                return BadRequest("El usuario ya tiene este permiso asignado");

            var userPermission = new UserPermission
            {
                UserId = dto.UserId,
                PermissionId = dto.PermissionId,
                GrantedAt = DateTime.UtcNow,
                GrantedByUserId = dto.GrantedByUserId,
                IsActive = true
            };

            _context.UserPermissions.Add(userPermission);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Permiso asignado correctamente" });
        }

        // DELETE: api/permissions/revoke/{userPermissionId}
        [HttpDelete("revoke/{userPermissionId}")]
        public async Task<IActionResult> RevokePermission(int userPermissionId)
        {
            var userPermission = await _context.UserPermissions.FindAsync(userPermissionId);
            if (userPermission == null || !userPermission.IsActive)
                return NotFound("Asignación de permiso no encontrada");

            userPermission.IsActive = false;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/permissions/user/{userId}
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<object>>> GetUserPermissions(int userId)
        {
            var userPermissions = await _context.UserPermissions
                .Include(up => up.Permission)
                .Where(up => up.UserId == userId && up.IsActive)
                .OrderBy(up => up.Permission.Module)
                .ThenBy(up => up.Permission.Action)
                .Select(up => new
                {
                    up.Id,
                    up.GrantedAt,
                    up.GrantedByUserId,
                    Permission = new
                    {
                        up.Permission.Id,
                        up.Permission.Name,
                        up.Permission.Description,
                        up.Permission.Module,
                        up.Permission.Action
                    }
                })
                .ToListAsync();

            return Ok(userPermissions);
        }

        // POST: api/permissions/bulk-assign
        [HttpPost("bulk-assign")]
        public async Task<ActionResult<object>> BulkAssignPermissions(BulkAssignPermissionDto dto)
        {
            // Validar que el usuario existe
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == dto.UserId && u.IsActive);
            if (user == null)
                return BadRequest("Usuario no encontrado o inactivo");

            var results = new List<object>();
            var errors = new List<string>();

            foreach (var permissionId in dto.PermissionIds)
            {
                try
                {
                    // Validar que el permiso existe
                    var permission = await _context.Permissions
                        .FirstOrDefaultAsync(p => p.Id == permissionId && p.IsActive);
                    if (permission == null)
                    {
                        errors.Add($"Permiso ID {permissionId} no encontrado o inactivo");
                        continue;
                    }

                    // Verificar que no existe ya esta asignación
                    var existingAssignment = await _context.UserPermissions
                        .FirstOrDefaultAsync(up => up.UserId == dto.UserId && 
                                                 up.PermissionId == permissionId && 
                                                 up.IsActive);
                    if (existingAssignment != null)
                    {
                        errors.Add($"El usuario ya tiene el permiso '{permission.Name}' asignado");
                        continue;
                    }

                    var userPermission = new UserPermission
                    {
                        UserId = dto.UserId,
                        PermissionId = permissionId,
                        GrantedAt = DateTime.UtcNow,
                        GrantedByUserId = dto.GrantedByUserId,
                        IsActive = true
                    };

                    _context.UserPermissions.Add(userPermission);
                    results.Add(new { PermissionId = permissionId, Status = "Assigned" });
                }
                catch (Exception ex)
                {
                    errors.Add($"Error procesando permiso ID {permissionId}: {ex.Message}");
                }
            }

            if (results.Any())
            {
                await _context.SaveChangesAsync();
            }

            return Ok(new
            {
                Assigned = results.Count,
                Errors = errors,
                Results = results
            });
        }

        // GET: api/permissions/stats
        [HttpGet("stats")]
        public async Task<ActionResult<object>> GetPermissionStats()
        {
            var totalPermissions = await _context.Permissions.CountAsync(p => p.IsActive);
            var permissionsWithUsers = await _context.Permissions
                .CountAsync(p => p.UserPermissions.Any(up => up.IsActive) && p.IsActive);

            var permissionsByModule = await _context.Permissions
                .Where(p => p.IsActive)
                .GroupBy(p => p.Module)
                .Select(g => new
                {
                    Module = g.Key,
                    Count = g.Count(),
                    UserCount = g.Sum(p => p.UserPermissions.Count(up => up.IsActive))
                })
                .OrderByDescending(x => x.Count)
                .ToListAsync();

            var permissionsByAction = await _context.Permissions
                .Where(p => p.IsActive)
                .GroupBy(p => p.Action)
                .Select(g => new
                {
                    Action = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .ToListAsync();

            var topPermissionsByUserCount = await _context.Permissions
                .Where(p => p.IsActive)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Module,
                    p.Action,
                    UserCount = p.UserPermissions.Count(up => up.IsActive)
                })
                .Where(p => p.UserCount > 0)
                .OrderByDescending(p => p.UserCount)
                .Take(10)
                .ToListAsync();

            return Ok(new
            {
                TotalPermissions = totalPermissions,
                PermissionsWithUsers = permissionsWithUsers,
                PermissionsByModule = permissionsByModule,
                PermissionsByAction = permissionsByAction,
                TopPermissionsByUserCount = topPermissionsByUserCount
            });
        }
    }

    public class CreatePermissionDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        [StringLength(50)]
        public string Module { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Action { get; set; } = string.Empty;
    }

    public class UpdatePermissionDto
    {
        [StringLength(100)]
        public string? Name { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(50)]
        public string? Module { get; set; }

        [StringLength(50)]
        public string? Action { get; set; }
    }

    public class AssignPermissionDto
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public int PermissionId { get; set; }

        [Required]
        public int GrantedByUserId { get; set; }
    }

    public class BulkAssignPermissionDto
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public List<int> PermissionIds { get; set; } = new();

        [Required]
        public int GrantedByUserId { get; set; }
    }
} 