using ProductManager.Entities;
using ProductManager.Interfaces;
using ProductManager.Services;
using Microsoft.EntityFrameworkCore;

namespace ProductManager.Data;

public class StoreRepo(DatabaseService dbService) : IStoreRepo
{
    public async Task AddStoreAsync(Store store)
    {
        var isIdExist = await dbService.AppDbContext.Stores.AnyAsync(s => s.Id == store.Id);

        if (isIdExist)
        {
            throw new Exception("Store Id is already exist");
        }

        await dbService.AppDbContext.Stores.AddAsync(store);
        await dbService.AppDbContext.SaveChangesAsync();
    }

    public async Task DeleteStoreAsync(Store store)
    {
        dbService.AppDbContext.Remove(store);
        await dbService.AppDbContext.SaveChangesAsync();
    }

    public async Task<Store> GetStoreByIdAsync(string id)
    {
        var store = await dbService.AppDbContext.Stores.FirstOrDefaultAsync(s => s.Id == id);
        if (store == null)
        {
            throw new Exception("Store not found");
        }

        return store;
    }

    public async Task<IEnumerable<Store>> GetStoresAsync()
    {
        return await dbService.AppDbContext.Stores.ToListAsync();
    }

    public async Task UpdateStoreAsync(Store store)
    {
        dbService.AppDbContext.Stores.Update(store);
        await dbService.AppDbContext.SaveChangesAsync();
    }
}
