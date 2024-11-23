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
using OfficeOpenXml;
using Microsoft.AspNetCore.Http.HttpResults;

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
            if (page < 1 || pageSize < 1)
            {
                return BadRequest(new
                {
                    Message = "Los valores de página y tamaño de página deben ser mayores a 0."
                });
            }
            int TotalCount = await _context.Products.CountAsync(p => p.IsActive);
            if (page > TotalCount / pageSize + 1)
            {
                return BadRequest(new
                {
                    Message = "La página solicitada no existe."
                });
            }

            var products = await _context.Products
                .Where(p => p.IsActive)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new
            {
                Data = products,
                TotalCount,
                Page = page,
                PageSize = pageSize
            });
        }

        // GET: api/Products/inactive
        [HttpGet("inactive")]
        public async Task<ActionResult<IEnumerable<Product>>> GetInactiveProducts(int page = 1, int pageSize = 10)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            if (role != "Admin")
            {
                return Forbid("No tienes permiso para acceder a este recurso.");
            }
            if (page < 1 || pageSize < 1)
            {
                return BadRequest(new
                {
                    Message = "Los valores de página y tamaño de página deben ser mayores a 0."
                });
            }
            int TotalCount = await _context.Products.CountAsync(p => !p.IsActive);
            if (page > TotalCount / pageSize + 1)
            {
                return BadRequest(new
                {
                    Message = "La página solicitada no existe."
                });
            }

            var products = await _context.Products
                .Where(p => !p.IsActive)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new
            {
                Data = products,
                TotalCount,
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
            if (page < 1 || pageSize < 1)
            {
                return BadRequest(new
                {
                    Message = "Los valores de página y tamaño de página deben ser mayores a 0."
                });
            }
            int TotalCount = await _context.Products.CountAsync();
            if (page > TotalCount / pageSize + 1)
            {
                return BadRequest(new
                {
                    Message = "La página solicitada no existe."
                });
            }

            var products = await _context.Products
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new
            {
                Data = products,
                TotalCount,
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

            product.Name = productdto.Name;
            product.InStock = productdto.InStock;
            product.CategoryId = productdto.categoryId;
            product.UpdatedAt = DateTime.Now;
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
                    InStock = productdto.InStock,
                    CategoryId = productdto.categoryId,
                    IsActive = true,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
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
        [HttpPatch("toggle-activation/{id}")]
        public async Task<IActionResult> ToggleProductActivation(int id, [FromQuery] bool isActive)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (role != "Admin")
            {
                return Forbid("No tienes permiso para acceder a este recurso.");
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            product.IsActive = isActive;
            product.UpdatedAt = DateTime.Now;
            _context.Entry(product).State = EntityState.Modified;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = isActive ? "Producto reactivado correctamente." : "Producto desactivado correctamente."
            });
        }

        // GET: api/Products/by-category/{categoryId}
        [HttpGet("by-category/{categoryId}")]
        public async Task<ActionResult<IEnumerable<Product>>> GetProductsByCategory(int categoryId, int page = 1, int pageSize = 10)
        {
            var products = await _context.Products
                .Where(p => p.CategoryId == categoryId && p.IsActive)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            if (!products.Any())
            {
                return NotFound(new { Message = "No se encontraron productos para esta categoría." });
            }

            return Ok(new
            {
                Data = products,
                TotalCount = await _context.Products.CountAsync(p => p.CategoryId == categoryId),
                Page = page,
                PageSize = pageSize
            });
        }

        // GET: api/Products/low-stock
        [HttpGet("low-stock")]
        public async Task<ActionResult<IEnumerable<Product>>> GetLowStockProducts([FromQuery] int threshold = 10)
        {
            var products = await _context.Products
                .Where(p => p.InStock < threshold && p.IsActive)
                .ToListAsync();

            if (!products.Any())
            {
                return NotFound(new { Message = "No se encontraron productos con bajo stock." });
            }

            return Ok(new
            {
                Data = products,
                Threshold = threshold
            });
        }

        // PUT: api/Products/update-stock
        [HttpPut("update-stock")]
        public async Task<IActionResult> UpdateStock([FromBody] List<UpdateStockDTO> updates)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            if (role != "Admin")
            {
                return Forbid("No tienes permiso para acceder a este recurso.");
            }

            foreach (var update in updates)
            {
                var product = await _context.Products.FindAsync(update.ProductId);
                if (product != null)
                {
                    product.InStock = update.NewStock;
                    product.UpdatedAt = DateTime.Now;
                    _context.Entry(product).State = EntityState.Modified;
                }
            }

            await _context.SaveChangesAsync();

            return Ok(new { Message = "Stock actualizado correctamente." });
        }

        // GET: api/Products/stats
        [HttpGet("stats")]
        public async Task<IActionResult> GetProductStats()
        {
            var totalProducts = await _context.Products.CountAsync();
            var activeProducts = await _context.Products.CountAsync(p => p.IsActive);
            var inactiveProducts = totalProducts - activeProducts;
            var averageStock = await _context.Products.AverageAsync(p => p.InStock);

            return Ok(new
            {
                TotalProducts = totalProducts,
                ActiveProducts = activeProducts,
                InactiveProducts = inactiveProducts,
                AverageStock = averageStock
            });
        }

        // GET: api/Products/export
        [HttpGet("export")]
        public async Task<IActionResult> ExportProducts()
        {
            var products = await _context.Products.Where(p => p.IsActive).ToListAsync();

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Products");

            worksheet.Cells[1, 1].Value = "Id";
            worksheet.Cells[1, 2].Value = "Name";
            worksheet.Cells[1, 3].Value = "InStock";
            worksheet.Cells[1, 4].Value = "CategoryId";

            for (int i = 0; i < products.Count; i++)
            {
                worksheet.Cells[i + 2, 1].Value = products[i].Id;
                worksheet.Cells[i + 2, 2].Value = products[i].Name;
                worksheet.Cells[i + 2, 3].Value = products[i].InStock;
                worksheet.Cells[i + 2, 4].Value = products[i].CategoryId;
            }

            var stream = new MemoryStream(package.GetAsByteArray());

            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Products.xlsx");
        }


        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
    }
}
