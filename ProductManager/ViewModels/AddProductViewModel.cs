using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ProductManager.Entities;
using ProductManager.Interfaces;
using ProductManager.Messages;

namespace ProductManager.ViewModels;

public partial class AddProductViewModel : ObservableObject
{
    private IProductRepo _productRepo;

    [ObservableProperty]
    private string? productId;

    [ObservableProperty]
    private string? productName;
    
    [ObservableProperty]
    private string? productPrice;

    public AddProductViewModel(IProductRepo productRepo)
    {
        _productRepo = productRepo;
    }

    [RelayCommand]
    private async Task AddProduct()
    {
        if (string.IsNullOrWhiteSpace(ProductId) || string.IsNullOrWhiteSpace(ProductName) || string.IsNullOrWhiteSpace(ProductPrice))
        {
            await Shell.Current.DisplayAlert("Error", "Missing information", "OK");
            return;
        }

        int price;
        try
        {
            price = int.Parse(ProductPrice);
        }
        catch (Exception)
        {
            await Shell.Current.DisplayAlert("Error", "Price must be a number", "OK");
            return;
        }

        var newProduct = new Product
        {
            Id = ProductId,
            Name = ProductName,
            Price = price
        };

        try
        {
            await _productRepo.AddProductAsync(newProduct);
            WeakReferenceMessenger.Default.Send(new ProductAddedMessage(newProduct));
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
        }

    }

    [RelayCommand]
    private async Task Cancel()
    {
        await Shell.Current.GoToAsync("..");
    }
}
