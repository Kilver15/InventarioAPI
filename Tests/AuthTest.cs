using EventosSernaJrAPI.Controllers;
using EventosSernaJrAPI.Models;
using EventosSernaJrAPI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Build.Framework;
using Microsoft.Extensions.Logging;
using EventosSernaJrAPI.Models.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Moq;
using FluentAssertions;
namespace Tests
{
    public class AuthTest
    {
        private readonly AuthController _controller;
        private readonly AppDBContext _context;
        private readonly JWTManager _jwtManager;
        private readonly ILogger<AuthController> _logger;

        public AuthTest()
        {
            var options = new DbContextOptionsBuilder<AppDBContext>()
            .UseInMemoryDatabase("TestDb") // Crea una base de datos en memoria para las pruebas
            .Options;

            // Crear instancias reales de las dependencias
            _context = new AppDBContext(options, new JWTManager(new ConfigurationBuilder().Build()));
            _jwtManager = new JWTManager(new ConfigurationBuilder().Build());
            _logger = new LoggerFactory().CreateLogger<AuthController>();

            // Crear el controlador con las dependencias reales
            _controller = new AuthController(_context, _jwtManager, _logger);
        }

        [Fact]
        public async Task Register_ReturnsBadRequest_WhenUserDtoIsNull()
        {
            // Act
            var result = await _controller.Register(null);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task Register_ReturnsOk_WhenUserIsSuccessfullyRegistered()
        {
            // Arrange
            var userDto = new LoginDTO { Username = "testuser", Password = "testpassword" };

            // Encriptar la contraseña usando el JWTManager
            _jwtManager.Setup(m => m.encriptarSHA256(It.IsAny<string>())).Returns("encryptedpassword");

            // Act
            var result = await _controller.Register(userDto);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Login_ReturnsUnauthorized_WhenUserIsNotFound()
        {
            // Arrange
            var userDto = new LoginDTO { Username = "nonexistent", Password = "wrongpassword" };

            // Act
            var result = await _controller.Login(userDto);

            // Assert
            result.Should().BeOfType<UnauthorizedObjectResult>();
        }

        [Fact]
        public async Task Login_ReturnsOk_WhenUserIsAuthenticated()
        {
            // Arrange
            var userDto = new LoginDTO { Username = "testuser", Password = "testpassword" };
            var user = new User { Id = 1, Username = "testuser", Password = "encryptedpassword" };

            // Simular que el usuario fue encontrado en la base de datos
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Mock de la encriptación de la contraseña
            _jwtManager.Setup(m => m.encriptarSHA256(It.IsAny<string>())).Returns("encryptedpassword");

            // Act
            var result = await _controller.Login(userDto);

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            var value = okResult.Value as dynamic;
            value.Token.Should().Be("fake-jwt-token");
        }

        [Fact]
        public async Task Logout_ReturnsUnauthorized_WhenUserIsNotLoggedIn()
        {
            // Arrange
            var controllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
                {
                    User = new System.Security.Claims.ClaimsPrincipal() // No se pasa ningún usuario
                }
            };
            _controller.ControllerContext = controllerContext;

            // Act
            var result = await _controller.Logout();

            // Assert
            result.Should().BeOfType<UnauthorizedObjectResult>();
        }

        [Fact]
        public async Task Logout_ReturnsOk_WhenUserIsLoggedOut()
        {
            // Arrange
            var userClaims = new System.Security.Claims.ClaimsIdentity();
            userClaims.AddClaim(new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "1"));
            var controllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
                {
                    User = new System.Security.Claims.ClaimsPrincipal(userClaims)
                }
            };
            _controller.ControllerContext = controllerContext;

            // Act
            var result = await _controller.Logout();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public void GetUserProfile_ReturnsUnauthorized_WhenUserIsNotAuthenticated()
        {
            // Arrange
            var controllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
                {
                    User = new System.Security.Claims.ClaimsPrincipal() // Sin autenticación
                }
            };
            _controller.ControllerContext = controllerContext;

            // Act
            var result = _controller.GetUserProfile();

            // Assert
            result.Should().BeOfType<UnauthorizedObjectResult>();
        }

        [Fact]
        public void GetUserProfile_ReturnsOk_WhenUserIsAuthenticated()
        {
            // Arrange
            var userClaims = new System.Security.Claims.ClaimsIdentity();
            userClaims.AddClaim(new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "1"));
            userClaims.AddClaim(new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, "testuser"));
            userClaims.AddClaim(new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, "user"));
            var controllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
                {
                    User = new System.Security.Claims.ClaimsPrincipal(userClaims)
                }
            };
            _controller.ControllerContext = controllerContext;

            // Act
            var result = _controller.GetUserProfile();

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            var value = okResult.Value as dynamic;
            value.UserId.Should().Be("1");
            value.Username.Should().Be("testuser");
        }
    }