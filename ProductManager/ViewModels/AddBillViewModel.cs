using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ProductManager.CombineData;
using ProductManager.Entities;
using ProductManager.Interfaces;
using ProductManager.Messages;
using ProductManager.Views;
using System.Collections.ObjectModel;

namespace ProductManager.ViewModels;

[QueryProperty(nameof(CurrentStoreId), "storeId")]
public partial class AddBillViewModel : ObservableObject
{
    private IBillRepo _billRepo;
    private IBillDetailRepo _billDetailRepo;

    [ObservableProperty]
    private string? currentStoreId;

    [ObservableProperty]
    private string? billId;

    [ObservableProperty]
    private DateTime billDate = DateTime.Now;

    [ObservableProperty]
    private TimeSpan billTime = DateTime.Now.TimeOfDay;

    [ObservableProperty]
    private ObservableCollection<ProductInBill>? products;

    [ObservableProperty]
    private string? searchText;
    private ObservableCollection<ProductInBill> _allProducts = new ObservableCollection<ProductInBill>();

    [ObservableProperty]
    private ObservableCollection<object> selectedProducts = new();

    [ObservableProperty]
    private long total;

    public AddBillViewModel(IBillRepo billRepo, IBillDetailRepo billDetailRepo)
    {
        _billRepo = billRepo;
        _billDetailRepo = billDetailRepo;

        WeakReferenceMessenger.Default.Register<ProductsInBillSelectedMessage>(this, (r, m) => AddProductsToBill(m.productsInBill));
        WeakReferenceMessenger.Default.Register<QuantityChangedMessage>(this, (r, m) => CalculateTotal());
    }

    private void AddProductsToBill(List<ProductInBill> productsInBill)
    {
        foreach (var product in productsInBill)
        {
            var productExist = _allProducts.FirstOrDefault(p => p.Id == product.Id);

            if (productExist != null)
                productExist.Quantity += 1;
            else
                _allProducts.Add(product);
        }

        CalculateTotal();
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
            Products = new ObservableCollection<ProductInBill>(_allProducts);
            return;
        }

        string keyword = SearchText.ToLower().Trim();

        Products = new ObservableCollection<ProductInBill>(
            _allProducts.Where(p => p.Id.Contains(SearchText) ||
                                    p.Name.ToLower().Contains(keyword))
            );
    }

    public void CalculateTotal()
    {
        Total = (long)_allProducts.Sum(p => p.Total);
    }

    [RelayCommand]
    private async Task AddBill()
    {
        if (string.IsNullOrWhiteSpace(BillId))
        {
            await Shell.Current.DisplayAlert("Error", "Missing information", "OK");
            return;
        }

        if (CurrentStoreId == null)
        {
            await Shell.Current.DisplayAlert("Error", "Missing StoreId. Contact Admin", "OK");
            return;
        }

        if (Products == null || Products.Count == 0)
        {
            await Shell.Current.DisplayAlert("Error", "Please add products to the bill", "OK");
            return;
        }

        // Check if bill has products
        var isAllQuantityZero = _allProducts.All(p => p.Quantity == 0);
        if (isAllQuantityZero)
        {
            await Shell.Current.DisplayAlert("Error", "Please add products to the bill", "OK");
            return;
        }

        try
        {
            var createdDateTime = BillDate.Date.Add(BillTime);

            var newBill = new Bill
            {
                Id = BillId,
                CreatedDateTime = createdDateTime,
                StoreId = CurrentStoreId
            };

            // Create billNeedEdit
            await _billRepo.AddBillAsync(newBill);

            // Create billNeedEdit details
            List<BillDetail> list = new List<BillDetail>();
            foreach (var product in _allProducts)
            {
                if (product.Quantity == 0 || product.Id == null)
                    continue;

                list.Add(new BillDetail
                {
                    BillId = BillId,
                    ProductId = product.Id,
                    Quantity = product.Quantity
                });
            }

            await _billDetailRepo.AddListBillDetailAsync(list);

            WeakReferenceMessenger.Default.Send(new BillAddedMessage(newBill));

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

    [RelayCommand]
    private async Task AddProduct()
    {
        await Shell.Current.GoToAsync(nameof(SelectProductInBillPage));
    }

    [RelayCommand]
    private async Task RemoveProduct()
    {
        if (SelectedProducts.Count == 0)
        {
            await Shell.Current.DisplayAlert("Error", "Please select a product to remove", "OK");
            return;
        }

        try
        {
            foreach (var product in SelectedProducts)
            {
                if (product is ProductInBill productInBill)
                    _allProducts.Remove(productInBill);
            }
            SelectedProducts.Clear();

            FilterProduct();
            CalculateTotal();
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
        }
    }
}
