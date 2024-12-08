using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProductManager.CombineData;
using ProductManager.Entities;
using ProductManager.Interfaces;
using System.Collections.ObjectModel;

namespace ProductManager.ViewModels;

[QueryProperty(nameof(BillId), "billId")]
public partial class BillDetailViewModel : ObservableObject
{
    private IBillRepo _billRepo;
    private IBillDetailRepo _billDetailRepo;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DetailOfBill))]
    private string? billId;

    [ObservableProperty]
    private Bill? detailOfBill;

    [ObservableProperty]
    private long total;

    [ObservableProperty]
    private ObservableCollection<ProductInBill>? products;


    public BillDetailViewModel(IBillRepo billRepo, IBillDetailRepo billDetailRepo)
    {
        _billRepo = billRepo;
        _billDetailRepo = billDetailRepo;
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

        DetailOfBill = await _billRepo.GetBillByIdAsync(BillId);
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
    private async Task Cancel()
    {
        await Shell.Current.GoToAsync("..");
    }
}
