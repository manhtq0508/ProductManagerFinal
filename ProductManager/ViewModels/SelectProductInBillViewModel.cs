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
    private ObservableCollection<Product>? products;

    [ObservableProperty]
    private Product? selectedProduct;

    public SelectProductInBillViewModel(IProductRepo productRepo)
    {
        _productRepo = productRepo;
        LoadProductsAsync().Wait();
    }

    private async Task LoadProductsAsync()
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
    private async Task SelectProduct()
    {
        if (SelectedProduct == null)
        {
            await Shell.Current.DisplayAlert("Error", "Please select a product", "OK");
            return;
        }

        var productInBill = new ProductInBill
        {
            Id = SelectedProduct.Id,
            Name = SelectedProduct.Name,
            Price = SelectedProduct.Price,
            Quantity = 1
        };

        WeakReferenceMessenger.Default.Send<ProductInBillSelectedMessage>(new ProductInBillSelectedMessage(productInBill));
        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    private async Task Cancel()
    {
        await Shell.Current.GoToAsync("..");
    }
}
