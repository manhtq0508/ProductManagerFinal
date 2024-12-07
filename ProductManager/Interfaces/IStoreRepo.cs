using ProductManager.Entities;

namespace ProductManager.Interfaces;

public interface IStoreRepo
{
    Task<IEnumerable<Store>> GetStoresAsync();
    Task<Store> GetStoreByIdAsync(string id);

    Task AddStoreAsync(Store strore);
    Task UpdateStoreAsync(Store store);
    Task DeleteStoreAsync(Store store);
}
