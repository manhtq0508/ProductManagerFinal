using ProductManager.Entities;
using ProductManager.Interfaces;
using ProductManager.Services;
using Microsoft.EntityFrameworkCore;

namespace ProductManager.Data;

public class BillDetailRepo(DatabaseService dbService) : IBillDetailRepo
{
    public async Task AddProductAsync(string billId, string productId, int quantity)
    {
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
            .ToListAsync();
    }

    public async Task UpdateProductQuantityAsync(string billId, string productId, int newQuantity)
    {
        var billDetail = dbService.AppDbContext.BillDetails.Find(billId, productId);
        if (billDetail == null)
        {
            throw new Exception("Product is not found in the bill");
        }

        billDetail.Quantity = newQuantity;
        dbService.AppDbContext.BillDetails.Update(billDetail);
        await dbService.AppDbContext.SaveChangesAsync();
    }
}
