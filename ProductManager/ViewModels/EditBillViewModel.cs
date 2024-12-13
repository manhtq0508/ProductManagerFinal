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
    private DateTime date = DateTime.Now;

    [ObservableProperty]
    private TimeSpan time = DateTime.Now.TimeOfDay;

    [ObservableProperty]
    private long total;

    [ObservableProperty]
    private string searchText = "";
    private ObservableCollection<ProductInBill> _allProducts = new();

    [ObservableProperty]
    private ObservableCollection<ProductInBill>? products;

    private List<ProductInBill> _productsDeleted = new List<ProductInBill>();

    [ObservableProperty]
    private ObservableCollection<object> selectedProducts = new();


    public EditBillViewModel(IBillRepo billRepo, IBillDetailRepo billDetailRepo)
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

            var productDeleted = _productsDeleted.FirstOrDefault(p => p.Id == product.Id);
            if (productDeleted != null)
                _productsDeleted.Remove(productDeleted);
        }

        CalculateTotal();
        FilterProduct();
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
        Date = BillNeedEdit.CreatedDateTime;
        Time = BillNeedEdit.CreatedDateTime.TimeOfDay;
    }

    private async Task LoadProductInBill()
    {
        if (BillId == null)
            return;

        _allProducts = new(await _billDetailRepo.GetListProductInBillAsync(BillId));

        FilterProduct();
    }

    partial void OnSearchTextChanged(string value)
    {
        FilterProduct();
    }

    partial void OnDateChanged(DateTime value)
    {
        if (BillNeedEdit == null)
            return;

        BillNeedEdit.CreatedDateTime = Date;
    }

    partial void OnTimeChanged(TimeSpan value)
    {
        if (BillNeedEdit == null)
            return;

        BillNeedEdit.CreatedDateTime = new DateTime(Date.Year, Date.Month, Date.Day, Time.Hours, Time.Minutes, Time.Seconds);
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

    private void CalculateTotal()
    {
        Total = _allProducts.Sum(p => p.Total);
    }

    [RelayCommand]
    private async Task SaveBill()
    {
        if (BillNeedEdit == null || BillId == null)
        {
            await Shell.Current.DisplayAlert("Error", "Bill is null. Contact admin", "OK");
            return;
        }

        if (_allProducts == null || _allProducts.Count == 0)
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
            // Update bill
            await _billRepo.UpdateBillAsync(BillNeedEdit);

            // Update products in bill
            List<BillDetail> billDetails = new List<BillDetail>();
            foreach (var product in _allProducts)
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

            // Delete products
            await _billDetailRepo.DeleteListProductsAsync(BillId, new(_productsDeleted));

            WeakReferenceMessenger.Default.Send(new BillEditedMessage(BillNeedEdit));

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
        if (SelectedProducts.Count == 0)
        {
            await Shell.Current.DisplayAlert("Error", "Please select products to remove", "OK");
            return;
        }

        foreach (var product in SelectedProducts)
        {

            if (product is ProductInBill productInBill)
            {
                _allProducts.Remove(productInBill);
                _productsDeleted.Add(productInBill);
            }
        }
        SelectedProducts.Clear();

        CalculateTotal();
        FilterProduct();
    }
}
