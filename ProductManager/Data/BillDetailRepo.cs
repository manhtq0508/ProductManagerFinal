using ProductManager.Entities;
using ProductManager.Interfaces;
using ProductManager.Services;
using Microsoft.EntityFrameworkCore;
using ProductManager.CombineData;

namespace ProductManager.Data;

public class BillDetailRepo(DatabaseService dbService) : IBillDetailRepo
{
    private async Task<bool> CheckBillDetailValid(BillDetail billDetail)
    {
        var isBillDetailExist = await dbService.AppDbContext.BillDetails
            .AnyAsync(bd => bd.BillId == billDetail.BillId && bd.ProductId == billDetail.ProductId);

        if (isBillDetailExist)
            throw new Exception("Bill detail is existed");

        var isProductExist = await dbService.AppDbContext.Products
            .AnyAsync(p => p.Id == billDetail.ProductId);

        if (!isProductExist)
            throw new Exception("Product does not exist");

        var isBillExist = await dbService.AppDbContext.Bills
            .AnyAsync(b => b.Id == billDetail.BillId);

        if (!isBillExist)
            throw new Exception("Bill does not exist");

        return true;
    }
    public async Task AddListBillDetailAsync(List<BillDetail> billDetails)
    {
        foreach (var billDetail in billDetails)
        {
            if (!await CheckBillDetailValid(billDetail))
                return;
        }

        await dbService.AppDbContext.BillDetails.AddRangeAsync(billDetails);
        await dbService.AppDbContext.SaveChangesAsync();
    }

    public async Task AddBillDetailAsync(BillDetail billDetail)
    {
        if (!await CheckBillDetailValid(billDetail)) { return; }

        await dbService.AppDbContext.BillDetails.AddAsync(billDetail);
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

    public async Task<long> GetRevenueOfAllStoresAsync()
    {
        return await dbService.AppDbContext.BillDetails
            .AsNoTracking()
            .SumAsync(bd => bd.Quantity * bd.Product.Price);
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

    public async Task<long> GetRevenueOfStoreByIdAsync(string storeId)
    {
        return await dbService.AppDbContext.BillDetails
            .Include(bd => bd.Bill)
            .Include(bd => bd.Product)
            .Where(bd => bd.Bill.StoreId == storeId)
            .AsNoTracking()
            .SumAsync(bd => bd.Quantity * bd.Product.Price);
    }

    public async Task<long> GetTotalOfBillByIdAsync(string billId)
    {
        return await dbService.AppDbContext.BillDetails
            .Where(bd => bd.BillId == billId)
            .AsNoTracking()
            .SumAsync(bd => bd.Quantity * bd.Product.Price);
    }

    public async Task<IEnumerable<ProductInBill>> GetListProductInBillAsync(string billId)
    {
        return await dbService.AppDbContext.BillDetails
            .Include(bd => bd.Product)
            .Where(bd => bd.BillId == billId)
            .Select(bd => new ProductInBill
            {
                Id = bd.ProductId,
                Name = bd.Product.Name,
                Price = bd.Product.Price,
                Quantity = bd.Quantity,
            })
            .AsNoTracking()
            .ToListAsync();
    }
}
