using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProductManager.Entities;
using ProductManager.Interfaces;
using System.Collections.ObjectModel;

namespace ProductManager.ViewModels;

public partial class ProductViewModel : ObservableObject
{
    private IProductRepo _productRepo;

    [ObservableProperty]
    private ObservableCollection<Product>? products;

    public ProductViewModel(IProductRepo productRepo)
    {
        _productRepo = productRepo;
        LoadProducts().Wait();
    }

    private async Task LoadProducts()
    {
        if (Products == null)
            Products = new ObservableCollection<Product>();
        else
            Products.Clear();

        foreach (var p in await _productRepo.GetProductsAsync())
        {
            Products.Add(p);
        }
    }

    [RelayCommand]
    private async Task AddProduct()
    {

    }

    [RelayCommand]
    private async Task EditProduct()
    {

    }

    [RelayCommand]
    private async Task DeleteProduct()
    {

    }
}
