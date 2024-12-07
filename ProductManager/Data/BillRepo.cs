using ProductManager.Entities;
using ProductManager.Interfaces;
using ProductManager.Services;
using Microsoft.EntityFrameworkCore;

namespace ProductManager.Data;

public class BillRepo(DatabaseService dbService) : IBillRepo
{
    public async Task AddBillAsync(Bill bill)
    {
        var isIdExist = await dbService.AppDbContext.Bills
            .AsNoTracking()
            .AnyAsync(b => b.Id == bill.Id);
        if (isIdExist)
        {
            throw new Exception("Bill Id is already exist");
        }

        await dbService.AppDbContext.Bills.AddAsync(bill);
        await dbService.AppDbContext.SaveChangesAsync();
    }

    public async Task DeleteBillAsync(Bill bill)
    {
        dbService.AppDbContext.Remove(bill);
        await dbService.AppDbContext.SaveChangesAsync();
    }

    public async Task<IEnumerable<Bill>> GetBillsOfStoreAsync(string storeId)
    {
        if (storeId == null)
        {
            throw new ArgumentNullException(nameof(storeId));
        }   

        return await dbService.AppDbContext.Bills
            .Where(b => b.StoreId == storeId)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task UpdateBillAsync(Bill bill)
    {
        var existingBill = await dbService.AppDbContext.Bills
            .FirstOrDefaultAsync(b => b.Id == bill.Id);

        if (existingBill == null)
            throw new Exception("Bill not found");

        existingBill.StoreId = bill.StoreId;
        existingBill.Date = bill.Date;

        await dbService.AppDbContext.SaveChangesAsync();
    }
}
