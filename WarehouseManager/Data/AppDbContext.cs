using Microsoft.EntityFrameworkCore;
using WarehouseManager.Models;

namespace WarehouseManager.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            
            optionsBuilder.UseSqlite("Data Source=warehouse.db");
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Elektronika" },
                new Category { Id = 2, Name = "Élelmiszer" }
            );

            modelBuilder.Entity<Product>().HasData(
                new Product { Id = 1, Name = "Laptop", SKU = "LAP-001", StockLevel = 10, Price = 250000, CategoryId = 1 },
                new Product { Id = 2, Name = "Monitor", SKU = "MON-002", StockLevel = 5, Price = 45000, CategoryId = 1 }
            );
        }
    }
}