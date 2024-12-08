using ProductManager.Entities;

namespace ProductManager.Interfaces;

public interface IProductRepo
{
    Task<IEnumerable<Product>> GetProductsAsync();
    Task<Product> GetProductByIdAsync(string id);
    Task AddProductAsync(Product product);
    Task AddListProductsAsync(List<Product> products);
    Task UpdateProductAsync(Product product);
    Task DeleteProductAsync(Product product);
}
