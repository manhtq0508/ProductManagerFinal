using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ProductManager.Entities;
using ProductManager.Interfaces;
using ProductManager.Messages;

namespace ProductManager.ViewModels;

[QueryProperty(nameof(ProductId), "productId")]
public partial class EditProductViewModel : ObservableObject
{
    private IProductRepo _productRepo;

    [ObservableProperty]
    private Product? productNeedEdit;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ProductNeedEdit))]
    private string? productId;

    public EditProductViewModel(IProductRepo productRepo)
    {
        _productRepo = productRepo;
    }

    partial void OnProductIdChanged(string? value)
    {
        if (value != null)
        {
            LoadProduct(value);
        }
    }

    private async void LoadProduct(string id)
    {
        try
        {
            ProductNeedEdit = await _productRepo.GetProductByIdAsync(id);
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
        }
    }

    [RelayCommand]
    private async Task SaveProduct()
    {
        if (ProductNeedEdit == null)
            return;

        try
        {
            await _productRepo.UpdateProductAsync(ProductNeedEdit);
            WeakReferenceMessenger.Default.Send(new ProductEditedMessage(ProductNeedEdit));
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
