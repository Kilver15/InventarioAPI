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
using EventosSernaJrAPI.Services;
using System.Data;

namespace EventosSernaJrAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly AppDBContext _context;
        private readonly ILogService _logService;
        private readonly ILogger<CategoriesController> _logger;

        public CategoriesController(AppDBContext context, ILogService logService, ILogger<CategoriesController> logger)
        {
            _context = context;
            _logService = logService;
            _logger = logger;
        }

        // GET: api/Categories
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Category>>> GetCategories(bool isActive = true, int page = 1, int pageSize = 10)
        {
            if (!isActive)
            {
                var role = User.FindFirst(ClaimTypes.Role)?.Value;

                if (role != "Admin")
                {
                    return Forbid("You don't have permission to access this resource.");
                }
            }

            if (page < 1 || pageSize < 1)
            {
                return BadRequest(new
                {
                    Message = "The page and page size values ​​must be greater than 0."
                });
            }

            int totalCount = await _context.Categories.CountAsync(p => p.IsActive == isActive);
            if (page > (totalCount + pageSize - 1) / pageSize)
            {
                return BadRequest(new
                {
                    Message = "The requested page does not exist."
                });
            }

            var categories = await _context.Categories
                .Where(p => p.IsActive == isActive)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new
            {
                Data = categories,
                TotalCount = totalCount,
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
                return Forbid("You don't have permission to access this resource.");
            }
            if (page < 1 || pageSize < 1)
            {
                return BadRequest(new
                {
                    Message = "The page and page size values ​​must be greater than 0."
                });
            }
            int TotalCount = await _context.Categories.CountAsync();
            if (page > TotalCount / pageSize + 1)
            {
                return BadRequest(new
                {
                    Message = "The requested page does not exist."
                });
            }

            var categories = await _context.Categories
            .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new
            {
                Data = categories,
                TotalCount,
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
                return Forbid("You don't have permission to access this resource.");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    Message = "Invalid data.",
                    Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });
            }

            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            category.Name = categoryDTO.Name;
            category.UpdatedAt = DateTime.Now;
            _context.Entry(category).State = EntityState.Modified;

            await _logService.AddLogAsync($"ModifiedCategory {category.Name}, with ID {category.Id}", User);
            _logger.LogInformation($"Category {category.Name} with ID {category.Id} was modified.");

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
                Message = "Data updated correctly."
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
                    return Forbid("You don't have permission to access this resource.");
                }
                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        Message = "Invalid data.",
                        Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                    });
                }

                var category = new Category
                {
                    Name = categorydto.Name,
                    IsActive = true,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
                _context.Categories.Add(category);
                await _context.SaveChangesAsync();

                await _logService.AddLogAsync($"CreatedCategory {category.Name}, with ID {category.Id}", User);
                _logger.LogInformation($"Category {category.Name} with ID {category.Id} was created.");

                var response = new
                {
                    Message = "The category was created successfully.",
                    Data = category
                };

                return CreatedAtAction("GetCategory", new { id = category.Id }, response);
            } catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Message = "An unexpected error occurred.",
                    Details = ex.Message
                });
            }
            
        }

        // PUT: api/Categories/toggle-activation/5
        [HttpPatch("toggle-activation/{id}")]
        public async Task<IActionResult> ToggleProductActivation(int id, [FromQuery] bool isActive)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (role != "Admin")
            {
                return Forbid("You don't have permission to access this resource.");
            }

            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            category.IsActive = isActive;
            category.UpdatedAt = DateTime.Now;
            _context.Entry(category).State = EntityState.Modified;

            await _logService.AddLogAsync($"User #{category.Id} {(isActive ? "activated" : "deactivated")}.", User);
            _logger.LogInformation($"User #{category.Id} {(isActive ? "activated" : "deactivated")} by {User.FindFirst(ClaimTypes.Name)?.Value}.");

            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = isActive ? "Category reactivated correctly." : "Category deactivated correctly."
            });
        }

        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.Id == id);
        }
    }
}
