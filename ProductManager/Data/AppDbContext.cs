using ProductManager.Entities;
using Microsoft.EntityFrameworkCore;

namespace ProductManager.Data;

public class AppDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<Product> Products { get; set; } = null!;
    public DbSet<Store> Stores { get; set; } = null!;
    public DbSet<Bill> Bills { get; set; } = null!;
    public DbSet<BillDetail> BillDetails { get; set; } = null!;
}
