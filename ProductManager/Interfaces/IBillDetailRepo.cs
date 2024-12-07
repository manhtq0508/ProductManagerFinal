using ProductManager.Entities;

namespace ProductManager.Interfaces;

public interface IBillDetailRepo
{
    Task<IEnumerable<BillDetail>> GetBillDetailsOfBillAsync(string billId);
    Task AddProductAsync(string billId, string productId, int quantity);
    Task UpdateProductQuantityAsync(string billId, string productId, int newQuantity);
    Task DeleteProductAsync(string billId, string productId);
}
