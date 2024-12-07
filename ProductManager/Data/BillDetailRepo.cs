using ProductManager.Entities;
using ProductManager.Interfaces;
using ProductManager.Services;
using Microsoft.EntityFrameworkCore;

namespace ProductManager.Data;

public class BillDetailRepo(DatabaseService dbService) : IBillDetailRepo
{
    public async Task AddProductAsync(string billId, string productId, int quantity)
    {
        var isBillExist = await dbService.AppDbContext.Bills
            .AsNoTracking()
            .AnyAsync(b => b.Id == billId);
        if (!isBillExist)
        {
            throw new Exception("Bill is not found");
        }

        var isProductExist = await dbService.AppDbContext.Products
            .AsNoTracking()
            .AnyAsync(p => p.Id == productId);
        if (!isProductExist)
        {
            throw new Exception("Product is not found");
        }

        await dbService.AppDbContext.BillDetails.AddAsync(
            new BillDetail
            {
                BillId = billId,
                ProductId = productId,
                Quantity = quantity
            }
        );

        await dbService.AppDbContext.SaveChangesAsync();
    }

    public async Task DeleteProductAsync(string billId, string productId)
    {
        var billDetail = dbService.AppDbContext.BillDetails.Find(billId, productId);
        if (billDetail == null)
        {
            throw new Exception("Product is not found in the bill");
        }

        dbService.AppDbContext.BillDetails.Remove(billDetail);
        await dbService.AppDbContext.SaveChangesAsync();
    }

    public async Task<IEnumerable<BillDetail>> GetBillDetailsOfBillAsync(string billId)
    {
        return await dbService.AppDbContext.BillDetails
            .Where(bd => bd.BillId == billId)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task UpdateProductQuantityAsync(string billId, string productId, int newQuantity)
    {
        var billDetail = dbService.AppDbContext.BillDetails
            .FirstOrDefault(bd => bd.BillId == billId && bd.ProductId == productId);
        if (billDetail == null)
        {
            throw new Exception("Product is not found in the bill");
        }

        billDetail.Quantity = newQuantity;
        await dbService.AppDbContext.SaveChangesAsync();
    }
}
