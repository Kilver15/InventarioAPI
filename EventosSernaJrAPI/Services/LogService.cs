using EventosSernaJrAPI.Models;
using System.Security.Claims;

namespace EventosSernaJrAPI.Services
{
    public interface ILogService
    {
        Task AddLogAsync(string action, ClaimsPrincipal user);
    }
    public class LogService : ILogService
    {
        private readonly AppDBContext _context;
        public LogService(AppDBContext context)
        {
            _context = context;
        }
        public async Task AddLogAsync(string action, ClaimsPrincipal user)
        {
            var userIdClaim = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out var userId))
            {
                userId = 0;
            }

            var logEntry = new Log
            {
                Action = action,
                UserId = userId,
                Date = DateTime.UtcNow
            };

            _context.Add(logEntry); 
            await _context.SaveChangesAsync();
        }
    }
}
