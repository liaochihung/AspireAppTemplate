using AspireAppTemplate.Shared;
using Microsoft.EntityFrameworkCore;

namespace AspireAppTemplate.ApiService.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Seed data
        modelBuilder.Entity<Product>().HasData(
            new Product { Id = 1, Name = "Sample Product 1", Price = 9.99m, Description = "Demo product 1" },
            new Product { Id = 2, Name = "Sample Product 2", Price = 19.99m, Description = "Demo product 2" }
        );
    }
}
