using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ProductManager.CombineData;
using ProductManager.Entities;
using ProductManager.Interfaces;
using ProductManager.Messages;
using System.Collections.ObjectModel;

namespace ProductManager.ViewModels;

public partial class SelectProductInBillViewModel : ObservableObject
{
    private IProductRepo _productRepo;

    [ObservableProperty]
    private ObservableCollection<Product> products = new();

    [ObservableProperty]
    private string searchText = "";
    private ObservableCollection<Product> _allProducts = new();

    [ObservableProperty]
    private ObservableCollection<object> selectedProducts = new();

    private ObservableCollection<object> _selectedLog = new();

    public SelectProductInBillViewModel(IProductRepo productRepo)
    {
        _productRepo = productRepo;
        LoadProductsAsync().Wait();
    }

    private async Task LoadProductsAsync()
    {
        _allProducts = new(await _productRepo.GetProductsAsync());
        FilterProduct();
    }

    partial void OnSearchTextChanged(string value)
    {
        FilterProduct();
    }

    private void FilterProduct()
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            Products = new ObservableCollection<Product>(_allProducts);
            return;
        }

        string keyword = SearchText.ToLower().Trim();
        Products = new ObservableCollection<Product>(
            _allProducts.Where(p => p.Id.Contains(SearchText) || 
                                    p.Name.ToLower().Contains(keyword))
        );
    }

    [RelayCommand]
    private void ItemTapped(Product product)
    {
        if (product == null)
            return;

        if (_selectedLog.Contains(product))
            _selectedLog.Remove(product);
        else
            _selectedLog.Add(product);

        SelectedProducts = new ObservableCollection<object>(_selectedLog);
    }

    [RelayCommand]
    private async Task SelectProduct()
    {
        List<ProductInBill> productsInBills = new();
        foreach (var p in SelectedProducts)
        {
            if (p is Product product)
            {
                productsInBills.Add(new ProductInBill
                {
                    Id = product.Id,
                    Name = product.Name,
                    Price = product.Price,
                    Quantity = 1
                });

            }
        }

        WeakReferenceMessenger.Default.Send<ProductsInBillSelectedMessage>(new ProductsInBillSelectedMessage(productsInBills));
        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    private async Task Cancel()
    {
        await Shell.Current.GoToAsync("..");
    }
}
