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

[QueryProperty(nameof(BillId), "billId")]
public partial class EditBillViewModel : ObservableObject
{
    private IBillRepo _billRepo;
    private IBillDetailRepo _billDetailRepo;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(BillNeedEdit))]
    private string? billId;

    [ObservableProperty]
    private Bill? billNeedEdit;

    [ObservableProperty]
    private long total;

    [ObservableProperty]
    private ObservableCollection<ProductInBill>? products;

    [ObservableProperty]
    private ProductInBill? selectedProduct;


    public EditBillViewModel(IBillRepo billRepo, IBillDetailRepo billDetailRepo)
    {
        _billRepo = billRepo;
        _billDetailRepo = billDetailRepo;

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


    partial void OnBillIdChanged(string? value)
    {
        if (value != null)
        {
            LoadBill().Wait();
            LoadProductInBill().Wait();
            CalculateTotal();
        }
    }

    private async Task LoadBill()
    {
        if (BillId == null)
        {
            await Shell.Current.DisplayAlert("Error", "Bill id is null. Contact admin", "OK");
            return;
        }

        BillNeedEdit = await _billRepo.GetBillByIdAsync(BillId);
    }

    private async Task LoadProductInBill()
    {
        if (Products == null)
            Products = new ObservableCollection<ProductInBill>();
        else
            Products.Clear();

        if (BillId == null)
            return;

        var productList = await _billDetailRepo.GetListProductInBillAsync(BillId);

        foreach (var product in productList)
        {
            Products.Add(product);
        }
    }

    private void CalculateTotal()
    {
        if (Products == null)
            Products = new ObservableCollection<ProductInBill>();

        Total = Products.Sum(p => p.Total);
    }

    [RelayCommand]
    private async Task SaveBill()
    {
        if (BillNeedEdit == null)
        {
            await Shell.Current.DisplayAlert("Error", "Bill is null. Contact admin", "OK");
            return;
        }

        if (Products == null || Products.Count == 0)
        {
            await Shell.Current.DisplayAlert("Error", "Please add products to the bill", "OK");
            return;
        }

        try
        {
            // Update bill
            await _billRepo.UpdateBillAsync(BillNeedEdit);

            // Update products in bill
            List<BillDetail> billDetails = new List<BillDetail>();
            foreach (var product in Products)
            {
                if (product.Quantity == 0 || product.Id == null)
                    continue;

                billDetails.Add(new BillDetail
                {
                    BillId = BillNeedEdit.Id,
                    ProductId = product.Id,
                    Quantity = product.Quantity
                });
            }

            await _billDetailRepo.UpdateListBillDetailAsync(billDetails);

            WeakReferenceMessenger.Default.Send(new BillEditedMessage());

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
        await Shell.Current.GoToAsync($"{nameof(SelectProductInBillPage)}?billId={BillId}");
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
