using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EventosSernaJrAPI.Models;
using EventosSernaJrAPI.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Drawing.Printing;

namespace EventosSernaJrAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly AppDBContext _context;

        public CategoriesController(AppDBContext context)
        {
            _context = context;
        }

        // GET: api/Categories
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Category>>> GetCategories(int page = 1, int pageSize = 10)
        {
            var categories = await _context.Categories
                .Where(c => c.IsActive == true)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new
            {
                Data = categories,
                TotalCount = await _context.Categories.CountAsync(),
                Page = page,
                PageSize = pageSize
            });
        }

        // GET: api/Categories/all
        [HttpGet("all")]
        public async Task<ActionResult<IEnumerable<Category>>> GetAllCategories(int page = 1, int pageSize = 10)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            if (role != "Admin")
            {
                return Forbid("No tienes permiso para acceder a este recurso.");
            }

            var categories = await _context.Categories
                .Where(c => c.IsActive == true)
            .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new
            {
                Data = categories,
                TotalCount = await _context.Categories.CountAsync(),
                Page = page,
                PageSize = pageSize
            });
        }

        // GET: api/Categories/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Category>> GetCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);

            if (category == null)
            {
                return NotFound();
            }

            return category;
        }

        // PUT: api/Categories/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCategory(int id, CategoryDTO categoryDTO)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (role != "Admin")
            {
                return Forbid("No tienes permiso para acceder a este recurso.");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    Message = "Datos inválidos.",
                    Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });
            }

            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            category.Name = categoryDTO.Name;
            _context.Entry(category).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CategoryExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Ok( new
            {
                Message = "Datos actualizados correctamente."
            });
        }

        // POST: api/Categories
        [HttpPost]
        public async Task<ActionResult<Category>> PostCategory(CategoryDTO categorydto)
        {
            try
            {
                var role = User.FindFirst(ClaimTypes.Role)?.Value;

                if (role != "Admin")
                {
                    return Forbid("No tienes permiso para acceder a este recurso.");
                }
                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        Message = "Datos inválidos.",
                        Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                    });
                }

                var category = new Category
                {
                    Name = categorydto.Name,
                    IsActive = true
                };
                _context.Categories.Add(category);
                await _context.SaveChangesAsync();

                var response = new
                {
                    Message = "La categoría se creó correctamente.",
                    Data = category
                };

                return CreatedAtAction("GetCategory", new { id = category.Id }, response);
            } catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Message = "Ocurrió un error inesperado.",
                    Details = ex.Message
                });
            }
            
        }

        // PUT: api/Categories/toggle-activation/5
        [HttpPut("toggle-activation/{id}")]
        public async Task<IActionResult> ToggleProductActivation(int id, [FromQuery] bool isActive)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (role != "Admin")
            {
                return Forbid("No tienes permiso para acceder a este recurso.");
            }

            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            // Actualizar el estado de activación
            category.IsActive = isActive;
            _context.Entry(category).State = EntityState.Modified;

            // Guardar cambios
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = isActive ? "Categoria reactivada correctamente." : "Categoria desactivada correctamente."
            });
        }

        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.Id == id);
        }
    }
}
