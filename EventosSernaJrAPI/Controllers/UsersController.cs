using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EventosSernaJrAPI.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Drawing.Printing;
using EventosSernaJrAPI.Models.DTOs;
using EventosSernaJrAPI.Services;

namespace EventosSernaJrAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly AppDBContext _context;
        private readonly JWTManager _jwtManager;
        private readonly ILogger<UsersController> _logger;
        private readonly ILogService _logService;

        public UsersController(AppDBContext context, JWTManager jwtManager, ILogger<UsersController> logger, ILogService logService)
        {
            _context = context;
            _jwtManager = jwtManager;
            _logger = logger;
            _logService = logService;
        }

        // GET: api/Users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers(int page = 1, int pageSize = 10)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (role != "Admin")
            {
                return Forbid("You don't have permission to access this resource.");
            }

            var users = await _context.Users
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new
            {
                Data = users,
                TotalCount = await _context.Users.CountAsync(),
                Page = page,
                PageSize = pageSize
            });
        }

        // GET: api/Users/5
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (role != "Admin")
            {
                return Forbid("You don't have permission to access this resource.");
            }
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            return user;
        }

        // PUT: api/Users/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(int id, LoginDTO userdto)
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

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            user.Username = userdto.Username;
            user.Password = _jwtManager.encriptarSHA256(userdto.Password);
            user.UpdatedAt = DateTime.Now;
            _context.Entry(user).State = EntityState.Modified;

            _logger.LogInformation($"User #{user.Id} updated by {User.FindFirst(ClaimTypes.Name)?.Value}.");
            await _logService.AddLogAsync($"User #{user.Id} updated.",User);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Users
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<User>> PostUser(User user)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (role != "Admin")
            {
                return Forbid("You don't have permission to access this resource.");
            }

            user.CreatedAt = DateTime.Now;
            user.UpdatedAt = DateTime.Now;
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            await _logService.AddLogAsync($"User #{user.Id} created.",User);
            _logger.LogInformation($"User #{user.Id} created by {User.FindFirst(ClaimTypes.Name)?.Value}.");
            return CreatedAtAction("GetUser", new { id = user.Id }, user);
        }

        // PUT: api/Users/toggle-activation/5
        [HttpPatch("toggle-activation/{id}")]
        public async Task<IActionResult> ToggleProductActivation(int id, [FromQuery] bool isActive)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (role != "Admin")
            {
                return Forbid("You don't have permission to access this resource.");
            }
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            user.IsActive = isActive;
            user.UpdatedAt = DateTime.Now;
            _context.Entry(user).State = EntityState.Modified;

            await _context.SaveChangesAsync();

            await _logService.AddLogAsync($"User #{user.Id} {(isActive ? "activated" : "deactivated")}.",User);
            _logger.LogInformation($"User #{user.Id} {(isActive ? "activated" : "deactivated")} by {User.FindFirst(ClaimTypes.Name)?.Value}.");
            return Ok(new
            {
                Message = isActive ? "User reactivated correctly." : "User deactivated correctly."
            });
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}
