﻿using ProductManager.Entities;
using ProductManager.Interfaces;
using ProductManager.Services;
using Microsoft.EntityFrameworkCore;

namespace ProductManager.Data;

public class ProductRepo(DatabaseService dbService) : IProductRepo
{
    public async Task AddProductAsync(Product product)
    {
        var isIdExist = await dbService.AppDbContext.Products
            .AsNoTracking()
            .AnyAsync(p => p.Id == product.Id);
        
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
        return await dbService.AppDbContext.Products
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task UpdateProductAsync(Product product)
    {
        var existingProduct = await dbService.AppDbContext.Products
            .FirstOrDefaultAsync(p => p.Id == product.Id);

        if (existingProduct == null)
            throw new Exception("Product not found");

        existingProduct.Name = product.Name;
        existingProduct.Price = product.Price;

        await dbService.AppDbContext.SaveChangesAsync();
    }
}
