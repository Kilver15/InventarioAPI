using Microsoft.EntityFrameworkCore;

namespace EventosSernaJrAPI.Models
{
    public class AppDBContext : DbContext
    {
        public AppDBContext(DbContextOptions<AppDBContext> options) : base(options)
        {
        }
        DbSet<Product> Products { get; set; }
        DbSet<Category> Categories { get; set; }
        DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>()
                .HasOne<Category>()
                .WithMany()
                .HasForeignKey(p => p.categoryId);
        }
    }
}
