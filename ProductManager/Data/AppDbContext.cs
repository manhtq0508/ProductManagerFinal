using Microsoft.EntityFrameworkCore;
using ProductManager.Entities;

namespace ProductManager.Data;

public class AppDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<Product> Products { get; set; } = null!;
    public DbSet<Store> Stores { get; set; } = null!;
    public DbSet<Bill> Bills { get; set; } = null!;
    public DbSet<BillDetail> BillDetails { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // If delete store, delete all bills of that store
        modelBuilder.Entity<Bill>()
            .HasOne(b => b.Store)
            .WithMany(s => s.Bills)
            .HasForeignKey(b => b.StoreId)
            .OnDelete(DeleteBehavior.Cascade);

        // If delete product, delete all bill details of that product
        modelBuilder.Entity<BillDetail>()
            .HasOne(bd => bd.Product)
            .WithMany(p => p.BillDetails)
            .HasForeignKey(bd => bd.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        // If delete billNeedEdit, delete all billNeedEdit details of that billNeedEdit
        modelBuilder.Entity<BillDetail>()
            .HasOne(bd => bd.Bill)
            .WithMany(b => b.BillDetails)
            .HasForeignKey(bd => bd.BillId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
