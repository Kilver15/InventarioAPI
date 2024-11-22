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
        public AuthController(AppDBContext appDBContext, JWTManager jWTManager)
        {
            _context = appDBContext;
            _jwtManager = jWTManager;
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
                IsAdmin = false
            };

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            return Ok("Usuario registrado Correctamente.");
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
                return Unauthorized("Usuario Invalido");
            }

            return Ok(new
            {
                mensaje = "Autenticación exitosa",
                token = _jwtManager.generarJWT(user)
            });

        }

        /*
        [HttpGet("profile")]
        [Authorize]
        public IActionResult GetUserProfile()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            return Ok(new
            {
                UserId = userId,
                Username = username,
                Role = role
            });
        }
        */
    }
}
