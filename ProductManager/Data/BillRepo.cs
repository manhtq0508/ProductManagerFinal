using Microsoft.EntityFrameworkCore;
using ProductManager.Entities;
using ProductManager.Interfaces;
using ProductManager.Services;

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
            throw new Exception("Bill id already exists");
        }

        await dbService.AppDbContext.Bills.AddAsync(bill);
        await dbService.AppDbContext.SaveChangesAsync();
    }

    public async Task AddListBillsAsync(List<Bill> bills)
    {
        foreach (var bill in bills)
        {
            var isBillExist = await dbService.AppDbContext.Bills
                .AsNoTracking()
                .AnyAsync(b => b.Id == bill.Id);
            if (isBillExist)
                throw new Exception("Bill id already exists");
        }

        await dbService.AppDbContext.Bills.AddRangeAsync(bills);
        await dbService.AppDbContext.SaveChangesAsync();
    }

    public async Task DeleteBillAsync(Bill bill)
    {
        dbService.AppDbContext.Remove(bill);
        await dbService.AppDbContext.SaveChangesAsync();
    }

    public async Task DeleteListBillsAsync(List<Bill> bills)
    {
        foreach (var bill in bills)
        {
            dbService.AppDbContext.Remove(bill);
        }
        await dbService.AppDbContext.SaveChangesAsync();
    }

    public async Task<Bill> GetBillByIdAsync(string billId)
    {
        var bill = await dbService.AppDbContext.Bills
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == billId);

        if (bill == null)
            throw new Exception("Bill not found");

        return bill;
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
        existingBill.CreatedDateTime = bill.CreatedDateTime;

        await dbService.AppDbContext.SaveChangesAsync();
        dbService.AppDbContext.Entry(existingBill).State = EntityState.Detached;
    }
}
