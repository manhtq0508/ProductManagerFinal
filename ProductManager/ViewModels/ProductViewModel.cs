using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ProductManager.Entities;
using ProductManager.Interfaces;
using ProductManager.Messages;
using ProductManager.Views;
using System.Collections.ObjectModel;

namespace ProductManager.ViewModels;

public partial class ProductViewModel : ObservableObject
{
    private IProductRepo _productRepo;

    [ObservableProperty]
    private ObservableCollection<Product>? products;

    [ObservableProperty]
    private Product? selectedProduct;

    public ProductViewModel(IProductRepo productRepo)
    {
        _productRepo = productRepo;
        LoadProducts().Wait();

        // Register to receive messages
        WeakReferenceMessenger.Default.Register<ProductAddedMessage>(this, (r, m) => OnProductAdded(m.newProduct));
        WeakReferenceMessenger.Default.Register<ProductEditedMessage>(this, async (r, m) => await OnProductEdited(m.editedProduct));
    }

    private void OnProductAdded(Product newProduct)
    {
        if (Products == null)
            Products = new ObservableCollection<Product>();

        Products.Add(newProduct);
    }

    private async Task OnProductEdited(Product editedProduct)
    {
        await LoadProducts();
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
        await Shell.Current.GoToAsync(nameof(AddProductPage));
    }

    [RelayCommand]
    private async Task EditProduct()
    {
        if (SelectedProduct == null)
        {
            await Shell.Current.DisplayAlert("Error", "Please select a product to edit", "OK");
            return;
        }

        await Shell.Current.GoToAsync($"{nameof(EditProductPage)}?productId={SelectedProduct.Id}");
    }

    [RelayCommand]
    private async Task DeleteProduct()
    {
        if (SelectedProduct == null)
        {
            await Shell.Current.DisplayAlert("Error", "Please select a product to delete", "OK");
            return;
        }

        try
        {
            if (Products == null)
                return;

            await _productRepo.DeleteProductAsync(SelectedProduct);
            Products.Remove(SelectedProduct);
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
        }
    }
}
