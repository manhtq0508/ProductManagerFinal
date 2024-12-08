using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ProductManager.Entities;
using ProductManager.Interfaces;
using ProductManager.Messages;
using ProductManager.Views;
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

        WeakReferenceMessenger.Default.Register<BillAddedMessage>(this, (r, m) => OnBillAdded(m.newBill));
    }

    private void OnBillAdded(Bill newBill)
    {
        if (Bills == null)
            Bills = new ObservableCollection<Bill>();

        Bills.Add(newBill);
        CalculateTotal().Wait();

        WeakReferenceMessenger.Default.Send(new BillChangedMessage());
    }

    partial void OnStoreIdChanged(string? value)
    {
        if (value != null)
        {
            LoadBillsAsync().Wait();
            CalculateTotal().Wait();
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

    private async Task CalculateTotal()
    {
        if (StoreId == null) return;

        Total = await _billDetailRepo.GetRevenueOfStoreByIdAsync(StoreId);
    }

    [RelayCommand]
    private async Task AddBill()
    {
        await Shell.Current.GoToAsync($"{nameof(AddBillPage)}?storeId={StoreId}");
    }

    [RelayCommand]
    private async Task EditBill()
    {
        if (SelectedBill == null)
        {
            await Shell.Current.DisplayAlert("Error", "Please select bill to edit", "OK");
            return;
        }

        await Shell.Current.GoToAsync($"{nameof(EditBillPage)}?billId={SelectedBill.Id}");
    }

    [RelayCommand]
    private async Task DeleteBill()
    {
        if (SelectedBill == null)
        {
            await Shell.Current.DisplayAlert("Error", "Please select bill to delete", "OK");
            return;
        }

        try
        {
            if (Bills == null) return;

            await _billRepo.DeleteBillAsync(SelectedBill);
            Bills.Remove(SelectedBill);
            await CalculateTotal();

            WeakReferenceMessenger.Default.Send(new BillChangedMessage());
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
        }
    }

    [RelayCommand]
    private async Task DetailOfBill()
    {

    }
}
