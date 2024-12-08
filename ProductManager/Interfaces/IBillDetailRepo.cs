using ProductManager.Entities;

namespace ProductManager.Interfaces;

public interface IBillDetailRepo
{
    Task<IEnumerable<BillDetail>> GetBillDetailsOfBillAsync(string billId);
    Task<long> GetTotalOfAllBillsAsync();
    Task AddBillDetailAsync(BillDetail billDetail);
    Task AddListBillDetailAsync(List<BillDetail> billDetails);
    Task UpdateProductQuantityAsync(string billId, string productId, int newQuantity);
    Task DeleteProductAsync(string billId, string productId);
}
