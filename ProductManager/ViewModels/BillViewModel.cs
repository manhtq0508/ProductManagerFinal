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
    private string searchText = "";
    private ObservableCollection<Bill>? _allBills;

    [ObservableProperty]
    private bool isFilterByDateTime = false;

    [ObservableProperty]
    private DateTime fromDate = DateTime.Now;
    [ObservableProperty]
    private TimeSpan fromTime = DateTime.Now.TimeOfDay;

    [ObservableProperty]
    private DateTime toDate = DateTime.Now;
    [ObservableProperty]
    private TimeSpan toTime = DateTime.Now.TimeOfDay;

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
        WeakReferenceMessenger.Default.Register<BillEditedMessage>(this, async (r, m) => await OnBillEditedAsync());
    }

    private void OnBillAdded(Bill newBill)
    {
        if (_allBills == null)
            _allBills = new ObservableCollection<Bill>();

        _allBills.Add(newBill);
        CalculateTotal().Wait();
        FilterBills();

        WeakReferenceMessenger.Default.Send(new BillChangedMessage());
    }
    private async Task OnBillEditedAsync()
    {
        await LoadBillsAsync();
        await CalculateTotal();

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

        _allBills = new ObservableCollection<Bill>(await _billRepo.GetBillsOfStoreAsync(StoreId));

        FilterBills();
    }

    partial void OnSearchTextChanged(string value)
    {
        FilterBills();
    }

    partial void OnFromDateChanged(DateTime value)
    {
        FilterBills();
    }

    partial void OnFromTimeChanged(TimeSpan value)
    {
        FilterBills();
    }

    partial void OnToDateChanged(DateTime value)
    {
        FilterBills();
    }

    partial void OnToTimeChanged(TimeSpan value)
    {
        FilterBills();
    }

    private void FilterBills()
    {
        if (IsFilterByDateTime)
        {
            FilterBillsWithDateTime();
        }
        else
        {
            FilterBillsById();
        }
    }

    private void FilterBillsWithDateTime()
    {
        if (_allBills == null) return;

        var fromDateTime = FromDate.Date.Add(FromTime);
        var toDateTime = ToDate.Date.Add(ToTime);

        if (string.IsNullOrWhiteSpace(SearchText))
        {
            // Only filter by date time
            Bills = new ObservableCollection<Bill>(_allBills.Where(b => b.CreatedDateTime >= fromDateTime && b.CreatedDateTime <= toDateTime));
        }
        else
        {
            // Filter by both id and date time
            Bills = new ObservableCollection<Bill>(
                _allBills.Where(b => b.Id.Contains(SearchText) &&
                                     b.CreatedDateTime >= fromDateTime && b.CreatedDateTime <= toDateTime)
            );
        }
    }

    private void FilterBillsById()
    {
        if (_allBills == null) return;

        if (string.IsNullOrWhiteSpace(SearchText))
        {
            Bills = new ObservableCollection<Bill>(_allBills);
        }

        Bills = new ObservableCollection<Bill>(_allBills.Where(b => b.Id.Contains(SearchText)));
    }

    private async Task CalculateTotal()
    {
        if (StoreId == null) return;

        Total = await _billDetailRepo.GetRevenueOfStoreByIdAsync(StoreId);
    }

    [RelayCommand]
    private void ToggleFilterByDateTime()
    {
        IsFilterByDateTime = !IsFilterByDateTime;
        FilterBills();
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
            if (_allBills == null) return;

            await _billRepo.DeleteBillAsync(SelectedBill);
            _allBills.Remove(SelectedBill);
            await CalculateTotal();
            FilterBills();

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
        if (SelectedBill == null)
        {
            await Shell.Current.DisplayAlert("Error", "Select a bill to view detail", "OK");
            return;
        }

        await Shell.Current.GoToAsync($"{nameof(BillDetailPage)}?billId={SelectedBill.Id}");
    }
}
