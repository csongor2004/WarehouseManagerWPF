using Microsoft.EntityFrameworkCore;
using WarehouseManager.Models;

namespace WarehouseManager.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            
            optionsBuilder.UseSqlite("Data Source=warehouse.db");
        }
    }
}