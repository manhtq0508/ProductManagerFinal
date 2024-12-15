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
    private ObservableCollection<Product> products = new();

    [ObservableProperty]
    private ObservableCollection<object> selectedProducts = new();

    private ObservableCollection<object> _selectedLog = new();

    [ObservableProperty]
    private string? searchText;
    private ObservableCollection<Product> _allProducts = new();

    public ProductViewModel(IProductRepo productRepo)
    {
        _productRepo = productRepo;
        LoadProducts().Wait();

        // Register to receive messages
        WeakReferenceMessenger.Default.Register<ProductAddedMessage>(this, (r, m) => OnProductAdded(m.newProduct));
        WeakReferenceMessenger.Default.Register<ProductEditedMessage>(this, (r, m) => OnProductEdited(m.editedProduct));
    }

    private void OnProductAdded(Product newProduct)
    {
        _allProducts.Add(newProduct);

        FilterProduct();
    }

    private void OnProductEdited(Product editedProduct)
    {
        _allProducts.Where(p => p.Id == editedProduct.Id).ToList().ForEach(p =>
        {
            p.Name = editedProduct.Name;
            p.Price = editedProduct.Price;
        });

        FilterProduct();
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
    private async Task AddProduct()
    {
        await Shell.Current.GoToAsync(nameof(AddProductPage));
    }

    [RelayCommand]
    private async Task EditProduct()
    {
        if (SelectedProducts.Count == 0)
        {
            await Shell.Current.DisplayAlert("Error", "Please select a product to edit", "OK");
            return;
        }

        if (SelectedProducts.Count > 1)
        {
            await Shell.Current.DisplayAlert("Error", "Please select only one product to edit", "OK");
            return;
        }

        if (SelectedProducts[0] is Product selectedProduct)
            await Shell.Current.GoToAsync($"{nameof(EditProductPage)}?productId={selectedProduct.Id}");
    }

    [RelayCommand]
    private async Task DeleteProduct()
    {
        if (SelectedProducts.Count == 0)
        {
            await Shell.Current.DisplayAlert("Error", "Please select a product to delete", "OK");
            return;
        }

        var isYes = await Shell.Current.DisplayAlert("Warning", $"Are you sure to delete {SelectedProducts.Count} product(s)?", "Yes", "No");
        if (!isYes)
            return;

        try
        {
            List<Product> products = new List<Product>();
            foreach (var product in SelectedProducts)
            {
                if (product is Product p)
                    products.Add(p);
            }
            SelectedProducts.Clear();
            _selectedLog.Clear();

            await _productRepo.DeleteListProductsAsync(products);

            foreach (var product in products)
            {
                _allProducts.Remove(product);
            }

            FilterProduct();
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
        }
    }
}
