using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EventosSernaJrAPI.Models;
using Microsoft.AspNetCore.Authorization;
using EventosSernaJrAPI.Models.DTOs;
using System.Security.Claims;
using OfficeOpenXml;

namespace EventosSernaJrAPI.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly AppDBContext _context;

        public OrdersController(AppDBContext context)
        {
            _context = context;
        }

        // GET: api/Orders
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrders()
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            if (role != "Admin")
            {
                return Forbid("You don't have permission to access this resource.");
            }

            return await _context.Orders.ToListAsync();
        }

        // GET: api/Orders/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Order>> GetOrder(long id)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            if (role != "Admin")
            {
                return Forbid("You don't have permission to access this resource.");
            }

            var order = await _context.Orders.FindAsync(id);

            if (order == null)
            {
                return NotFound();
            }

            return order;
        }

        // POST: api/Orders
        [HttpPost]
        public async Task<ActionResult> CreateOrder(CreateOrderRequest request)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            if (role != "Admin")
            {
                return Forbid("You don't have permission to access this resource.");
            }

            var order = new Order
            {
                CustomerName = request.CustomerName,
                PhoneNumber = request.PhoneNumber,
                Address = request.Address,
                OrderDate = DateTime.Now,
                DeliveryTime = request.DeliveryTime,
                PickupDateTime = request.PickupDateTime,
                OrderProducts = request.Products.Select(p => new OrderProduct
                {
                    ProductId = p.ProductId,
                    Quantity = p.Quantity
                }).ToList()
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            var filePath = @"C:\Users\josei\source\repos\EventosSernaJrAPI\EventosSernaJrAPI\Pedidos.xlsx";

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("El archivo Pedidos.xlsx no existe.");
            }

            var date = order.OrderDate.ToString("yyyy-MM-dd");
            var customerNameInitials = string.Join("", order.CustomerName.Split(' ').Select(word => word[0]));
            var fileName = $"Pedido_{date}_{customerNameInitials}.xlsx";

            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                var worksheet = package.Workbook.Worksheets[0];

                worksheet.Cells["B7"].Value = order.OrderDate.ToString("yyyy-MM-dd HH:mm");
                worksheet.Cells["F7"].Value = order.PhoneNumber;
                worksheet.Cells["B8"].Value = order.CustomerName;
                worksheet.Cells["B10"].Value = order.Address;
                worksheet.Cells["B11"].Value = order.DeliveryTime.ToString(@"hh\:mm");
                worksheet.Cells["F11"].Value = order.PickupDateTime.ToString("yyyy-MM-dd HH:mm");

                int startRow = 13;
                for (int i = 0; i < order.OrderProducts.Count; i++)
                {
                    var product = order.OrderProducts[i];

                    var productName = await _context.Products
                        .Where(p => p.Id == product.ProductId)
                        .Select(p => p.Name)
                        .FirstOrDefaultAsync();

                    worksheet.Cells[$"A{startRow + i}"].Value = product.Quantity;
                    worksheet.Cells[$"B{startRow + i}"].Value = productName ?? "Producto no encontrado";
                }

                var memoryStream = new MemoryStream(package.GetAsByteArray());
                return File(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }

        }

        private bool OrderExists(long id)
        {
            return _context.Orders.Any(e => e.Id == id);
        }
    }
}
