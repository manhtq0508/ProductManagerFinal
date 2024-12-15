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
    private ObservableCollection<Bill> bills = new();

    [ObservableProperty]
    private ObservableCollection<object> selectedBills = new();

    private ObservableCollection<object> _selectedLog = new();

    [ObservableProperty]
    private string searchText = "";
    private ObservableCollection<Bill> _allBills = new();

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

        WeakReferenceMessenger.Default.Register<BillAddedMessage>(this, async (r, m) => await OnBillAdded(m.newBill));
        WeakReferenceMessenger.Default.Register<BillEditedMessage>(this, async (r, m) => await OnBillEditedAsync(m.billEdited));
    }

    private async Task OnBillAdded(Bill newBill)
    {
        _allBills.Add(newBill);
        await CalculateTotal();
        FilterBills();

        WeakReferenceMessenger.Default.Send(new BillChangedMessage());
    }
    private async Task OnBillEditedAsync(Bill billEdited)
    {
        _allBills.Where(b => b.Id == billEdited.Id).ToList().ForEach(b =>
        {
            b.CreatedDateTime = billEdited.CreatedDateTime;
        });

        await CalculateTotal();

        FilterBills();

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
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            Bills = new ObservableCollection<Bill>(_allBills);
            return;
        }

        Bills = new ObservableCollection<Bill>(_allBills.Where(b => b.Id.Contains(SearchText)));
    }

    private async Task CalculateTotal()
    {
        if (StoreId == null) return;

        Total = await _billDetailRepo.GetRevenueOfStoreByIdAsync(StoreId);
    }

    [RelayCommand]
    private void SelectAll()
    {
        SelectedBills = new ObservableCollection<object>(_allBills);
        _selectedLog = new ObservableCollection<object>(_allBills);
    }

    [RelayCommand]
    private void DeselectAll()
    {
        SelectedBills.Clear();
        _selectedLog.Clear();
    }

    [RelayCommand]
    private void ItemTapped(Bill bill)
    {
        if (bill == null)
            return;

        if (_selectedLog.Contains(bill))
            _selectedLog.Remove(bill);
        else
            _selectedLog.Add(bill);

        SelectedBills = new ObservableCollection<object>(_selectedLog);
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
        if (SelectedBills.Count == 0)
        {
            await Shell.Current.DisplayAlert("Error", "Please select bill to edit", "OK");
            return;
        }

        if (SelectedBills.Count > 1)
        {
            await Shell.Current.DisplayAlert("Error", "Please select only one bill to edit", "OK");
            return;
        }

        if (SelectedBills[0] is Bill selectedBill)
            await Shell.Current.GoToAsync($"{nameof(EditBillPage)}?billId={selectedBill.Id}");
    }

    [RelayCommand]
    private async Task DeleteBill()
    {
        if (SelectedBills.Count == 0)
        {
            await Shell.Current.DisplayAlert("Error", "Please select bill to delete", "OK");
            return;
        }

        var isYes = await Shell.Current.DisplayAlert("Warning", $"Are you sure to delete {SelectedBills.Count} bill(s)?", "Yes", "No");
        if (!isYes)
            return;

        try
        {
            List<Bill> bills = new List<Bill>();
            foreach (var bill in SelectedBills)
            {
                if (bill is Bill b)
                    bills.Add(b);
            }
            SelectedBills.Clear();
            _selectedLog.Clear();

            await _billRepo.DeleteListBillsAsync(bills);
            foreach (var bill in bills)
            {
                _allBills.Remove(bill);
            }
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
        if (SelectedBills.Count == 0)
        {
            await Shell.Current.DisplayAlert("Error", "Please select bill to view detail", "OK");
            return;
        }

        if (SelectedBills.Count > 1)
        {
            await Shell.Current.DisplayAlert("Error", "Please select only one bill to view detail", "OK");
            return;
        }

        if (SelectedBills[0] is Bill selectedBill)
            await Shell.Current.GoToAsync($"{nameof(BillDetailPage)}?billId={selectedBill.Id}");
    }
}
