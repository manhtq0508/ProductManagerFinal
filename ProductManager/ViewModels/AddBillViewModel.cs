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
    private DateOnly billDate;

    [ObservableProperty]
    private TimeSpan billTime;

    [ObservableProperty]
    private ObservableCollection<ProductInBill>? products;

    [ObservableProperty]
    private ProductInBill? selectedProduct;

    [ObservableProperty]
    private long total;

    public AddBillViewModel(IBillRepo billRepo, IBillDetailRepo billDetailRepo)
    {
        _billRepo = billRepo;
        _billDetailRepo = billDetailRepo;

        BillDate = DateOnly.FromDateTime(DateTime.Now);
        BillTime = new TimeSpan(hours: DateTime.Now.Hour, minutes: DateTime.Now.Minute, seconds: DateTime.Now.Second);

        WeakReferenceMessenger.Default.Register<ProductInBillSelectedMessage>(this, (r, m) => AddProductToBill(m.productInBill));
        WeakReferenceMessenger.Default.Register<QuantityChangedMessage>(this, (r, m) => CalculateTotal());
    }

    private void AddProductToBill(ProductInBill productInBill)
    {
        if (Products == null)
            Products = new ObservableCollection<ProductInBill>();

        var productExist = Products.FirstOrDefault(p => p.Id == productInBill.Id);

        if (productExist != null)
            productExist.Quantity++;
        else
            Products.Add(productInBill);

        CalculateTotal();
    }

    public void CalculateTotal()
    {
        if (Products == null)
            return;

        Total = (long)Products.Sum(p => p.Total);
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

        try
        {
            var newBill = new Bill
            {
                Id = BillId,
                Date = new DateTime(BillDate, TimeOnly.FromTimeSpan(BillTime)),
                StoreId = CurrentStoreId
            };

            // Create bill
            await _billRepo.AddBillAsync(newBill);

            // Create bill details
            List<BillDetail> list = new List<BillDetail>();
            foreach (var product in Products)
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
            return;
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
        if (SelectedProduct == null)
        {
            await Shell.Current.DisplayAlert("Error", "Please select a product to remove", "OK");
            return;
        }

        if (Products == null)
            return;

        Products.Remove(SelectedProduct);
        CalculateTotal();
    }
}
