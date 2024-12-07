using EventosSernaJrAPI.Models.DTOs;
using EventosSernaJrAPI.Services;
using Microsoft.Data.SqlClient;
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
        public DbSet<Log> Logs { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderProduct> OrderProducts { get; set; }

        public async Task<List<Product>> GetProductsByCategoryAsync(int categoryId, int page = 1, int pageSize = 10)
        {
            var products = await this.Products
                .FromSqlInterpolated($"EXEC GetProductsByCategory @CategoryId = {categoryId}, @Page = {page}, @PageSize = {pageSize}")
                .ToListAsync();

            return products;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId);

            modelBuilder.Entity<OrderProduct>()
                .HasKey(op => new { op.OrderId, op.ProductId });

            modelBuilder.Entity<OrderProduct>()
                .HasOne(op => op.Order)
                .WithMany(o => o.OrderProducts)
                .HasForeignKey(op => op.OrderId);

            modelBuilder.Entity<OrderProduct>()
                .HasOne(op => op.Product)
                .WithMany(p => p.OrderProducts)
                .HasForeignKey(op => op.ProductId);

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

