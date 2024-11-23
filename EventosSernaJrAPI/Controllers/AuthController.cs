using EventosSernaJrAPI.Models;
using EventosSernaJrAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using EventosSernaJrAPI.Models.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EventosSernaJrAPI.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDBContext _context;
        private readonly JWTManager _jwtManager;
        private readonly ILogger<AuthController> _logger;
        public AuthController(AppDBContext appDBContext, JWTManager jWTManager, ILogger<AuthController> logger)
        {
            _context = appDBContext;
            _jwtManager = jWTManager;
            _logger = logger;
        }

        // POST: api/Auth/Register
        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register(LoginDTO userdto)
        {
            if (userdto == null)
            {
                return BadRequest("Invalid request");
            }

            var user = new User
            {
                Username = userdto.Username,
                Password = _jwtManager.encriptarSHA256(userdto.Password),
                IsAdmin = false,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            _logger.LogInformation("New registered user.");

            return Ok("Registered user Successfully.");
        }
        
        // POST: api/Auth/Login
        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login(LoginDTO userdto)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == userdto.Username &&
                u.Password == _jwtManager.encriptarSHA256(userdto.Password));

            if (user == null)
            {
                return Unauthorized("Invalid user.");
            }

            _logger.LogInformation($"User #{user.Id} logged in.");
            return Ok(new
            {
                Message = "Successful authentication",
                Token = _jwtManager.generarJWT(user)
            });

        }

        // POST: api/Auth/Logout
        [HttpPost]
        [Route("Logout")]
        public async Task<IActionResult> Logout()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized();
            }
            _logger.LogInformation($"User #{userId} logged out.");
            return Ok("Logged out successfully.");
        }

        // GET: api/Auth/Profile
        [HttpGet("profile")]
        [Authorize]
        public IActionResult GetUserProfile()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            if (userId == null) {
                return Unauthorized();
            }
            return Ok(new
            {
                UserId = userId,
                Username = username,
                Role = role
            });
        }
    }
}
