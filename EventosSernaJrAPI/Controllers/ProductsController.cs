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

namespace EventosSernaJrAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly AppDBContext _context;

        public ProductsController(AppDBContext context)
        {
            _context = context;
        }

        // GET: api/Products
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts(int page = 1, int pageSize = 10)
        {
            var products = await _context.Products
                .Where(p => p.IsActive == true)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new
            {
                Data = products,
                TotalCount = await _context.Products.CountAsync(),
                Page = page,
                PageSize = pageSize
            });
        }

        // GET: api/Products/all
        [HttpGet("all")]
        public async Task<ActionResult<IEnumerable<Product>>> GetAllProducts(int page = 1, int pageSize = 10)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            if (role != "Admin")
            {
                return Forbid("No tienes permiso para acceder a este recurso.");
            }

            var products = await _context.Products
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new
            {
                Data = products,
                TotalCount = await _context.Products.CountAsync(),
                Page = page,
                PageSize = pageSize
            });
        }

        // GET: api/Products/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            return product;
        }

        // PUT: api/Products/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduct(int id, ProductDTO productdto)
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

            var product = await _context.Products.FindAsync(id);
            if(product == null)
            {
                return NotFound();
            }

            _context.Entry(product).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Ok(new
            {
                Message = "Datos actualizados correctamente."
            });
        }

        // POST: api/Products
        [HttpPost]
        public async Task<ActionResult<Product>> PostProduct(ProductDTO productdto)
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

                var product = new Product
                {
                    Name = productdto.Name,
                    InInventory = productdto.InInventory,
                    InvReal = productdto.InvReal,
                    CategoryId = productdto.categoryId,
                    IsActive = true
                };

                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                var response = new
                {
                    Message = "El producto se creó correctamente.",
                    Data = product
                };

                return CreatedAtAction("GetProduct", new { id = product.Id }, product);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Message = "Ocurrió un error inesperado.",
                    Details = ex.Message
                });
            }
        }

        // PUT: api/Products/toggle-activation/5
        [HttpPut("toggle-activation/{id}")]
        public async Task<IActionResult> ToggleProductActivation(int id, [FromQuery] bool isActive)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (role != "Admin")
            {
                return Forbid("No tienes permiso para acceder a este recurso.");
            }
            // Buscar el producto por ID
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            // Actualizar el estado de activación
            product.IsActive = isActive;
            _context.Entry(product).State = EntityState.Modified;

            // Guardar cambios
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = isActive ? "Producto activado correctamente." : "Producto desactivado correctamente."
            });
        }


        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
    }
}
