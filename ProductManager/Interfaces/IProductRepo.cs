using ProductManager.Entities;

namespace ProductManager.Interfaces;

public interface IProductRepo
{
    Task<IEnumerable<Product>> GetProductsAsync();
    Task AddProductAsync(Product product);
    Task UpdateProductAsync(Product product);
    Task DeleteProductAsync(Product product);
}
