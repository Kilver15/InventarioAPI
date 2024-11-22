using EventosSernaJrAPI.Services;
using Microsoft.EntityFrameworkCore;
namespace EventosSernaJrAPI.Models
{
    public class AppDBContext : DbContext
    {
        private readonly JWTManager _jwtManager;
        public AppDBContext(DbContextOptions<AppDBContext> options, JWTManager jwtManager) : base(options)
        {
            _jwtManager = jwtManager;
        }
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId);

            // Seeder
            if (_jwtManager != null)
            {
                modelBuilder.Entity<User>().HasData(
                    new User
                    {
                        Id = 1,
                        Username = "admin",
                        Password = _jwtManager.encriptarSHA256("123456"),
                        IsAdmin = true,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });
            }
        }
    }
}

