using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProductManager.Entities;
using ProductManager.Interfaces;
using System.Collections.ObjectModel;

namespace ProductManager.ViewModels;

[QueryProperty(nameof(StoreId), "storeId")]
public partial class BillViewModel : ObservableObject
{
    private IBillRepo _billRepo;
    private IBillDetailRepo _billDetailRepo;

    [ObservableProperty]
    private ObservableCollection<Bill>? bills;

    [ObservableProperty]
    private Bill? selectedBill;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Bills))]
    private string? storeId;

    [ObservableProperty]
    private long total;

    public BillViewModel(IBillRepo billRepo, IBillDetailRepo billDetailRepo)
    {
        _billRepo = billRepo;
        _billDetailRepo = billDetailRepo;
    }

    partial void OnStoreIdChanged(string? value)
    {
        if (value != null)
        {
            LoadBillsAsync().Wait();
        }
    }

    private async Task LoadBillsAsync()
    {
        if (StoreId == null) return;

        if (Bills == null)
            Bills = new ObservableCollection<Bill>();
        else
            Bills.Clear();

        foreach (var b in await _billRepo.GetBillsOfStoreAsync(StoreId))
        {
            Bills.Add(b);
        }
    }

    [RelayCommand]
    private async Task AddBill()
    {
        
    }

    [RelayCommand]
    private async Task EditBill()
    {

    }

    [RelayCommand]
    private async Task DeleteBill()
    {

    }

    [RelayCommand]
    private async Task DetailOfBill()
    {

    }
}
