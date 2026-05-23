using Microsoft.EntityFrameworkCore;
using ProductService.Models;

namespace ProductService.Data
{
    public class ProductDbContext : DbContext
    {
        public ProductDbContext(DbContextOptions<ProductDbContext> options)
            : base(options) { }

        public DbSet<Product> Products { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Seed some initial data so the app isn't empty
            modelBuilder.Entity<Product>().HasData(
                new Product { Id = 1, Name = "Laptop", Description = "High performance laptop", Price = 15000, Stock = 10, Category = "Electronics", ImageUrl = "", CreatedAt = DateTime.UtcNow },
                new Product { Id = 2, Name = "Phone", Description = "Latest smartphone", Price = 8000, Stock = 25, Category = "Electronics", ImageUrl = "", CreatedAt = DateTime.UtcNow },
                new Product { Id = 3, Name = "Headphones", Description = "Noise cancelling headphones", Price = 2000, Stock = 50, Category = "Electronics", ImageUrl = "", CreatedAt = DateTime.UtcNow }
            );
        }
    }
}