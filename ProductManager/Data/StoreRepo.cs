using Microsoft.EntityFrameworkCore;
using ProductManager.Entities;
using ProductManager.Interfaces;
using ProductManager.Services;

namespace ProductManager.Data;

public class StoreRepo(DatabaseService dbService) : IStoreRepo
{
    public async Task AddListStoresAsync(List<Store> stores)
    {
        foreach (var store in stores)
        {
            var isIdExist = await dbService.AppDbContext.Stores
                .AsNoTracking()
                .AnyAsync(s => s.Id == store.Id);

            if (isIdExist)
            {
                throw new Exception("Store id already exists");
            }
        }

        await dbService.AppDbContext.AddRangeAsync(stores);
        await dbService.AppDbContext.SaveChangesAsync();
    }

    public async Task AddStoreAsync(Store store)
    {
        var isIdExist = await dbService.AppDbContext.Stores
            .AsNoTracking()
            .AnyAsync(s => s.Id == store.Id);

        if (isIdExist)
        {
            throw new Exception("Store id already exists");
        }

        await dbService.AppDbContext.Stores.AddAsync(store);
        await dbService.AppDbContext.SaveChangesAsync();
    }

    public async Task DeleteStoreAsync(Store store)
    {
        dbService.AppDbContext.Stores.Attach(store);
        dbService.AppDbContext.Remove(store);
        await dbService.AppDbContext.SaveChangesAsync();
    }

    public async Task<Store> GetStoreByIdAsync(string id)
    {
        var store = await dbService.AppDbContext.Stores
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id);

        if (store == null)
        {
            throw new Exception("Store not found");
        }

        return store;
    }

    public async Task<IEnumerable<Store>> GetStoresAsync()
    {
        return await dbService.AppDbContext.Stores
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task UpdateStoreAsync(Store store)
    {
        var existingStore = await dbService.AppDbContext.Stores
            .FirstOrDefaultAsync(s => s.Id == store.Id);

        if (existingStore == null)
            throw new Exception("Store not found");

        existingStore.Name = store.Name;
        existingStore.Address = store.Address;

        await dbService.AppDbContext.SaveChangesAsync();
    }
}
