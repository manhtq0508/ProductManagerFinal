using ProductManager.Entities;

namespace ProductManager.Interfaces;

public interface IBillDetailRepo
{
    Task<IEnumerable<BillDetail>> GetBillDetailsOfBillAsync(string billId);
    Task<long> GetRevenueOfAllStoresAsync();
    Task<long> GetRevenueOfStoreByIdAsync(string storeId);
    Task<long> GetTotalOfBillByIdAsync(string billId);
    Task AddBillDetailAsync(BillDetail billDetail);
    Task AddListBillDetailAsync(List<BillDetail> billDetails);
    Task UpdateProductQuantityAsync(string billId, string productId, int newQuantity);
    Task DeleteProductAsync(string billId, string productId);
}
