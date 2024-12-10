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
    private ObservableCollection<Store>? stores;

    [ObservableProperty]
    private Store? selectedStore;

    [ObservableProperty]
    private string? searchText;
    private ObservableCollection<Store>? _allStore;

    [ObservableProperty]
    private long? revenue;

    public StoreViewModel(IStoreRepo storeRepo, IBillDetailRepo billDetailRepo)
    {
        _storeRepo = storeRepo;
        _billDetailRepo = billDetailRepo;

        LoadStoresAsync().Wait();
        CalculateRevenue().Wait();

        // Register to receive messages
        WeakReferenceMessenger.Default.Register<StoreAddedMessage>(this, (r, m) => OnStoreAdded(m.newStore));
        WeakReferenceMessenger.Default.Register<StoreEditedMessage>(this, async (r, m) => await LoadStoresAsync());
        // Update revenue when a billNeedEdit is added or edited
        WeakReferenceMessenger.Default.Register<BillChangedMessage>(this, async (r, m) => await CalculateRevenue());
    }

    partial void OnSearchTextChanged(string? value)
    {
        FilterStore();
    }

    private void FilterStore()
    {
        if (_allStore == null)
            return;

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
        if (_allStore == null)
            _allStore = new ObservableCollection<Store>();

        _allStore.Add(newStore);

        FilterStore();
    }

    [RelayCommand]
    private async Task AddStore()
    {
        await Shell.Current.GoToAsync(nameof(AddStorePage));
    }

    [RelayCommand]
    private async Task EditStore()
    {
        if (SelectedStore == null)
        {
            await Shell.Current.DisplayAlert("Error", "Please select a store to edit", "OK");
            return;
        }

        await Shell.Current.GoToAsync($"{nameof(EditStorePage)}?storeId={SelectedStore.Id}");
    }

    [RelayCommand]
    private async Task DeleteStore()
    {
        if (SelectedStore == null)
        {
            await Shell.Current.DisplayAlert("Error", "Please select a store to delete", "OK");
            return;
        }

        try
        {
            if (Stores == null)
                return;

            await _storeRepo.DeleteStoreAsync(SelectedStore);
            Stores.Remove(SelectedStore);
            await CalculateRevenue();
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
        }
    }

    [RelayCommand]
    private async Task ViewBills()
    {
        if (SelectedStore == null)
        {
            await Shell.Current.DisplayAlert("Error", "Please select the store", "OK");
            return;
        }

        await Shell.Current.GoToAsync($"{nameof(BillPage)}?storeId={SelectedStore?.Id}");
    }
}
