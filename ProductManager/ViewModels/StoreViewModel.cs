using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ProductManager.Entities;
using ProductManager.Interfaces;
using ProductManager.Messages;
using ProductManager.Views;
using System.Collections.ObjectModel;

namespace ProductManager.ViewModels;

public partial class StoreViewModel : ObservableObject
{
    private IStoreRepo _storeRepo;
    private IBillDetailRepo _billDetailRepo; // To calculate revenue

    [ObservableProperty]
    private ObservableCollection<Store> stores = new();

    [ObservableProperty]
    private ObservableCollection<object> selectedStores = new();

    private ObservableCollection<object> _selectedLog = new();

    [ObservableProperty]
    private string searchText = "";
    private ObservableCollection<Store> _allStore = new();

    [ObservableProperty]
    private long revenue = 0;

    public StoreViewModel(IStoreRepo storeRepo, IBillDetailRepo billDetailRepo)
    {
        _storeRepo = storeRepo;
        _billDetailRepo = billDetailRepo;

        LoadStoresAsync().Wait();
        CalculateRevenue().Wait();

        // Register to receive messages
        WeakReferenceMessenger.Default.Register<StoreAddedMessage>(this, (r, m) => OnStoreAdded(m.newStore));
        WeakReferenceMessenger.Default.Register<StoreEditedMessage>(this, (r, m) => OnStoreEdited(m.storeEdited));
        // Update revenue when a billNeedEdit is added or edited
        WeakReferenceMessenger.Default.Register<BillChangedMessage>(this, async (r, m) => await CalculateRevenue());
    }

    private void OnStoreEdited(Store storeEdited)
    {
        _allStore.Where(s => s.Id == storeEdited.Id).ToList().ForEach(s =>
        {
            s.Name = storeEdited.Name;
            s.Address = storeEdited.Address;
        });

        FilterStore();
    }

    partial void OnSearchTextChanged(string value)
    {
        FilterStore();
    }

    private void FilterStore()
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            Stores = new ObservableCollection<Store>(_allStore);
            return;
        }

        string keyword = SearchText.ToLower().Trim();

        Stores = new ObservableCollection<Store>(
            _allStore.Where(s => s.Id.Contains(SearchText) || 
                            s.Name.ToLower().Contains(keyword) ||
                            s.Address.ToLower().Contains(keyword))
        );
    }

    private async Task CalculateRevenue()
    {
        Revenue = await _billDetailRepo.GetRevenueOfAllStoresAsync();
    }

    private async Task LoadStoresAsync()
    {
        _allStore = new ObservableCollection<Store>(await _storeRepo.GetStoresAsync());

        FilterStore();
    }

    private void OnStoreAdded(Store newStore)
    {
        _allStore.Add(newStore);

        FilterStore();
    }

    [RelayCommand]
    private void ItemTapped(Store store)
    {
        if (store == null)
            return;

        if (_selectedLog.Contains(store))
            _selectedLog.Remove(store);
        else
            _selectedLog.Add(store);

        SelectedStores = new ObservableCollection<object>(_selectedLog);
    }

    [RelayCommand]
    private async Task AddStore()
    {
        await Shell.Current.GoToAsync(nameof(AddStorePage));
    }

    [RelayCommand]
    private async Task EditStore()
    {
        if (SelectedStores.Count == 0)
        {
            await Shell.Current.DisplayAlert("Error", "Please select a store to edit", "OK");
            return;
        }

        if (SelectedStores.Count > 1)
        {
            await Shell.Current.DisplayAlert("Error", "Please select only one store to edit", "OK");
            return;
        }

        if (SelectedStores[0] is Store selectedStore)
            await Shell.Current.GoToAsync($"{nameof(EditStorePage)}?storeId={selectedStore.Id}");
    }

    [RelayCommand]
    private async Task DeleteStore()
    {
        if (SelectedStores.Count == 0)
        {
            await Shell.Current.DisplayAlert("Error", "Please select at least one store to delete", "OK");
            return;
        }

        var isYes = await Shell.Current.DisplayAlert("Warning", $"Are you sure to delete {SelectedStores.Count} store(s)?", "Yes", "No");
        if (!isYes)
            return;

        try
        {
            List<Store> storeList = new List<Store>();
            foreach (var store in SelectedStores)
            {
                if (store is Store s)
                    storeList.Add(s);
            }
            SelectedStores.Clear();
            _selectedLog.Clear();

            await _storeRepo.DeleteListStoresAsync(storeList);

            foreach (var store in storeList)
            {
                _allStore.Remove(store);
            }

            await CalculateRevenue();

            FilterStore();
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
        }
    }

    [RelayCommand]
    private async Task ViewBills()
    {
        if (SelectedStores.Count == 0)
        {
            await Shell.Current.DisplayAlert("Error", "Please select a store to view bills", "OK");
            return;
        }

        if (SelectedStores.Count > 1)
        {
            await Shell.Current.DisplayAlert("Error", "Please select only one store to view bills", "OK");
            return;
        }

        if (SelectedStores[0] is Store selectedStore)
            await Shell.Current.GoToAsync($"{nameof(BillPage)}?storeId={selectedStore.Id}");
        else
            await Shell.Current.DisplayAlert("Error", "Invalid store", "OK");
    }
}
