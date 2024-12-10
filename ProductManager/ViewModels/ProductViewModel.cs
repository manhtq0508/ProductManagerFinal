using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ProductManager.Entities;
using ProductManager.Interfaces;
using ProductManager.Messages;
using ProductManager.Views;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;

namespace ProductManager.ViewModels;

public partial class ProductViewModel : ObservableObject
{
    private IProductRepo _productRepo;

    [ObservableProperty]
    private ObservableCollection<Product>? products;

    [ObservableProperty]
    private Product? selectedProduct;

    [ObservableProperty]
    private string? searchText;
    private ObservableCollection<Product>? _allProducts;

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
        if (_allProducts == null)
            _allProducts = new ObservableCollection<Product>();

        _allProducts.Add(newProduct);

        FilterProduct();
    }

    private async Task OnProductEdited(Product editedProduct)
    {
        await LoadProducts();
    }

    private async Task LoadProducts()
    {
        _allProducts = new ObservableCollection<Product>(await _productRepo.GetProductsAsync());

        FilterProduct();
    }

    partial void OnSearchTextChanged(string? value)
    {
        FilterProduct();
    }

    private void FilterProduct()
    {
        if (_allProducts == null)
            return;

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
