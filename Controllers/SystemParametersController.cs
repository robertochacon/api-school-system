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
    public class SystemParametersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SystemParametersController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/systemparameters
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetSystemParameters(
            [FromQuery] string? category = null,
            [FromQuery] string? searchTerm = null)
        {
            var query = _context.SystemParameters.Where(sp => sp.IsActive);

            if (!string.IsNullOrEmpty(category))
                query = query.Where(sp => sp.Category.Contains(category));
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(sp => sp.Name.Contains(searchTerm) ||
                                        sp.Description.Contains(searchTerm) ||
                                        sp.Value.Contains(searchTerm));
            }

            var parameters = await query
                .OrderBy(sp => sp.Category)
                .ThenBy(sp => sp.Name)
                .Select(sp => new
                {
                    sp.Id,
                    sp.Name,
                    sp.Description,
                    sp.Value,
                    sp.Category,
                    sp.DataType,
                    sp.IsEditable,
                    sp.IsActive,
                    sp.CreatedAt,
                    sp.UpdatedAt
                })
                .ToListAsync();

            return Ok(parameters);
        }

        // GET: api/systemparameters/5
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetSystemParameter(int id)
        {
            var parameter = await _context.SystemParameters
                .Where(sp => sp.Id == id && sp.IsActive)
                .Select(sp => new
                {
                    sp.Id,
                    sp.Name,
                    sp.Description,
                    sp.Value,
                    sp.Category,
                    sp.DataType,
                    sp.IsEditable,
                    sp.IsActive,
                    sp.CreatedAt,
                    sp.UpdatedAt
                })
                .FirstOrDefaultAsync();

            if (parameter == null)
            {
                return NotFound("Parámetro del sistema no encontrado");
            }

            return Ok(parameter);
        }

        // GET: api/systemparameters/category/{category}
        [HttpGet("category/{category}")]
        public async Task<ActionResult<IEnumerable<object>>> GetParametersByCategory(string category)
        {
            var parameters = await _context.SystemParameters
                .Where(sp => sp.Category == category && sp.IsActive)
                .OrderBy(sp => sp.Name)
                .Select(sp => new
                {
                    sp.Id,
                    sp.Name,
                    sp.Description,
                    sp.Value,
                    sp.DataType,
                    sp.IsEditable
                })
                .ToListAsync();

            return Ok(parameters);
        }

        // GET: api/systemparameters/categories
        [HttpGet("categories")]
        public async Task<ActionResult<IEnumerable<string>>> GetCategories()
        {
            var categories = await _context.SystemParameters
                .Where(sp => sp.IsActive)
                .Select(sp => sp.Category)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();

            return Ok(categories);
        }

        // POST: api/systemparameters
        [HttpPost]
        public async Task<ActionResult<SystemParameter>> CreateSystemParameter(CreateSystemParameterDto dto)
        {
            // Verificar que el nombre sea único
            var existingParameter = await _context.SystemParameters
                .FirstOrDefaultAsync(sp => sp.Name == dto.Name && sp.IsActive);
            if (existingParameter != null)
                return BadRequest("Ya existe un parámetro con este nombre");

            // Validar el valor según el tipo de datos
            if (!ValidateParameterValue(dto.Value, dto.DataType))
                return BadRequest($"El valor no es válido para el tipo de datos {dto.DataType}");

            var parameter = new SystemParameter
            {
                Name = dto.Name,
                Description = dto.Description,
                Value = dto.Value,
                Category = dto.Category,
                DataType = dto.DataType,
                IsEditable = dto.IsEditable,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.SystemParameters.Add(parameter);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetSystemParameter), new { id = parameter.Id }, parameter);
        }

        // PUT: api/systemparameters/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSystemParameter(int id, UpdateSystemParameterDto dto)
        {
            var parameter = await _context.SystemParameters.FindAsync(id);
            if (parameter == null || !parameter.IsActive)
                return NotFound("Parámetro del sistema no encontrado");

            // Verificar que el parámetro sea editable
            if (!parameter.IsEditable)
                return BadRequest("Este parámetro no es editable");

            // Verificar que el nombre sea único (si se está cambiando)
            if (!string.IsNullOrEmpty(dto.Name) && dto.Name != parameter.Name)
            {
                var existingParameter = await _context.SystemParameters
                    .FirstOrDefaultAsync(sp => sp.Name == dto.Name && sp.IsActive);
                if (existingParameter != null)
                    return BadRequest("Ya existe un parámetro con este nombre");
            }

            // Validar el valor según el tipo de datos (si se está cambiando)
            if (!string.IsNullOrEmpty(dto.Value) && dto.Value != parameter.Value)
            {
                var dataType = dto.DataType ?? parameter.DataType;
                if (!ValidateParameterValue(dto.Value, dataType))
                    return BadRequest($"El valor no es válido para el tipo de datos {dataType}");
            }

            // Actualizar campos
            if (!string.IsNullOrEmpty(dto.Name)) parameter.Name = dto.Name;
            if (dto.Description != null) parameter.Description = dto.Description;
            if (!string.IsNullOrEmpty(dto.Value)) parameter.Value = dto.Value;
            if (!string.IsNullOrEmpty(dto.Category)) parameter.Category = dto.Category;
            if (dto.DataType.HasValue) parameter.DataType = dto.DataType.Value;
            if (dto.IsEditable.HasValue) parameter.IsEditable = dto.IsEditable.Value;

            parameter.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/systemparameters/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeactivateSystemParameter(int id)
        {
            var parameter = await _context.SystemParameters.FindAsync(id);
            if (parameter == null || !parameter.IsActive)
                return NotFound("Parámetro del sistema no encontrado");

            // Verificar que el parámetro sea editable
            if (!parameter.IsEditable)
                return BadRequest("Este parámetro no se puede desactivar");

            parameter.IsActive = false;
            parameter.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/systemparameters/value/{name}
        [HttpGet("value/{name}")]
        public async Task<ActionResult<object>> GetParameterValue(string name)
        {
            var parameter = await _context.SystemParameters
                .Where(sp => sp.Name == name && sp.IsActive)
                .Select(sp => new
                {
                    sp.Value,
                    sp.DataType,
                    sp.Description
                })
                .FirstOrDefaultAsync();

            if (parameter == null)
            {
                return NotFound("Parámetro del sistema no encontrado");
            }

            // Convertir el valor al tipo de datos correspondiente
            var convertedValue = ConvertParameterValue(parameter.Value, parameter.DataType);

            return Ok(new
            {
                Value = convertedValue,
                DataType = parameter.DataType,
                Description = parameter.Description
            });
        }

        // PUT: api/systemparameters/value/{name}
        [HttpPut("value/{name}")]
        public async Task<IActionResult> UpdateParameterValue(string name, UpdateParameterValueDto dto)
        {
            var parameter = await _context.SystemParameters
                .FirstOrDefaultAsync(sp => sp.Name == name && sp.IsActive);
            if (parameter == null)
                return NotFound("Parámetro del sistema no encontrado");

            // Verificar que el parámetro sea editable
            if (!parameter.IsEditable)
                return BadRequest("Este parámetro no es editable");

            // Validar el valor según el tipo de datos
            if (!ValidateParameterValue(dto.Value, parameter.DataType))
                return BadRequest($"El valor no es válido para el tipo de datos {parameter.DataType}");

            parameter.Value = dto.Value;
            parameter.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // POST: api/systemparameters/bulk-update
        [HttpPost("bulk-update")]
        public async Task<ActionResult<object>> BulkUpdateParameters(BulkUpdateParametersDto dto)
        {
            var results = new List<object>();
            var errors = new List<string>();

            foreach (var update in dto.Parameters)
            {
                try
                {
                    var parameter = await _context.SystemParameters
                        .FirstOrDefaultAsync(sp => sp.Name == update.Name && sp.IsActive);
                    if (parameter == null)
                    {
                        errors.Add($"Parámetro '{update.Name}' no encontrado");
                        continue;
                    }

                    // Verificar que el parámetro sea editable
                    if (!parameter.IsEditable)
                    {
                        errors.Add($"Parámetro '{update.Name}' no es editable");
                        continue;
                    }

                    // Validar el valor según el tipo de datos
                    if (!ValidateParameterValue(update.Value, parameter.DataType))
                    {
                        errors.Add($"El valor para '{update.Name}' no es válido para el tipo de datos {parameter.DataType}");
                        continue;
                    }

                    parameter.Value = update.Value;
                    parameter.UpdatedAt = DateTime.UtcNow;

                    results.Add(new { Name = update.Name, Status = "Updated" });
                }
                catch (Exception ex)
                {
                    errors.Add($"Error procesando parámetro '{update.Name}': {ex.Message}");
                }
            }

            if (results.Any())
            {
                await _context.SaveChangesAsync();
            }

            return Ok(new
            {
                Updated = results.Count,
                Errors = errors,
                Results = results
            });
        }

        // GET: api/systemparameters/export
        [HttpGet("export")]
        public async Task<ActionResult<object>> ExportParameters()
        {
            var parameters = await _context.SystemParameters
                .Where(sp => sp.IsActive)
                .OrderBy(sp => sp.Category)
                .ThenBy(sp => sp.Name)
                .Select(sp => new
                {
                    sp.Name,
                    sp.Description,
                    sp.Value,
                    sp.Category,
                    sp.DataType,
                    sp.IsEditable,
                    sp.CreatedAt,
                    sp.UpdatedAt
                })
                .ToListAsync();

            var exportData = parameters
                .GroupBy(p => p.Category)
                .Select(g => new
                {
                    Category = g.Key,
                    Parameters = g.ToList()
                })
                .ToList();

            return Ok(new
            {
                ExportDate = DateTime.UtcNow,
                TotalParameters = parameters.Count,
                Categories = exportData
            });
        }

        // POST: api/systemparameters/import
        [HttpPost("import")]
        public async Task<ActionResult<object>> ImportParameters(ImportParametersDto dto)
        {
            var results = new List<object>();
            var errors = new List<string>();

            foreach (var importParam in dto.Parameters)
            {
                try
                {
                    var existingParameter = await _context.SystemParameters
                        .FirstOrDefaultAsync(sp => sp.Name == importParam.Name && sp.IsActive);

                    if (existingParameter != null)
                    {
                        // Actualizar parámetro existente
                        if (existingParameter.IsEditable)
                        {
                            if (!ValidateParameterValue(importParam.Value, importParam.DataType))
                            {
                                errors.Add($"El valor para '{importParam.Name}' no es válido");
                                continue;
                            }

                            existingParameter.Value = importParam.Value;
                            existingParameter.Description = importParam.Description;
                            existingParameter.Category = importParam.Category;
                            existingParameter.UpdatedAt = DateTime.UtcNow;

                            results.Add(new { Name = importParam.Name, Status = "Updated" });
                        }
                        else
                        {
                            errors.Add($"Parámetro '{importParam.Name}' no es editable");
                        }
                    }
                    else
                    {
                        // Crear nuevo parámetro
                        if (!ValidateParameterValue(importParam.Value, importParam.DataType))
                        {
                            errors.Add($"El valor para '{importParam.Name}' no es válido");
                            continue;
                        }

                        var newParameter = new SystemParameter
                        {
                            Name = importParam.Name,
                            Description = importParam.Description,
                            Value = importParam.Value,
                            Category = importParam.Category,
                            DataType = importParam.DataType,
                            IsEditable = importParam.IsEditable,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        };

                        _context.SystemParameters.Add(newParameter);
                        results.Add(new { Name = importParam.Name, Status = "Created" });
                    }
                }
                catch (Exception ex)
                {
                    errors.Add($"Error procesando parámetro '{importParam.Name}': {ex.Message}");
                }
            }

            if (results.Any())
            {
                await _context.SaveChangesAsync();
            }

            return Ok(new
            {
                Processed = results.Count,
                Errors = errors,
                Results = results
            });
        }

        // GET: api/systemparameters/stats
        [HttpGet("stats")]
        public async Task<ActionResult<object>> GetSystemParameterStats()
        {
            var totalParameters = await _context.SystemParameters.CountAsync(sp => sp.IsActive);
            var editableParameters = await _context.SystemParameters.CountAsync(sp => sp.IsEditable && sp.IsActive);
            var nonEditableParameters = await _context.SystemParameters.CountAsync(sp => !sp.IsEditable && sp.IsActive);

            var parametersByCategory = await _context.SystemParameters
                .Where(sp => sp.IsActive)
                .GroupBy(sp => sp.Category)
                .Select(g => new
                {
                    Category = g.Key,
                    Count = g.Count(),
                    EditableCount = g.Count(sp => sp.IsEditable)
                })
                .OrderByDescending(x => x.Count)
                .ToListAsync();

            var parametersByDataType = await _context.SystemParameters
                .Where(sp => sp.IsActive)
                .GroupBy(sp => sp.DataType)
                .Select(g => new
                {
                    DataType = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .ToListAsync();

            return Ok(new
            {
                TotalParameters = totalParameters,
                EditableParameters = editableParameters,
                NonEditableParameters = nonEditableParameters,
                ParametersByCategory = parametersByCategory,
                ParametersByDataType = parametersByDataType
            });
        }

        private bool ValidateParameterValue(string value, Models.ParameterDataType dataType)
        {
            try
            {
                switch (dataType)
                {
                    case Models.ParameterDataType.String:
                        return true; // Cualquier string es válido
                    case Models.ParameterDataType.Integer:
                        return int.TryParse(value, out _);
                    case Models.ParameterDataType.Decimal:
                        return decimal.TryParse(value, out _);
                    case Models.ParameterDataType.Boolean:
                        return bool.TryParse(value, out _);
                    case Models.ParameterDataType.DateTime:
                        return DateTime.TryParse(value, out _);
                    case Models.ParameterDataType.Json:
                        // Validar JSON básico
                        return value.StartsWith("{") || value.StartsWith("[");
                    default:
                        return true;
                }
            }
            catch
            {
                return false;
            }
        }

        private object ConvertParameterValue(string value, Models.ParameterDataType dataType)
        {
            try
            {
                switch (dataType)
                {
                    case Models.ParameterDataType.String:
                        return value;
                    case Models.ParameterDataType.Integer:
                        return int.Parse(value);
                    case Models.ParameterDataType.Decimal:
                        return decimal.Parse(value);
                    case Models.ParameterDataType.Boolean:
                        return bool.Parse(value);
                    case Models.ParameterDataType.DateTime:
                        return DateTime.Parse(value);
                    case Models.ParameterDataType.Json:
                        return value; // Retornar como string para que el cliente lo parse
                    default:
                        return value;
                }
            }
            catch
            {
                return value; // Retornar el valor original si no se puede convertir
            }
        }
    }

    public class CreateSystemParameterDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        [StringLength(1000)]
        public string Value { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Category { get; set; } = string.Empty;

        [Required]
        public Models.ParameterDataType DataType { get; set; }

        public bool IsEditable { get; set; } = true;
    }

    public class UpdateSystemParameterDto
    {
        [StringLength(100)]
        public string? Name { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(1000)]
        public string? Value { get; set; }

        [StringLength(50)]
        public string? Category { get; set; }

        public Models.ParameterDataType? DataType { get; set; }
        public bool? IsEditable { get; set; }
    }

    public class UpdateParameterValueDto
    {
        [Required]
        [StringLength(1000)]
        public string Value { get; set; } = string.Empty;
    }

    public class BulkUpdateParametersDto
    {
        [Required]
        public List<ParameterUpdate> Parameters { get; set; } = new List<ParameterUpdate>();
    }

    public class ParameterUpdate
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        public string Value { get; set; } = string.Empty;
    }

    public class ImportParametersDto
    {
        [Required]
        public List<ImportParameter> Parameters { get; set; } = new List<ImportParameter>();
    }

    public class ImportParameter
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        [StringLength(1000)]
        public string Value { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Category { get; set; } = string.Empty;

        [Required]
        public Models.ParameterDataType DataType { get; set; }

        public bool IsEditable { get; set; } = true;
    }
} 