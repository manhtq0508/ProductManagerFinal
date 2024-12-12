using ProductManager.CombineData;
using ProductManager.Entities;

namespace ProductManager.Interfaces;

public interface IBillDetailRepo
{
    Task<IEnumerable<BillDetail>> GetBillDetailsOfBillAsync(string billId);
    Task<long> GetRevenueOfAllStoresAsync();
    Task<long> GetRevenueOfStoreByIdAsync(string storeId);
    Task<long> GetTotalOfBillByIdAsync(string billId);
    Task<IEnumerable<ProductInBill>> GetListProductInBillAsync(string billId);
    Task AddBillDetailAsync(BillDetail billDetail);
    Task AddListBillDetailAsync(List<BillDetail> billDetails);
    Task UpdateListBillDetailAsync(List<BillDetail> billDetails);
    Task DeleteProductAsync(string billId, string productId);
    Task DeleteListProductsAsync(string billId, List<ProductInBill> products);
}
