using ProductManager.Entities;
using ProductManager.Interfaces;
using ProductManager.Services;
using Microsoft.EntityFrameworkCore;

namespace ProductManager.Data;

public class ProductRepo(DatabaseService dbService) : IProductRepo
{
    public async Task AddProductAsync(Product product)
    {
        var isIdExist = await dbService.AppDbContext.Products.AnyAsync(p => p.Id == product.Id);
        
        if (isIdExist)
        {
            throw new Exception("Product Id is already exist");
        }

        await dbService.AppDbContext.Products.AddAsync(product);
        await dbService.AppDbContext.SaveChangesAsync();
    }

    public async Task DeleteProductAsync(Product product)
    {
        dbService.AppDbContext.Products.Remove(product);
        await dbService.AppDbContext.SaveChangesAsync();
    }

    public async Task<IEnumerable<Product>> GetProductsAsync()
    {
        return await dbService.AppDbContext.Products.ToListAsync();
    }

    public async Task UpdateProductAsync(Product product)
    {
        dbService.AppDbContext.Products.Update(product);
        await dbService.AppDbContext.SaveChangesAsync();
    }
}
